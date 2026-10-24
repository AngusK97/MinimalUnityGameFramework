using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace FrameworkConfig.Editor
{
    internal sealed class ConfigBuildCheck : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            if (!Directory.Exists("Assets/GameContent"))
            {
                return;
            }

            try
            {
                foreach (string root in Directory.GetDirectories("Assets/GameContent"))
                {
                    string codeFolder = root + "/Scripts/Generated";
                    bool generatedCode = Directory.Exists(codeFolder)
                        && Directory.GetFiles(codeFolder, "*.g.cs").Any(path =>
                            File.ReadAllText(path).StartsWith(ConfigCompiler.GeneratedHeader, StringComparison.Ordinal));

                    if (File.Exists(root + "/Editor/ConfigSource.json")
                        || File.Exists(root + "/" + ConfigFiles.ManifestPath) || generatedCode)
                    {
                        Validate(root.Replace('\\', '/'));
                    }
                }
            }
            catch (Exception exception)
            {
                throw new BuildFailedException("Config is not ready for a Player build. " + exception.Message);
            }
        }

        public static void Validate(string root)
        {
            if (!File.Exists(root + "/Editor/ConfigSource.json"))
            {
                throw new InvalidOperationException(root + " is missing Editor/ConfigSource.json. Restore its source settings and generated files from the same Git revision.");
            }

            var manifest = ConfigFiles.ReadManifest(root);

            if (manifest == null)
            {
                throw new InvalidOperationException(root + " has no generated config. Click Update Config before building.");
            }

            var source = JsonConvert.DeserializeObject<ConfigSource>(File.ReadAllText(root + "/Editor/ConfigSource.json"));

            if (source == null || source.Namespace != manifest.Namespace
                || (!string.IsNullOrEmpty(manifest.FolderId) && source.FolderId != manifest.FolderId))
            {
                throw new InvalidOperationException(root + " source settings do not match its generated snapshot. Restore them from Git or update config.");
            }

            var files = ConfigFiles.ReadOwnedFiles(root, manifest);
            var issues = new List<ConfigIssue>();
            var models = new Dictionary<string, ConfigTableModel>(StringComparer.Ordinal);

            foreach (var table in manifest.Tables)
            {
                var model = ConfigCompiler.Parse(table.Source, issues);

                if (model != null)
                {
                    ConfigCompiler.ValidateAssets(model, issues);
                    models.Add(table.Source.Name, model);
                }
            }

            ConfigCompiler.ValidateReferences(models, issues);
            ConfigCompiler.ValidateNames(models, manifest.Namespace, issues, root);

            if (issues.Count > 0)
            {
                throw new InvalidOperationException("Fix the configuration issues below, then update config before building.\n"
                    + string.Join("\n\n", issues));
            }

            var expected = new Dictionary<string, string>(StringComparer.Ordinal);

            foreach (var model in models.Values)
            {
                foreach (var file in ConfigCompiler.Generate(model, models, manifest.Namespace))
                {
                    expected.Add(file.Key, file.Value);
                }

                expected.Add("Editor/Tables/Generated/" + model.Source.Name + ".csv", ConfigFiles.Csv(model.Source));
            }

            foreach (var file in ConfigCompiler.GenerateBootstrap(models, manifest.Namespace))
            {
                expected.Add(file.Key, file.Value);
            }

            if (expected.Count != files.Count || expected.Any(file => !files.TryGetValue(file.Key, out string value) || value != file.Value))
            {
                throw new InvalidOperationException(root + " generated config is stale. Click Update Config, then build again.");
            }

            string codeFolder = root + "/Scripts/Generated";

            foreach (string path in Directory.Exists(codeFolder) ? Directory.GetFiles(codeFolder, "*.g.cs") : Array.Empty<string>())
            {
                string relative = path.Substring(root.Length + 1).Replace('\\', '/');

                if (File.ReadAllText(path).StartsWith(ConfigCompiler.GeneratedHeader, StringComparison.Ordinal)
                    && !files.ContainsKey(relative))
                {
                    throw new InvalidOperationException("Generated code is not in the manifest: " + path
                        + ". Restore a matching snapshot from Git.");
                }
            }
        }
    }
}
