using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace FrameworkConfig.Editor
{
    internal sealed class ConfigUpdateResult
    {
        public bool Failed;
        public bool Partial;
        public string Summary;
        public string ReportPath;
        public List<string> Actions = new();
        public List<ConfigIssue> Issues = new();

        public string Describe()
        {
            return Summary + "\n\n" + string.Join("\n", Actions)
                + (Issues.Count == 0 ? "" : "\n\n" + string.Join("\n\n", Issues));
        }
    }

    internal static class ConfigUpdater
    {
        private static bool _updating;

        public static async Task<ConfigUpdateResult> UpdateAsync(string sourcePath, string credentialPath,
            CancellationToken cancellation)
        {
            var result = new ConfigUpdateResult();
            bool ownsUpdate = false;

            try
            {
                if (_updating)
                {
                    throw new InvalidOperationException("Another config update is running. Wait for it to finish, then update again.");
                }

                _updating = true;
                ownsUpdate = true;
                RequireIdleEditor();

                string root = ConfigFiles.GameRoot(sourcePath);
                byte[] sourceBefore = File.ReadAllBytes(sourcePath);
                using var sourceReader = new StreamReader(new MemoryStream(sourceBefore));
                var source = JsonConvert.DeserializeObject<ConfigSource>(sourceReader.ReadToEnd());
                ValidateSource(root, source);
                string manifestPath = ConfigFiles.InGame(root, ConfigFiles.ManifestPath);
                byte[] manifestBefore = File.Exists(manifestPath) ? File.ReadAllBytes(manifestPath) : null;
                var previous = ConfigFiles.ReadManifest(root);
                ConfigFiles.ReadOwnedFiles(root, previous);
                string cachePath = ConfigFiles.CacheDirectory(root) + "/source-cache.json";
                var cached = new List<SheetSnapshot>();

                if (File.Exists(cachePath))
                {
                    try
                    {
                        cached = JsonConvert.DeserializeObject<List<SheetSnapshot>>(File.ReadAllText(cachePath)) ?? cached;
                    }
                    catch (JsonException)
                    {
                        // A download cache is disposable; applied snapshots remain in the manifest.
                    }
                }

                // Include applied sources when checking removals, even after a cache was deleted.
                foreach (var table in string.IsNullOrEmpty(previous?.FolderId) ? new List<AppliedTable>() : previous.Tables)
                {
                    if (!cached.Any(sheet => sheet.Key == table.Source.Key))
                    {
                        cached.Add(table.Source);
                    }
                }

                using var client = new GoogleSheetsClient(credentialPath);
                var read = await client.ReadFolderAsync(source.FolderId, cached, cancellation);
                cancellation.ThrowIfCancellationRequested();
                RequireIdleEditor();

                if (!SameFileBytes(sourcePath, sourceBefore) || !SameFileBytes(manifestPath, manifestBefore))
                {
                    throw new InvalidOperationException("Config settings or the applied snapshot changed while downloading. "
                        + "No downloaded changes were applied. Update config again.");
                }

                result = ApplySnapshots(root, source, read.Sheets, read.FailedWorkbookIds, read.Errors);

                if (!result.Failed)
                {
                    try
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(cachePath));
                        File.WriteAllText(cachePath, ConfigFiles.Json(read.Sheets), ConfigFiles.Utf8);
                    }
                    catch (Exception)
                    {
                        result.Issues.Add(new ConfigIssue
                        {
                            Warning = true,
                            Table = "Download cache",
                            Message = "Config was updated, but the download cache could not be saved. The next update will read the source again."
                        });
                    }
                }
            }
            catch (OperationCanceledException)
            {
                result.Failed = true;
                result.Summary = "Update cancelled. Previous output was kept.";
            }
            catch (Exception exception)
            {
                result.Failed = true;
                result.Summary = exception is ConfigRecoveryException
                    ? "Update failed. File recovery needs attention."
                    : "Update failed. Previous output was kept.";
                result.Issues.Add(new ConfigIssue { Table = "Config", Message = exception.Message });
            }
            finally
            {
                if (ownsUpdate)
                {
                    _updating = false;
                }
            }

            SaveReport(result);
            return result;
        }

        private static bool SameFileBytes(string path, byte[] expected)
        {
            return expected == null ? !File.Exists(path)
                : File.Exists(path) && expected.SequenceEqual(File.ReadAllBytes(path));
        }

        public static ConfigUpdateResult ApplySnapshots(string root, ConfigSource source,
            IReadOnlyList<SheetSnapshot> snapshots, ISet<string> failedWorkbooks, IReadOnlyList<string> readErrors)
        {
            RequireIdleEditor();
            var result = new ConfigUpdateResult();
            var manifest = ConfigFiles.ReadManifest(root);
            var owned = ConfigFiles.ReadOwnedFiles(root, manifest);

            if (manifest != null && (manifest.Namespace != source.Namespace
                || (!string.IsNullOrEmpty(manifest.FolderId) && manifest.FolderId != source.FolderId)))
            {
                throw new InvalidOperationException("The source folder or namespace changed. Use a new game folder, "
                    + "or restore the original ConfigSource.json. Existing generated files were kept.");
            }

            foreach (string error in readErrors)
            {
                result.Issues.Add(new ConfigIssue { Table = "Google Sheets", Message = error });
            }

            if (snapshots.Select(sheet => sheet.Key).Distinct().Count() != snapshots.Count
                || snapshots.GroupBy(sheet => sheet.Name, StringComparer.OrdinalIgnoreCase).Any(group => group.Count() > 1))
            {
                throw new InvalidOperationException("Two source tabs have the same name or identity. Give each table a unique name.");
            }

            var old = (manifest?.Tables ?? new List<AppliedTable>()).ToDictionary(table => table.Source.Key);
            var incoming = snapshots.ToDictionary(sheet => sheet.Key);
            var parsed = new Dictionary<string, ConfigTableModel>();
            var oldModels = new Dictionary<string, ConfigTableModel>();
            var blocked = new HashSet<string>(StringComparer.Ordinal);
            var graph = BuildReferenceGraph(snapshots.Concat(old.Values.Select(table => table.Source)));

            foreach (var table in old.Values)
            {
                var errors = new List<ConfigIssue>();
                var model = ConfigCompiler.Parse(table.Source, errors);

                if (model == null || errors.Any(issue => !issue.Warning))
                {
                    throw new InvalidOperationException("The previous snapshot for " + table.Source.Name
                        + " is invalid. Restore the manifest and generated files from the same Git revision.");
                }

                oldModels.Add(table.Source.Key, model);

                if (failedWorkbooks.Contains(table.Source.WorkbookId))
                {
                    blocked.Add(table.Source.Name);
                }
            }

            foreach (var sheet in snapshots)
            {
                int before = result.Issues.Count;
                var model = ConfigCompiler.Parse(sheet, result.Issues);
                bool valid = model != null && !result.Issues.Skip(before).Any(issue => !issue.Warning);

                if (valid)
                {
                    valid = ConfigCompiler.ValidateAssets(model, result.Issues);
                }

                if (valid)
                {
                    parsed.Add(sheet.Key, model);
                }
                else
                {
                    blocked.Add(sheet.Name);

                    if (old.TryGetValue(sheet.Key, out var previous))
                    {
                        blocked.Add(previous.Source.Name);
                    }
                }
            }

            // Keep connected changes together, including reverse references to deleted rows/tables.
            ExpandBlocked(blocked, graph);
            Dictionary<string, ConfigTableModel> selected;

            while (true)
            {
                selected = SelectTables(oldModels, parsed, incoming, blocked);
                var names = ByName(selected.Values);
                var referenceIssues = new List<ConfigIssue>();
                var invalidNames = ConfigCompiler.ValidateReferences(names, referenceIssues);

                if (invalidNames.Count == 0)
                {
                    break;
                }

                result.Issues.AddRange(referenceIssues);
                int before = blocked.Count;
                blocked.UnionWith(invalidNames);
                ExpandBlocked(blocked, graph);

                if (blocked.Count == before)
                {
                    throw new InvalidOperationException("The retained tables have broken references. Restore a consistent "
                        + "configuration snapshot from Git, then update again.");
                }
            }

            var finalModels = ByName(selected.Values);
            var retainedAssetIssues = new List<ConfigIssue>();

            foreach (var model in finalModels.Values.Where(model => blocked.Contains(model.Source.Name)))
            {
                ConfigCompiler.ValidateAssets(model, retainedAssetIssues);
            }

            result.Issues.AddRange(retainedAssetIssues);

            if (retainedAssetIssues.Any(issue => !issue.Warning))
            {
                throw new InvalidOperationException("The previous configuration has invalid local assets. "
                    + "Fix the assets before updating. Previous output was kept.\n"
                    + string.Join("\n\n", retainedAssetIssues));
            }

            if (manifest != null && string.IsNullOrEmpty(manifest.FolderId) && !string.IsNullOrEmpty(source.FolderId)
                && manifest.Tables.Any(table => !finalModels.TryGetValue(table.Source.Name, out var model)
                    || blocked.Contains(table.Source.Name) || string.IsNullOrEmpty(model.Source.WorkbookId)
                    || model.Source.WorkbookId == table.Source.WorkbookId))
            {
                throw new InvalidOperationException("For the first connection, import all bundled tables into your Google folder "
                    + "and fix their errors. The bundled snapshot was kept.\n" + string.Join("\n", result.Issues));
            }

            if (!ConfigCompiler.ValidateNames(finalModels, source.Namespace, result.Issues, root))
            {
                throw new InvalidOperationException("Generated names conflict. No files were changed.\n"
                    + string.Join("\n", result.Issues));
            }

            var next = new Dictionary<string, string>(StringComparer.Ordinal);
            var applied = new ConfigManifest { Namespace = source.Namespace, FolderId = source.FolderId };
            int updated = 0;
            int kept = 0;
            int omitted = 0;
            int deleted = 0;

            foreach (var pair in selected.OrderBy(pair => pair.Value.Source.Name, StringComparer.Ordinal))
            {
                string key = pair.Key;
                var model = pair.Value;
                bool retained = blocked.Contains(model.Source.Name);

                if (retained && old.TryGetValue(key, out var previous))
                {
                    foreach (string path in previous.Files.Keys)
                    {
                        AddOutput(next, path, owned[path]);
                    }

                    applied.Tables.Add(previous);
                    kept++;
                    result.Actions.Add(model.Source.Name + ": kept previous version (this table or a related table has errors).");
                    continue;
                }

                var generated = ConfigCompiler.Generate(model, finalModels, source.Namespace);
                generated.Add("Editor/Tables/Generated/" + model.Source.Name + ".csv", ConfigFiles.Csv(model.Source));
                var entry = new AppliedTable
                {
                    Source = model.Source,
                    Hash = ConfigFiles.Hash(ConfigFiles.Json(model.Source.Rows)),
                    RowCount = model.Data.Count,
                    Files = generated.ToDictionary(file => file.Key, file => ConfigFiles.Hash(file.Value))
                };

                foreach (var file in generated)
                {
                    AddOutput(next, file.Key, file.Value);
                }

                bool unchanged = old.TryGetValue(key, out previous) && entry.Hash == previous.Hash
                    && entry.Source.Name == previous.Source.Name
                    && ConfigFiles.Json(entry.Files) == ConfigFiles.Json(previous.Files);
                applied.Tables.Add(unchanged ? previous : entry);
                result.Actions.Add(model.Source.Name + (unchanged ? ": unchanged." : ": updated."));

                if (!unchanged)
                {
                    updated++;
                }
            }

            foreach (var sheet in snapshots.Where(sheet => !selected.ContainsKey(sheet.Key)))
            {
                omitted++;
                result.Actions.Add(sheet.Name + ": not generated (no previous valid version).");
            }

            foreach (var previous in old.Values.Where(table => !selected.ContainsKey(table.Source.Key)
                && !finalModels.ContainsKey(table.Source.Name)))
            {
                deleted++;
                result.Actions.Add(previous.Source.Name + ": deleted.");
            }

            foreach (var file in ConfigCompiler.GenerateBootstrap(finalModels, source.Namespace))
            {
                AddOutput(next, file.Key, file.Value);
                applied.SharedFiles.Add(file.Key, ConfigFiles.Hash(file.Value));
            }

            next.Add(ConfigFiles.ManifestPath, ConfigFiles.Json(applied));
            string oldManifestPath = ConfigFiles.InGame(root, ConfigFiles.ManifestPath);

            if (File.Exists(oldManifestPath))
            {
                owned.Add(ConfigFiles.ManifestPath, File.ReadAllText(oldManifestPath));
            }

            ConfigFiles.Apply(root, owned, next);
            result.Partial = kept > 0 || omitted > 0 || readErrors.Count > 0;
            result.Summary = result.Partial
                ? $"Partially updated: {updated} updated, {kept} kept previous, {omitted} not generated, {deleted} deleted."
                : $"Config is up to date: {updated} updated, {deleted} deleted.";
            return result;
        }

        public static void ValidateSource(string root, ConfigSource source)
        {
            if (source == null || string.IsNullOrWhiteSpace(source.FolderId) || string.IsNullOrWhiteSpace(source.Namespace))
            {
                throw new InvalidOperationException("Choose a Google Drive folder and a code namespace in Framework > Config.");
            }

            foreach (string path in Directory.GetFiles("Assets/GameContent", "ConfigSource.json", SearchOption.AllDirectories))
            {
                string otherRoot = ConfigFiles.GameRoot(path);

                if (otherRoot == root)
                {
                    continue;
                }

                var other = JsonConvert.DeserializeObject<ConfigSource>(File.ReadAllText(path));

                if (other?.Namespace == source.Namespace)
                {
                    throw new InvalidOperationException("Namespace " + source.Namespace + " is already used by " + otherRoot
                        + ". Give each game its own namespace.");
                }
            }
        }

        private static Dictionary<string, ConfigTableModel> ByName(IEnumerable<ConfigTableModel> models)
        {
            var result = new Dictionary<string, ConfigTableModel>(StringComparer.Ordinal);
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var model in models)
            {
                if (!names.Add(model.Source.Name))
                {
                    throw new InvalidOperationException("New and retained tables share the name " + model.Source.Name
                        + ". Give each table a unique name before updating.");
                }

                result.Add(model.Source.Name, model);
            }

            return result;
        }

        private static Dictionary<string, ConfigTableModel> SelectTables(Dictionary<string, ConfigTableModel> old,
            Dictionary<string, ConfigTableModel> parsed, Dictionary<string, SheetSnapshot> incoming, HashSet<string> blocked)
        {
            var result = new Dictionary<string, ConfigTableModel>();

            foreach (var pair in old)
            {
                if (blocked.Contains(pair.Value.Source.Name)
                    || (incoming.TryGetValue(pair.Key, out var next) && blocked.Contains(next.Name)))
                {
                    result.Add(pair.Key, pair.Value);
                }
            }

            foreach (var pair in parsed)
            {
                if (!blocked.Contains(pair.Value.Source.Name) && !result.ContainsKey(pair.Key))
                {
                    result.Add(pair.Key, pair.Value);
                }
            }

            return result;
        }

        private static Dictionary<string, HashSet<string>> BuildReferenceGraph(IEnumerable<SheetSnapshot> sheets)
        {
            var graph = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);

            foreach (var sheet in sheets)
            {
                if (!graph.ContainsKey(sheet.Name))
                {
                    graph.Add(sheet.Name, new HashSet<string>(StringComparer.Ordinal));
                }

                foreach (string type in sheet.Rows.Count > 1 ? sheet.Rows[1] : new List<string>())
                {
                    if (!type.StartsWith("ref:", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    string target = type.Substring(4);

                    if (!graph.ContainsKey(target))
                    {
                        graph.Add(target, new HashSet<string>(StringComparer.Ordinal));
                    }

                    graph[sheet.Name].Add(target);
                    graph[target].Add(sheet.Name);
                }
            }

            return graph;
        }

        private static void ExpandBlocked(HashSet<string> blocked, Dictionary<string, HashSet<string>> graph)
        {
            var queue = new Queue<string>(blocked);

            while (queue.Count > 0)
            {
                if (!graph.TryGetValue(queue.Dequeue(), out var neighbours))
                {
                    continue;
                }

                foreach (string name in neighbours)
                {
                    if (blocked.Add(name))
                    {
                        queue.Enqueue(name);
                    }
                }
            }
        }

        private static void AddOutput(Dictionary<string, string> files, string path, string content)
        {
            if (files.Keys.Any(existing => string.Equals(existing, path, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException("Generated output has two owners: " + path);
            }

            files.Add(path, content);
        }

        public static ConfigUpdateResult ReportFailure(Exception exception)
        {
            var result = new ConfigUpdateResult
            {
                Failed = true,
                Summary = exception is ConfigRecoveryException ? "Update failed. File recovery needs attention." : "Update failed. Previous output was kept."
            };
            result.Issues.Add(new ConfigIssue { Table = "Config", Message = exception.Message });
            SaveReport(result);
            return result;
        }

        private static void RequireIdleEditor()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
            {
                throw new InvalidOperationException("Stop Play Mode and wait for compilation, then update config again.");
            }
        }

        private static void SaveReport(ConfigUpdateResult result)
        {
            try
            {
                WriteReport(result);
            }
            catch (Exception)
            {
                result.ReportPath = "";
                Debug.LogWarning(result.Describe() + "\nThe local report could not be saved. Check write access to Library/FrameworkConfig/Reports.");
            }
        }

        private static void WriteReport(ConfigUpdateResult result)
        {
            string folder = "Library/FrameworkConfig/Reports";
            Directory.CreateDirectory(folder);
            result.ReportPath = folder + "/" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff") + ".txt";
            string content = DateTime.UtcNow.ToString("u") + "\n" + result.Describe();
            File.WriteAllText(result.ReportPath, content, ConfigFiles.Utf8);
            File.WriteAllText(folder + "/latest.txt", content, ConfigFiles.Utf8);

            foreach (string old in Directory.GetFiles(folder, "*.txt").Where(path => !path.EndsWith("latest.txt", StringComparison.Ordinal))
                .OrderByDescending(path => path, StringComparer.Ordinal).Skip(30))
            {
                File.Delete(old);
            }

            string message = result.Describe() + "\n\nReport: " + result.ReportPath;

            if (result.Failed || result.Partial)
            {
                Debug.LogError(message);
            }
            else if (result.Issues.Count > 0)
            {
                Debug.LogWarning(message);
            }
            else
            {
                Debug.Log(message);
            }
        }
    }
}
