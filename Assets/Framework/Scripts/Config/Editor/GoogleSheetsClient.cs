using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FrameworkConfig.Editor
{
    internal sealed class SourceReadResult
    {
        public readonly List<SheetSnapshot> Sheets = new();
        public readonly HashSet<string> FailedWorkbookIds = new(StringComparer.Ordinal);
        public readonly List<string> Errors = new();
        public readonly Dictionary<string, string> ObservedVersions = new(StringComparer.Ordinal);
    }

    internal sealed class GoogleSheetsClient : IDisposable
    {
        private const string TokenUrl = "https://oauth2.googleapis.com/token";
        private const string DriveUrl = "https://www.googleapis.com/drive/v3/files";
        private const string SheetsUrl = "https://sheets.googleapis.com/v4/spreadsheets/";
        private const string SpreadsheetMime = "application/vnd.google-apps.spreadsheet";
        private const string FolderMime = "application/vnd.google-apps.folder";
        private const string FileFields = "id,name,version,mimeType,trashed,parents,driveId";
        private const int MaxResponseBytes = 32 * 1024 * 1024;
        private const int MaxWorkbooks = 200;
        private const int MaxSheets = 100;
        private const int MaxRows = 50000;
        private const int MaxColumns = 256;
        private const int MaxCells = 1000000;
        private const int RequestAttempts = 4;
        private readonly HttpClient _http;
        private readonly RSA _signer;
        private readonly string _email;
        private string _accessToken;
        private DateTimeOffset _tokenExpires;
        private bool _disposed;

        private sealed class FileMetadata
        {
            public string Id;
            public string Name;
            public string Version;
            public string MimeType;
            public string DriveId;
            public bool Trashed;
            public bool HasParents;
            public HashSet<string> Parents;
        }

        private sealed class SourceChangedException : Exception
        {
            public SourceChangedException() : base("Google Sheets changed during the update. Wait for editing to finish, then click Update Config again.")
            {
            }
        }

        public GoogleSheetsClient(string credentialPath)
        {
            if (string.IsNullOrWhiteSpace(credentialPath))
            {
                throw new InvalidOperationException("Select a service-account JSON key stored outside the Unity project.");
            }

            var path = Path.GetFullPath(credentialPath);
            var projectRoot = Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, ".."))
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
            var comparison = Path.DirectorySeparatorChar == '\\' ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            if (path.StartsWith(projectRoot, comparison))
            {
                throw new InvalidOperationException("Move the service-account JSON key outside the Unity project, then select it again.");
            }

            RSA signer = null;

            try
            {
                var file = new FileInfo(path);

                if (!file.Exists || file.Length > 64 * 1024)
                {
                    throw new InvalidOperationException("Select an existing service-account JSON key smaller than 64 KB.");
                }

                var credentials = JObject.Parse(File.ReadAllText(path));
                _email = (string)credentials["client_email"];
                var pem = (string)credentials["private_key"];

                if ((string)credentials["type"] != "service_account" || string.IsNullOrWhiteSpace(_email)
                    || !_email.EndsWith(".gserviceaccount.com", StringComparison.OrdinalIgnoreCase)
                    || string.IsNullOrWhiteSpace(pem))
                {
                    throw new InvalidOperationException("Select a Google service-account JSON key with client_email and private_key.");
                }

                const string begin = "-----BEGIN PRIVATE KEY-----";
                const string end = "-----END PRIVATE KEY-----";
                pem = pem.Trim();

                if (!pem.StartsWith(begin, StringComparison.Ordinal) || !pem.EndsWith(end, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException("The service-account key must contain an unencrypted PKCS#8 private key.");
                }

                var bytes = Convert.FromBase64String(pem.Substring(begin.Length, pem.Length - begin.Length - end.Length));

                try
                {
                    signer = GoogleServiceKey.CreateSigner(bytes);
                }
                finally
                {
                    Array.Clear(bytes, 0, bytes.Length);
                }

                _signer = signer;
                _http = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false })
                {
                    Timeout = Timeout.InfiniteTimeSpan
                };
            }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException
                || exception is JsonException || exception is FormatException || exception is CryptographicException
                || exception is NotSupportedException)
            {
                signer?.Dispose();
                throw new InvalidOperationException("Cannot read the service-account key. Select a valid Google JSON key outside the Unity project.");
            }
            catch
            {
                signer?.Dispose();
                throw;
            }
        }

        public async Task<SourceReadResult> ReadFolderAsync(string folderId,
            IReadOnlyList<SheetSnapshot> cachedSnapshots, CancellationToken ct)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(GoogleSheetsClient));
            }

            ValidateId(folderId);
            cachedSnapshots ??= Array.Empty<SheetSnapshot>();

            // A changing folder is retried once as a whole. No partial result escapes this check.
            for (var attempt = 0; attempt < 2; attempt++)
            {
                ct.ThrowIfCancellationRequested();

                try
                {
                    return await ReadFolderOnceAsync(folderId, cachedSnapshots, ct);
                }
                catch (SourceChangedException) when (attempt == 0)
                {
                    await Task.Delay(500, ct);
                }
            }

            throw new SourceChangedException();
        }

        private async Task<SourceReadResult> ReadFolderOnceAsync(string folderId,
            IReadOnlyList<SheetSnapshot> cachedSnapshots, CancellationToken ct)
        {
            var initial = await ListFolderAsync(folderId, ct);
            await VerifyRemovedWorkbooksAsync(folderId, initial, cachedSnapshots, ct);
            var result = new SourceReadResult();
            var observed = new Dictionary<string, FileMetadata>(initial, StringComparer.Ordinal);

            foreach (var workbook in initial.Values.OrderBy(file => file.Id, StringComparer.Ordinal))
            {
                ct.ThrowIfCancellationRequested();

                try
                {
                    var read = await ReadWorkbookAsync(folderId, workbook.Id, ct);
                    observed[workbook.Id] = read.Metadata;
                    result.Sheets.AddRange(read.Sheets);
                    result.ObservedVersions.Add(workbook.Id, read.Metadata.Version);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (SourceChangedException)
                {
                    throw;
                }
                catch (Exception exception) when (exception is InvalidOperationException)
                {
                    result.FailedWorkbookIds.Add(workbook.Id);
                    result.Errors.Add($"Workbook '{workbook.Name}': {exception.Message}");
                }
            }

            var final = await ListFolderAsync(folderId, ct);

            if (observed.Count != final.Count || observed.Any(pair => !final.TryGetValue(pair.Key, out var file)
                || !SameFileVersion(pair.Value, file)))
            {
                throw new SourceChangedException();
            }

            // Recheck removals after all reads, so a file restored during the update is not deleted locally.
            await VerifyRemovedWorkbooksAsync(folderId, final, cachedSnapshots, ct);
            return result;
        }

        private async Task<Dictionary<string, FileMetadata>> ListFolderAsync(string folderId, CancellationToken ct)
        {
            var folder = await GetFileAsync(folderId, ct);

            if (folder.Trashed || folder.MimeType != FolderMime)
            {
                throw new InvalidOperationException("The configured source is not an available Google Drive folder. Check Folder ID and sharing.");
            }

            var files = new Dictionary<string, FileMetadata>(StringComparer.Ordinal);
            var seenTokens = new HashSet<string>(StringComparer.Ordinal);
            var query = $"'{folderId}' in parents and trashed = false and mimeType = '{SpreadsheetMime}'";
            var url = DriveUrl + "?q=" + Escape(query) + "&fields=" + Escape("nextPageToken,incompleteSearch,files(" + FileFields + ")")
                + "&pageSize=100&spaces=drive&supportsAllDrives=true&includeItemsFromAllDrives=true"
                + (string.IsNullOrEmpty(folder.DriveId) ? "&corpora=user" : "&corpora=drive&driveId=" + Escape(folder.DriveId));
            string pageToken = null;

            do
            {
                var page = await GetJsonAsync(url + (pageToken == null ? "" : "&pageToken=" + Escape(pageToken)), "List source folder", ct);

                if ((bool?)page["incompleteSearch"] == true)
                {
                    throw new InvalidOperationException("Google Drive returned an incomplete folder listing. Check shared-drive access and try again. No files were removed.");
                }

                foreach (var item in page["files"] as JArray ?? new JArray())
                {
                    var file = ParseFile(item);

                    if (files.ContainsKey(file.Id))
                    {
                        throw new SourceChangedException();
                    }

                    if (!IsSourceWorkbook(file, folderId))
                    {
                        throw new SourceChangedException();
                    }

                    files.Add(file.Id, file);

                    if (files.Count > MaxWorkbooks)
                    {
                        throw new InvalidOperationException($"The source folder contains more than {MaxWorkbooks} spreadsheets. Use a smaller configuration folder.");
                    }
                }

                pageToken = (string)page["nextPageToken"];

                if (!string.IsNullOrEmpty(pageToken) && !seenTokens.Add(pageToken))
                {
                    throw new InvalidOperationException("Google Drive repeated a listing page. Try Update Config again. No files were removed.");
                }

                if (seenTokens.Count > 200)
                {
                    throw new InvalidOperationException("Google Drive returned too many listing pages. Use a smaller source folder and try again.");
                }
            }
            while (!string.IsNullOrEmpty(pageToken));

            return files;
        }

        private async Task VerifyRemovedWorkbooksAsync(string folderId,
            Dictionary<string, FileMetadata> current, IReadOnlyList<SheetSnapshot> cached, CancellationToken ct)
        {
            foreach (var id in cached.Select(sheet => sheet.WorkbookId).Distinct(StringComparer.Ordinal))
            {
                if (current.ContainsKey(id))
                {
                    continue;
                }

                FileMetadata file;

                try
                {
                    file = await GetFileAsync(id, ct);
                }
                catch (InvalidOperationException)
                {
                    throw new InvalidOperationException($"Cannot verify removal of workbook '{id}'. Restore access or sharing, then update again. Existing configuration was kept.");
                }

                if (!file.Trashed && !file.HasParents)
                {
                    throw new InvalidOperationException($"Cannot verify the parent folder of workbook '{id}'. Restore access or sharing, then update again. Existing configuration was kept.");
                }

                if (!file.Trashed && file.Parents.Contains(folderId))
                {
                    throw new SourceChangedException();
                }
            }
        }

        private async Task<(FileMetadata Metadata, List<SheetSnapshot> Sheets)> ReadWorkbookAsync(
            string folderId, string workbookId, CancellationToken ct)
        {
            for (var attempt = 0; attempt < 2; attempt++)
            {
                var before = await GetFileAsync(workbookId, ct);

                if (!IsSourceWorkbook(before, folderId))
                {
                    throw new SourceChangedException();
                }

                // Drive versions can lag behind cell edits. Always read the current table data.
                var data = await GetWorkbookJsonAsync(workbookId, ct);
                var sheets = ParseWorkbook(data, before);

                var after = await GetFileAsync(workbookId, ct);

                if (SameFileVersion(before, after) && IsSourceWorkbook(after, folderId))
                {
                    return (after, sheets);
                }
            }

            throw new SourceChangedException();
        }

        private Task<JObject> GetWorkbookJsonAsync(string id, CancellationToken ct)
        {
            var fields = "spreadsheetId,sheets(properties(sheetId,title,sheetType)"
                + ",data(startRow,startColumn,rowData(values(userEnteredValue,effectiveValue))))";
            return GetJsonAsync(SheetsUrl + Escape(id) + "?fields=" + Escape(fields), "Read spreadsheet", ct);
        }

        private static List<SheetSnapshot> ParseWorkbook(JObject workbook, FileMetadata metadata)
        {
            if ((string)workbook["spreadsheetId"] != metadata.Id || !(workbook["sheets"] is JArray sheets)
                || sheets.Count > MaxSheets)
            {
                throw new InvalidOperationException($"Google Sheets returned an invalid workbook or more than {MaxSheets} tabs. Split large workbooks and try again.");
            }

            var result = new List<SheetSnapshot>();
            var ids = new HashSet<int>();

            foreach (var item in sheets)
            {
                var properties = item["properties"];
                var id = (int?)properties?["sheetId"];
                var name = (string)properties?["title"];

                if (id == null || id < 0 || name == null || !ids.Add(id.Value))
                {
                    throw new InvalidOperationException("Google Sheets returned invalid tab metadata. Try Update Config again.");
                }

                var snapshot = new SheetSnapshot
                {
                    WorkbookId = metadata.Id,
                    WorkbookName = metadata.Name,
                    Version = metadata.Version,
                    SheetId = id.Value,
                    Name = name
                };
                result.Add(snapshot);

                if ((string)properties["sheetType"] != "GRID")
                {
                    snapshot.ReadErrors.Add("Only ordinary grid tabs are supported. Copy literal values into a grid tab.");
                    continue;
                }

                try
                {
                    ParseGrid(item["data"] as JArray ?? new JArray(), snapshot);
                }
                catch (InvalidOperationException exception)
                {
                    snapshot.Rows.Clear();
                    snapshot.ReadErrors.Clear();
                    snapshot.ReadErrors.Add(exception.Message);
                }
            }

            return result;
        }

        private static void ParseGrid(JArray grids, SheetSnapshot snapshot)
        {
            var height = 0;
            var width = 0;

            foreach (var grid in grids)
            {
                var startRow = (int?)grid["startRow"] ?? 0;
                var startColumn = (int?)grid["startColumn"] ?? 0;
                var rows = grid["rowData"] as JArray ?? new JArray();
                var columns = rows.Select(row => (row["values"] as JArray)?.Count ?? 0).DefaultIfEmpty().Max();

                if (startRow < 0 || startColumn < 0 || (long)startRow + rows.Count > MaxRows
                    || (long)startColumn + columns > MaxColumns)
                {
                    throw new InvalidOperationException($"A table may contain at most {MaxRows} rows and {MaxColumns} columns. Remove unused distant cells or split the table.");
                }

                height = Math.Max(height, startRow + rows.Count);
                width = Math.Max(width, startColumn + columns);
            }

            if ((long)height * width > MaxCells)
            {
                throw new InvalidOperationException($"A table may contain at most {MaxCells} rectangular cells. Remove unused distant cells or split the table.");
            }

            for (var row = 0; row < height; row++)
            {
                snapshot.Rows.Add(Enumerable.Repeat("", width).ToList());
            }

            foreach (var grid in grids)
            {
                var startRow = (int?)grid["startRow"] ?? 0;
                var startColumn = (int?)grid["startColumn"] ?? 0;
                var rows = grid["rowData"] as JArray ?? new JArray();

                for (var row = 0; row < rows.Count; row++)
                {
                    var cells = rows[row]["values"] as JArray ?? new JArray();

                    for (var column = 0; column < cells.Count; column++)
                    {
                        var cell = cells[column];
                        var entered = cell["userEnteredValue"] as JObject;
                        var effective = cell["effectiveValue"] as JObject;
                        var position = $"Row {startRow + row + 1}, column {startColumn + column + 1}: ";

                        if (entered?["formulaValue"] != null)
                        {
                            AddReadError(snapshot, position + "Formulas are not supported. Replace the formula with a literal value.");
                            continue;
                        }

                        if (effective?["errorValue"] != null || (entered == null && effective != null && effective.Count > 0))
                        {
                            AddReadError(snapshot, position + "Use a literal value. Calculated, linked, or error cells are not supported.");
                            continue;
                        }

                        snapshot.Rows[startRow + row][startColumn + column] = ReadLiteral(entered, snapshot, position);
                    }
                }
            }
        }

        private static string ReadLiteral(JObject value, SheetSnapshot snapshot, string position)
        {
            if (value == null || value.Count == 0)
            {
                return "";
            }

            if (value["stringValue"] != null)
            {
                return (string)value["stringValue"];
            }

            if (value["boolValue"] != null)
            {
                return (bool)value["boolValue"] ? "true" : "false";
            }

            if (value["numberValue"] != null)
            {
                var number = (double)value["numberValue"];

                if (!double.IsNaN(number) && !double.IsInfinity(number))
                {
                    return number.ToString("R", CultureInfo.InvariantCulture);
                }
            }

            AddReadError(snapshot, position + "This cell type is not supported. Use text, a finite number, or a boolean.");
            return "";
        }

        private static void AddReadError(SheetSnapshot snapshot, string error)
        {
            if (snapshot.ReadErrors.Count < 100)
            {
                snapshot.ReadErrors.Add(error);
            }
            else if (snapshot.ReadErrors.Count == 100)
            {
                snapshot.ReadErrors.Add("Additional invalid cells were omitted. Fix the reported cells and update again.");
            }
        }

        private async Task<FileMetadata> GetFileAsync(string id, CancellationToken ct)
        {
            ValidateId(id);
            var json = await GetJsonAsync(DriveUrl + "/" + Escape(id) + "?supportsAllDrives=true&fields=" + Escape(FileFields), "Read Drive metadata", ct);
            var file = ParseFile(json);

            if (file.Id != id)
            {
                throw new InvalidOperationException("Google Drive returned unexpected file metadata. Try Update Config again.");
            }

            return file;
        }

        private static FileMetadata ParseFile(JToken file)
        {
            var id = (string)file["id"];
            var version = (string)file["version"];
            var name = (string)file["name"];
            var mime = (string)file["mimeType"];

            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(version) || name == null || string.IsNullOrEmpty(mime))
            {
                throw new InvalidOperationException("Google Drive returned incomplete metadata. No configuration was removed; try again.");
            }

            ValidateId(id);
            return new FileMetadata
            {
                Id = id,
                Name = name,
                Version = version,
                MimeType = mime,
                DriveId = (string)file["driveId"],
                Trashed = (bool?)file["trashed"] ?? false,
                HasParents = file["parents"] is JArray,
                Parents = new HashSet<string>((file["parents"] as JArray ?? new JArray()).Values<string>(), StringComparer.Ordinal)
            };
        }

        private static bool SameFileVersion(FileMetadata first, FileMetadata second)
        {
            return first.Id == second.Id && first.Version == second.Version && first.Name == second.Name
                && first.MimeType == second.MimeType && first.Trashed == second.Trashed
                && first.Parents.SetEquals(second.Parents);
        }

        private static bool IsSourceWorkbook(FileMetadata file, string folderId)
        {
            return !file.Trashed && file.MimeType == SpreadsheetMime && file.Parents.Contains(folderId);
        }

        private static void ValidateId(string id)
        {
            if (string.IsNullOrEmpty(id) || id.Length > 200 || !Regex.IsMatch(id, "\\A[A-Za-z0-9_-]+\\z"))
            {
                throw new InvalidOperationException("Enter a Google Drive folder or file ID, not a URL.");
            }
        }

        private static string Escape(string value)
        {
            return Uri.EscapeDataString(value);
        }

        private Task<JObject> GetJsonAsync(string url, string operation, CancellationToken ct)
        {
            return SendJsonAsync(() => new HttpRequestMessage(HttpMethod.Get, url), operation, true, ct);
        }

        private async Task EnsureTokenAsync(CancellationToken ct)
        {
            if (_accessToken != null && DateTimeOffset.UtcNow < _tokenExpires)
            {
                return;
            }

            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var header = Base64Url(Encoding.UTF8.GetBytes("{\"alg\":\"RS256\",\"typ\":\"JWT\"}"));
            var claims = new JObject
            {
                ["iss"] = _email,
                ["scope"] = "https://www.googleapis.com/auth/drive.metadata.readonly https://www.googleapis.com/auth/spreadsheets.readonly",
                ["aud"] = TokenUrl,
                ["iat"] = now - 30,
                ["exp"] = now + 3500
            };
            var unsigned = header + "." + Base64Url(Encoding.UTF8.GetBytes(claims.ToString(Formatting.None)));
            var signature = _signer.SignData(Encoding.UTF8.GetBytes(unsigned), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            var assertion = unsigned + "." + Base64Url(signature);
            var token = await SendJsonAsync(() => new HttpRequestMessage(HttpMethod.Post, TokenUrl)
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["grant_type"] = "urn:ietf:params:oauth:grant-type:jwt-bearer",
                    ["assertion"] = assertion
                })
            }, "Authorize service account", false, ct);
            var accessToken = token["access_token"]?.Type == JTokenType.String ? (string)token["access_token"] : null;
            var expiry = token["expires_in"];

            if (string.IsNullOrWhiteSpace(accessToken) || expiry?.Type != JTokenType.Integer
                || !int.TryParse(expiry.ToString(Formatting.None), NumberStyles.Integer, CultureInfo.InvariantCulture, out var expires)
                || expires <= 60)
            {
                throw new InvalidOperationException("Google did not return a usable access token. Check the service-account key and computer clock.");
            }

            _accessToken = accessToken;
            _tokenExpires = DateTimeOffset.UtcNow.AddSeconds(Math.Min(expires, 3600) - 60);
        }

        private async Task<JObject> SendJsonAsync(Func<HttpRequestMessage> createRequest,
            string operation, bool authorize, CancellationToken ct)
        {
            var refreshedToken = false;

            for (var attempt = 0; attempt < RequestAttempts; attempt++)
            {
                ct.ThrowIfCancellationRequested();

                if (authorize)
                {
                    await EnsureTokenAsync(ct);
                }

                using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
                timeout.CancelAfter(TimeSpan.FromSeconds(30));
                using var request = createRequest();

                if (authorize)
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
                }

                try
                {
                    using var response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeout.Token);
                    var status = (int)response.StatusCode;

                    if (response.StatusCode == HttpStatusCode.Unauthorized && authorize && !refreshedToken && attempt < RequestAttempts - 1)
                    {
                        _accessToken = null;
                        refreshedToken = true;
                        continue;
                    }

                    if ((status == 429 || status >= 500) && attempt < RequestAttempts - 1)
                    {
                        var delay = response.Headers.RetryAfter?.Delta?.TotalSeconds
                            ?? (response.Headers.RetryAfter?.Date - DateTimeOffset.UtcNow)?.TotalSeconds
                            ?? Math.Pow(2, attempt);
                        await Task.Delay(TimeSpan.FromSeconds(Math.Max(0.25, Math.Min(delay, 8))), ct);
                        continue;
                    }

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new InvalidOperationException(HttpFailure(operation, status));
                    }

                    if (response.Content.Headers.ContentLength > MaxResponseBytes)
                    {
                        throw new InvalidOperationException($"{operation}: response is larger than 32 MB. Split the workbook or remove unused distant cells.");
                    }

                    using var stream = await response.Content.ReadAsStreamAsync();
                    using var buffer = new MemoryStream();
                    var bytes = new byte[8192];
                    int count;

                    while ((count = await stream.ReadAsync(bytes, 0, bytes.Length, timeout.Token)) > 0)
                    {
                        if (buffer.Length + count > MaxResponseBytes)
                        {
                            throw new InvalidOperationException($"{operation}: response is larger than 32 MB. Split the workbook or remove unused distant cells.");
                        }

                        buffer.Write(bytes, 0, count);
                    }

                    using var reader = new JsonTextReader(new StringReader(Encoding.UTF8.GetString(buffer.ToArray())))
                    {
                        DateParseHandling = DateParseHandling.None,
                        MaxDepth = 64,
                        Culture = CultureInfo.InvariantCulture
                    };
                    return JObject.Load(reader);
                }
                catch (OperationCanceledException) when (!ct.IsCancellationRequested)
                {
                    throw new InvalidOperationException($"{operation} timed out. Check the network and try again.");
                }
                catch (HttpRequestException)
                {
                    throw new InvalidOperationException($"{operation} failed. Check the network and try again.");
                }
                catch (IOException)
                {
                    throw new InvalidOperationException($"{operation} could not finish reading the response. Check the network and try again.");
                }
                catch (JsonException)
                {
                    throw new InvalidOperationException($"{operation} returned invalid JSON. Try Update Config again.");
                }
            }

            throw new InvalidOperationException($"{operation} failed after retries. Try again later.");
        }

        private static string HttpFailure(string operation, int status)
        {
            var advice = status switch
            {
                400 => "Check Folder ID, the service-account key, and the computer clock.",
                401 => "Check the service-account key and try again.",
                403 => "Enable the Google Drive and Sheets APIs, and share the source folder with the service-account email. Also check API quotas.",
                404 => "Check the file ID and sharing. An inaccessible file is not treated as deleted.",
                429 => "Google's rate limit was reached. Wait and try again.",
                _ => "Google could not complete the request. Try again later."
            };
            return $"{operation} failed (HTTP {status}). {advice}";
        }

        private static string Base64Url(byte[] bytes)
        {
            return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _accessToken = null;
            _http.Dispose();
            _signer.Dispose();
        }
    }
}
