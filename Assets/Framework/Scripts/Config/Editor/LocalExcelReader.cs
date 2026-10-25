using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace FrameworkConfig.Editor
{
    internal static class LocalExcelReader
    {
        private const int MaxWorkbooks = 200;
        private const int MaxSheets = 100;
        private const int MaxRows = 50000;
        private const int MaxColumns = 256;
        private const int MaxCells = 1000000;
        private const int MaxFileBytes = 32 * 1024 * 1024;
        private const int MaxArchiveBytes = 128 * 1024 * 1024;
        private const string ChangedMessage = "Local Excel files changed during the update. Finish saving the files, then click Update Config again. Existing configuration was kept.";

        private sealed class SourceChangedException : InvalidOperationException
        {
            public SourceChangedException() : base(ChangedMessage)
            {
            }
        }

        public static async Task<SourceReadResult> ReadFolderAsync(string folderPath,
            CancellationToken cancellation, Action<string, float> progress = null)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
            {
                throw new InvalidOperationException("Select a local folder containing .xlsx configuration files.");
            }

            var folder = Path.GetFullPath(folderPath);
            var initial = ListFiles(folder);
            var result = new SourceReadResult();
            var versions = new Dictionary<string, string>(StringComparer.Ordinal);

            for (var index = 0; index < initial.Length; index++)
            {
                cancellation.ThrowIfCancellationRequested();
                var path = initial[index];
                var id = "local:" + Path.GetFileName(path);
                progress?.Invoke($"Reading Excel ({index + 1}/{initial.Length})...", 0.15f + 0.55f * index / initial.Length);

                try
                {
                    var bytes = await Task.Run(() => ReadBytes(path, cancellation), cancellation);
                    var version = Hash(bytes);
                    versions.Add(path, version);
                    result.ObservedVersions.Add(id, version);
                    var sheets = await Task.Run(() => ReadWorkbook(bytes, id, Path.GetFileName(path), version, cancellation), cancellation);
                    result.Sheets.AddRange(sheets);
                }
                catch (Exception exception) when (IsReadFailure(exception))
                {
                    result.FailedWorkbookIds.Add(id);
                    result.Errors.Add($"Workbook '{Path.GetFileName(path)}': {ReadFailureMessage(exception)}");
                }
            }

            progress?.Invoke("Verifying local Excel files...", 0.75f);
            var final = ListFiles(folder);

            if (!initial.SequenceEqual(final, StringComparer.Ordinal))
            {
                throw new SourceChangedException();
            }

            foreach (var pair in versions)
            {
                cancellation.ThrowIfCancellationRequested();

                try
                {
                    var bytes = await Task.Run(() => ReadBytes(pair.Key, cancellation), cancellation);

                    if (Hash(bytes) != pair.Value)
                    {
                        throw new SourceChangedException();
                    }
                }
                catch (Exception exception) when (IsReadFailure(exception))
                {
                    throw new InvalidOperationException(ChangedMessage, exception);
                }
            }

            if (!initial.SequenceEqual(ListFiles(folder), StringComparer.Ordinal))
            {
                throw new SourceChangedException();
            }

            return result;
        }

        private static string[] ListFiles(string folder)
        {
            ValidatePath(folder);

            if (!Directory.Exists(folder))
            {
                throw new InvalidOperationException("The local Excel folder does not exist. Select an existing folder and try again.");
            }

            var files = Directory.EnumerateFiles(folder, "*", SearchOption.TopDirectoryOnly)
                .Where(path => string.Equals(Path.GetExtension(path), ".xlsx", StringComparison.OrdinalIgnoreCase)
                    && !Path.GetFileName(path).StartsWith("~$", StringComparison.Ordinal))
                .OrderBy(path => path, StringComparer.Ordinal).Take(MaxWorkbooks + 1).ToArray();

            if (files.Length > MaxWorkbooks)
            {
                throw new InvalidOperationException($"The source folder contains more than {MaxWorkbooks} Excel workbooks. Use a smaller configuration folder.");
            }

            return files;
        }

        private static void ValidatePath(string path)
        {
            // Never follow links to another source folder or workbook.
            for (var current = new DirectoryInfo(path); current != null; current = current.Parent)
            {
                if ((File.GetAttributes(current.FullName) & FileAttributes.ReparsePoint) != 0)
                {
                    throw new InvalidOperationException("Linked source paths are not supported. Select the real folder and ordinary .xlsx files.");
                }
            }
        }

        private static byte[] ReadBytes(string path, CancellationToken cancellation)
        {
            ValidatePath(path);
            var before = new FileInfo(path);
            var length = before.Length;
            var modified = before.LastWriteTimeUtc;

            if (length > MaxFileBytes)
            {
                throw new InvalidOperationException("The workbook is larger than 32 MB. Split it into smaller workbooks.");
            }

            using var input = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var output = new MemoryStream();
            var buffer = new byte[8192];
            int count;

            while ((count = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                cancellation.ThrowIfCancellationRequested();

                if (output.Length + count > MaxFileBytes)
                {
                    throw new InvalidOperationException("The workbook is larger than 32 MB. Split it into smaller workbooks.");
                }

                output.Write(buffer, 0, count);
            }

            ValidatePath(path);
            var after = new FileInfo(path);

            if (length != after.Length || modified != after.LastWriteTimeUtc || output.Length != length)
            {
                throw new SourceChangedException();
            }

            return output.ToArray();
        }

        private static string Hash(byte[] bytes)
        {
            using var algorithm = SHA256.Create();
            return BitConverter.ToString(algorithm.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant();
        }

        private static bool IsReadFailure(Exception exception)
        {
            return !(exception is SourceChangedException) && (exception is IOException || exception is InvalidDataException || exception is UnauthorizedAccessException
                || exception is InvalidOperationException || exception is XmlException
                || exception is ArgumentException || exception is NotSupportedException);
        }

        private static string ReadFailureMessage(Exception exception)
        {
            if (exception is InvalidDataException || exception is XmlException || exception is ArgumentException)
            {
                return "The workbook is damaged or has an invalid format. Save it as a new .xlsx file and try again.";
            }

            if (exception is IOException || exception is UnauthorizedAccessException)
            {
                return "Cannot read the workbook. Finish saving it, check file access, then try again.";
            }

            return exception.Message;
        }

        private static List<SheetSnapshot> ReadWorkbook(byte[] bytes, string id, string name,
            string version, CancellationToken cancellation)
        {
            using var memory = new MemoryStream(bytes, false);
            using var archive = new ZipArchive(memory, ZipArchiveMode.Read);
            var entries = new Dictionary<string, ZipArchiveEntry>(StringComparer.Ordinal);
            long totalBytes = 0;

            foreach (var entry in archive.Entries)
            {
                cancellation.ThrowIfCancellationRequested();
                totalBytes += entry.Length;

                if (archive.Entries.Count > 4096 || entry.Length > MaxFileBytes || totalBytes > MaxArchiveBytes)
                {
                    throw new InvalidOperationException("The expanded workbook is too large. Remove unused content or split the workbook.");
                }

                if (!entries.TryAdd(entry.FullName, entry))
                {
                    throw new InvalidOperationException("The workbook contains duplicate ZIP entries. Save it as a new .xlsx file.");
                }
            }

            var rootRelationships = ReadXml(entries, "_rels/.rels", cancellation);
            var workbookLink = rootRelationships.Root?.Elements().SingleOrDefault(element =>
                ((string)element.Attribute("Type"))?.EndsWith("/officeDocument", StringComparison.Ordinal) == true);
            var workbookPath = ResolveTarget("", workbookLink);
            var workbook = ReadXml(entries, workbookPath, cancellation);
            var ns = SpreadsheetNamespace(workbook, "workbook");
            var slash = workbookPath.LastIndexOf('/');
            var directory = slash < 0 ? "" : workbookPath.Substring(0, slash + 1);
            var relationships = ReadXml(entries, directory + "_rels/" + workbookPath.Substring(slash + 1) + ".rels", cancellation)
                .Root?.Elements().ToDictionary(element => (string)element.Attribute("Id") ?? "", StringComparer.Ordinal);
            var sheets = workbook.Root?.Element(ns + "sheets")?.Elements(ns + "sheet").ToArray();

            if (relationships == null || sheets == null || sheets.Length > MaxSheets)
            {
                throw new InvalidOperationException($"The workbook metadata is invalid or contains more than {MaxSheets} sheets.");
            }

            var sharedLink = relationships.Values.SingleOrDefault(element =>
                ((string)element.Attribute("Type"))?.EndsWith("/sharedStrings", StringComparison.Ordinal) == true);
            var strings = new List<string>();

            if (sharedLink != null)
            {
                var shared = ReadXml(entries, ResolveTarget(directory, sharedLink), cancellation);
                var sharedNs = SpreadsheetNamespace(shared, "sst");

                foreach (var item in shared.Root.Elements(sharedNs + "si"))
                {
                    cancellation.ThrowIfCancellationRequested();
                    strings.Add(ReadText(item, sharedNs));

                    if (strings.Count > MaxCells)
                    {
                        throw new InvalidOperationException("The workbook contains too many shared strings. Split it into smaller workbooks.");
                    }
                }
            }

            var snapshots = new List<SheetSnapshot>();
            var sheetIds = new HashSet<int>();

            foreach (var sheet in sheets)
            {
                cancellation.ThrowIfCancellationRequested();

                if (!int.TryParse((string)sheet.Attribute("sheetId"), NumberStyles.None, CultureInfo.InvariantCulture, out var sheetId)
                    || sheetId <= 0 || !sheetIds.Add(sheetId) || sheet.Attribute("name") == null)
                {
                    throw new InvalidOperationException("The workbook has invalid or duplicate sheet identifiers. Save it as a new .xlsx file.");
                }

                var snapshot = new SheetSnapshot
                {
                    WorkbookId = id,
                    WorkbookName = name,
                    Version = version,
                    SheetId = sheetId,
                    Name = (string)sheet.Attribute("name")
                };
                snapshots.Add(snapshot);

                try
                {
                    var relationshipId = sheet.Attributes().FirstOrDefault(attribute => attribute.Name.LocalName == "id")?.Value;

                    if (relationshipId == null || !relationships.TryGetValue(relationshipId, out var link)
                        || !(((string)link.Attribute("Type"))?.EndsWith("/worksheet", StringComparison.Ordinal) ?? false))
                    {
                        throw new InvalidOperationException("Only ordinary worksheet tabs are supported. Copy literal values into a worksheet.");
                    }

                    ReadSheet(ReadXml(entries, ResolveTarget(directory, link), cancellation), strings, snapshot, cancellation);
                }
                catch (Exception exception) when (IsReadFailure(exception))
                {
                    snapshot.Rows.Clear();
                    snapshot.ReadErrors.Add(ReadFailureMessage(exception));
                }
            }

            return snapshots;
        }

        private static string ResolveTarget(string directory, XElement relationship)
        {
            var target = (string)relationship?.Attribute("Target");

            if (string.IsNullOrEmpty(target) || (string)relationship.Attribute("TargetMode") == "External"
                || target.Contains('\\') || target.Contains(':') || target.Contains('?') || target.Contains('#'))
            {
                throw new InvalidOperationException("The workbook contains a missing or external part reference. Save it as a normal .xlsx file.");
            }

            var segments = new List<string>();

            foreach (var part in (target.StartsWith("/", StringComparison.Ordinal) ? target : directory + target).Split('/'))
            {
                if (part.Length == 0 || part == ".")
                {
                    continue;
                }

                if (part == "..")
                {
                    if (segments.Count == 0)
                    {
                        throw new InvalidOperationException("The workbook contains an invalid part path.");
                    }

                    segments.RemoveAt(segments.Count - 1);
                }
                else
                {
                    segments.Add(part);
                }
            }

            return string.Join("/", segments);
        }

        private static XDocument ReadXml(Dictionary<string, ZipArchiveEntry> entries, string path, CancellationToken cancellation)
        {
            cancellation.ThrowIfCancellationRequested();

            if (!entries.TryGetValue(path, out var entry))
            {
                throw new InvalidOperationException($"The workbook is missing '{path}'. Save it as a new .xlsx file.");
            }

            using var stream = entry.Open();
            using var reader = XmlReader.Create(stream, new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null,
                MaxCharactersInDocument = MaxFileBytes
            });
            return XDocument.Load(reader);
        }

        private static XNamespace SpreadsheetNamespace(XDocument document, string rootName)
        {
            var root = document.Root;
            var uri = root?.Name.NamespaceName;

            if (root?.Name.LocalName != rootName || (uri != "http://schemas.openxmlformats.org/spreadsheetml/2006/main"
                && uri != "http://purl.oclc.org/ooxml/spreadsheetml/main"))
            {
                throw new InvalidOperationException("The workbook contains invalid spreadsheet XML. Save it as a new .xlsx file.");
            }

            return root.Name.Namespace;
        }

        private static void ReadSheet(XDocument document, List<string> strings, SheetSnapshot snapshot, CancellationToken cancellation)
        {
            var ns = SpreadsheetNamespace(document, "worksheet");
            var cells = new Dictionary<(int Row, int Column), string>();
            var height = 0;
            var width = 0;
            var previousRow = 0;

            foreach (var row in document.Root.Element(ns + "sheetData")?.Elements(ns + "row") ?? Enumerable.Empty<XElement>())
            {
                cancellation.ThrowIfCancellationRequested();
                var rowNumber = previousRow + 1;
                var rowReference = (string)row.Attribute("r");

                if (rowReference != null && !int.TryParse(rowReference, NumberStyles.None, CultureInfo.InvariantCulture, out rowNumber))
                {
                    throw new InvalidOperationException("The worksheet contains an invalid row reference.");
                }

                if (rowNumber <= previousRow || rowNumber > MaxRows)
                {
                    throw new InvalidOperationException($"Worksheet rows must be ordered and within the first {MaxRows} rows.");
                }

                previousRow = rowNumber;
                var previousColumn = 0;

                foreach (var cell in row.Elements(ns + "c"))
                {
                    var column = previousColumn + 1;
                    var reference = (string)cell.Attribute("r");

                    if (reference != null)
                    {
                        var match = Regex.Match(reference, "\\A([A-Z]+)([0-9]+)\\z");
                        column = 0;

                        if (!match.Success || !int.TryParse(match.Groups[2].Value, out var cellRow) || cellRow != rowNumber
                            || match.Groups[1].Length > 3)
                        {
                            throw new InvalidOperationException("The worksheet contains an invalid cell reference.");
                        }

                        foreach (var letter in match.Groups[1].Value)
                        {
                            column = column * 26 + letter - 'A' + 1;
                        }
                    }

                    if (column <= previousColumn || column > MaxColumns)
                    {
                        throw new InvalidOperationException($"Worksheet cells must be ordered and within the first {MaxColumns} columns.");
                    }

                    previousColumn = column;
                    var value = ReadCell(cell, ns, strings, reference ?? $"row {rowNumber}, column {column}");

                    if (value.Length == 0)
                    {
                        continue;
                    }

                    height = Math.Max(height, rowNumber);
                    width = Math.Max(width, column);

                    if ((long)height * width > MaxCells)
                    {
                        throw new InvalidOperationException($"A table may contain at most {MaxCells} rectangular cells. Remove unused distant cells or split the table.");
                    }

                    cells.Add((rowNumber - 1, column - 1), value);
                }
            }

            for (var row = 0; row < height; row++)
            {
                snapshot.Rows.Add(Enumerable.Repeat("", width).ToList());
            }

            foreach (var cell in cells)
            {
                snapshot.Rows[cell.Key.Row][cell.Key.Column] = cell.Value;
            }
        }

        private static string ReadCell(XElement cell, XNamespace ns, List<string> strings, string position)
        {
            if (cell.Element(ns + "f") != null)
            {
                throw new InvalidOperationException($"{position}: Formulas are not supported. Replace the formula with a literal value.");
            }

            var value = cell.Element(ns + "v")?.Value ?? "";
            var type = (string)cell.Attribute("t") ?? "n";

            switch (type)
            {
                case "inlineStr":
                    return ReadText(cell.Element(ns + "is"), ns);
                case "s":
                    if (!int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var index) || index >= strings.Count)
                    {
                        throw new InvalidOperationException($"{position}: The shared string reference is invalid.");
                    }

                    return strings[index];
                case "b":
                    if (value == "0" || value == "1")
                    {
                        return value == "1" ? "true" : "false";
                    }

                    throw new InvalidOperationException($"{position}: The boolean value must be 0 or 1.");
                case "n":
                    if (value.Length == 0)
                    {
                        return "";
                    }

                    if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var number)
                        && !double.IsInfinity(number) && !double.IsNaN(number))
                    {
                        return number == 0d ? "0" : number.ToString("R", CultureInfo.InvariantCulture);
                    }

                    throw new InvalidOperationException($"{position}: The numeric value is invalid.");
                case "str":
                    return DecodeText(value);
                case "e":
                    throw new InvalidOperationException($"{position}: Excel reports a cell error ({value}). Replace it with a literal value.");
                default:
                    throw new InvalidOperationException($"{position}: Cell type '{type}' is not supported. Use text, numbers or booleans.");
            }
        }

        private static string ReadText(XElement item, XNamespace ns)
        {
            if (item == null)
            {
                return "";
            }

            return DecodeText(string.Concat(item.Elements().SelectMany(element => element.Name == ns + "t"
                ? new[] { element.Value }
                : element.Name == ns + "r" ? element.Elements(ns + "t").Select(text => text.Value) : Enumerable.Empty<string>())));
        }

        private static string DecodeText(string text)
        {
            return Regex.Replace(text, "_x([0-9A-Fa-f]{4})_", match =>
                ((char)int.Parse(match.Groups[1].Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture)).ToString());
        }
    }
}
