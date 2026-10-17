using System;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Demo.Editor
{
    // Build the example without changing the project's own scene list.
    public static class DemoBuild
    {
        [MenuItem("Demo/Build Player")]
        public static void Build()
        {
            var options = BuildPlayerWindow.DefaultBuildMethods.GetBuildPlayerOptions(new BuildPlayerOptions());
            Build(options);
        }

        public static BuildReport Build(BuildPlayerOptions options)
        {
            options.scenes = new[] { "Assets/GameContent/Demo/Scenes/DemoStart.unity" };
            AddressableAssetSettings.BuildPlayerContent(out var content);

            if (!string.IsNullOrEmpty(content.Error))
            {
                throw new InvalidOperationException("Demo content build failed: " + content.Error);
            }

            var report = BuildPipeline.BuildPlayer(options);

            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException("Demo Player build failed: " + report.summary.result);
            }

            Debug.Log("Demo Player built: " + report.summary.outputPath);
            return report;
        }
    }
}
