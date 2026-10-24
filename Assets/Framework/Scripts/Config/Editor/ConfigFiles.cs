using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using UnityEditor;

namespace FrameworkConfig.Editor
{
    internal sealed class ConfigRecoveryException : IOException
    {
        public ConfigRecoveryException(string message, Exception cause) : base(message, cause)
        {
        }
    }

    internal static class ConfigFiles
    {
        public const string ManifestPath = "Editor/Tables/Generated/manifest.json";
        public static readonly UTF8Encoding Utf8 = new(false);

        public static string Hash(string text)
        {
            using var sha = SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(Utf8.GetBytes(text))).Replace("-", "").ToLowerInvariant();
        }

        public static string Json(object value)
        {
            return JsonConvert.SerializeObject(value, Formatting.Indented) + "\n";
        }

        public static string GameRoot(string sourcePath)
        {
            var path = sourcePath.Replace('\\', '/');
            const string suffix = "/Editor/ConfigSource.json";

            if (!path.StartsWith("Assets/GameContent/", StringComparison.Ordinal)
                || !path.EndsWith(suffix, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("Save ConfigSource.json in Assets/GameContent/YourGame/Editor.");
            }

            string root = path.Substring(0, path.Length - suffix.Length);

            if (root.Split('/').Length != 3 || root.Contains(".."))
            {
                throw new InvalidOperationException("Choose one game folder directly inside Assets/GameContent.");
            }

            return root;
        }

        public static string InGame(string root, string relativePath)
        {
            string fullRoot = Path.GetFullPath(root) + Path.DirectorySeparatorChar;
            string fullPath = Path.GetFullPath(Path.Combine(root, relativePath));

            if (!fullPath.StartsWith(fullRoot, StringComparison.Ordinal)
                || Path.IsPathRooted(relativePath) || relativePath.Contains("\\")
                || relativePath.Split('/').Any(part => part == ".." || part.Length == 0))
            {
                throw new InvalidOperationException("Generated path is outside this game: " + relativePath);
            }

            // Do not follow user-created links when replacing or deleting generated files.
            for (var entry = new DirectoryInfo(Path.GetDirectoryName(fullPath)); entry != null; entry = entry.Parent)
            {
                if (entry.Exists && (entry.Attributes & FileAttributes.ReparsePoint) != 0)
                {
                    throw new InvalidOperationException("Generated folders must not be symbolic links: " + entry.FullName);
                }

                if (entry.FullName == Path.GetFullPath(root))
                {
                    break;
                }
            }

            if (File.Exists(fullPath) && (File.GetAttributes(fullPath) & FileAttributes.ReparsePoint) != 0)
            {
                throw new InvalidOperationException("Generated files must not be symbolic links: " + relativePath);
            }

            return fullPath;
        }

        public static ConfigManifest ReadManifest(string root)
        {
            string path = InGame(root, ManifestPath);

            if (!File.Exists(path))
            {
                return null;
            }

            var manifest = JsonConvert.DeserializeObject<ConfigManifest>(File.ReadAllText(path));

            if (manifest == null || manifest.GeneratorVersion != ConfigManifest.CurrentVersion
                || manifest.Tables == null || manifest.SharedFiles == null)
            {
                throw new InvalidOperationException("The generated manifest is invalid. Restore it from Git before updating.");
            }

            return manifest;
        }

        public static Dictionary<string, string> ReadOwnedFiles(string root, ConfigManifest manifest)
        {
            var result = new Dictionary<string, string>(StringComparer.Ordinal);

            if (manifest == null)
            {
                return result;
            }

            var hashes = manifest.Tables.SelectMany(table => table.Files).Concat(manifest.SharedFiles);

            foreach (var file in hashes)
            {
                string path = InGame(root, file.Key);

                if (result.ContainsKey(file.Key) || !File.Exists(path))
                {
                    throw new InvalidOperationException("Generated file is missing or has conflicting owners: " + file.Key
                        + ". Restore generated files and the manifest from the same Git revision.");
                }

                string content = File.ReadAllText(path);

                if (Hash(content) != file.Value)
                {
                    throw new InvalidOperationException("Generated file was edited: " + file.Key
                        + ". Restore it from Git, then change the Google Sheet instead.");
                }

                result.Add(file.Key, content);
            }

            return result;
        }

        public static string Csv(SheetSnapshot sheet)
        {
            return string.Join("\n", sheet.Rows.Select(row => string.Join(",", row.Select(value =>
                "\"" + (value ?? "").Replace("\"", "\"\"") + "\"")))) + "\n";
        }

        public static string CacheDirectory(string root)
        {
            return "Library/FrameworkConfig/" + Hash(root).Substring(0, 16);
        }

        public static int Apply(string root, Dictionary<string, string> previous, Dictionary<string, string> next)
        {
            var writes = next.Where(file => !previous.TryGetValue(file.Key, out string old) || old != file.Value)
                .OrderBy(file => file.Key == ManifestPath ? 1 : 0).ThenBy(file => file.Key, StringComparer.Ordinal).ToList();
            var deletes = previous.Keys.Where(path => !next.ContainsKey(path)).OrderBy(path => path, StringComparer.Ordinal).ToList();

            foreach (var file in writes)
            {
                string path = InGame(root, file.Key);

                if (File.Exists(path) && !previous.ContainsKey(file.Key))
                {
                    if (previous.Keys.Any(old => string.Equals(old, file.Key, StringComparison.OrdinalIgnoreCase)))
                    {
                        throw new InvalidOperationException("A table or module was renamed using only letter case. "
                            + "Give it a different temporary name, update once, then use the final name.");
                    }

                    throw new InvalidOperationException("A file already exists at " + file.Key
                        + ". Move it or choose another namespace. No files were changed.");
                }
            }

            if (writes.Count + deletes.Count == 0)
            {
                return 0;
            }

            string staging = CacheDirectory(root) + "/PendingUpdate";

            if (Directory.Exists(staging))
            {
                throw new InvalidOperationException("An interrupted update has a backup at " + staging
                    + ". Restore the game from Git or this backup before removing the backup folder.");
            }

            Directory.CreateDirectory(staging);
            var backups = new Dictionary<string, byte[]>(StringComparer.Ordinal);

            foreach (string relative in writes.Select(file => file.Key).Concat(deletes).Distinct())
            {
                string path = InGame(root, relative);
                backups[path] = File.Exists(path) ? File.ReadAllBytes(path) : null;

                if (deletes.Contains(relative))
                {
                    backups[path + ".meta"] = File.Exists(path + ".meta") ? File.ReadAllBytes(path + ".meta") : null;
                }
            }

            File.WriteAllText(staging + "/backup.json", Json(backups), Utf8);
            EditorApplication.LockReloadAssemblies();
            AssetDatabase.DisallowAutoRefresh();
            AssetDatabase.StartAssetEditing();
            bool safeToRemoveBackup = false;

            try
            {
                foreach (var file in writes)
                {
                    string stagedPath = Path.Combine(staging, file.Key);
                    Directory.CreateDirectory(Path.GetDirectoryName(stagedPath));
                    File.WriteAllText(stagedPath, file.Value, Utf8);
                }

                foreach (var file in writes)
                {
                    string path = InGame(root, file.Key);
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    File.Copy(Path.Combine(staging, file.Key), path, true);
                }

                foreach (string relative in deletes)
                {
                    string path = InGame(root, relative);
                    File.Delete(path);
                    File.Delete(path + ".meta");
                }

                safeToRemoveBackup = true;
                return writes.Count + deletes.Count;
            }
            catch (Exception writeError)
            {
                var recoveryErrors = new List<Exception>();

                foreach (var backup in backups)
                {
                    try
                    {
                        if (backup.Value == null)
                        {
                            File.Delete(backup.Key);
                        }
                        else
                        {
                            File.WriteAllBytes(backup.Key, backup.Value);
                        }
                    }
                    catch (Exception recoveryError)
                    {
                        recoveryErrors.Add(recoveryError);
                    }
                }

                if (recoveryErrors.Count > 0)
                {
                    throw new ConfigRecoveryException("Update failed and some files could not be restored. "
                        + "Stop editing and restore the game from Git or the backup at " + staging + ".", writeError);
                }

                safeToRemoveBackup = true;
                throw;
            }
            finally
            {
                FinishEditorOperation(AssetDatabase.StopAssetEditing);
                FinishEditorOperation(AssetDatabase.AllowAutoRefresh);
                FinishEditorOperation(EditorApplication.UnlockReloadAssemblies);

                if (safeToRemoveBackup)
                {
                    FinishEditorOperation(() => Directory.Delete(staging, true));
                }

                FinishEditorOperation(() => AssetDatabase.Refresh());
            }
        }

        private static void FinishEditorOperation(Action action)
        {
            try
            {
                action();
            }
            catch (Exception exception)
            {
                UnityEngine.Debug.LogWarning("Config files finished updating, but Editor cleanup needs attention: " + exception.Message);
            }
        }
    }
}
