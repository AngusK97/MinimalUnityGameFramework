using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine;

namespace FrameworkConfig.Editor
{
    [InitializeOnLoad]
    internal static class ConfigToolbar
    {
        private const string ElementId = "Framework/Update Config";
        private static readonly string InitializationKey = "FrameworkConfig.ToolbarInitialized."
            + ConfigFiles.Hash(Application.dataPath);
        private static bool _initialized;
        private static bool _canUpdate;
        private static double _nextInitializationAttempt;

        static ConfigToolbar()
        {
            if (Application.isBatchMode)
            {
                return;
            }

            _initialized = EditorPrefs.GetBool(InitializationKey, false);
            _canUpdate = ConfigUpdateCommand.CanUpdate;
            EditorApplication.update += Update;
        }

        // Unity 6.6 accepts -1 before Play's index 0; this uses Unity's reserved index range.
        [MainToolbarElement(ElementId, defaultDockPosition = MainToolbarDockPosition.Middle, defaultDockIndex = -1)]
        private static MainToolbarElement CreateButton()
        {
            return new MainToolbarButton(
                new MainToolbarContent("Update Config", "Update configuration from Google Sheets."),
                ConfigUpdateCommand.RunFromToolbar)
            {
                enabled = ConfigUpdateCommand.CanUpdate
            };
        }

        private static void Update()
        {
            bool canUpdate = ConfigUpdateCommand.CanUpdate;

            if (_canUpdate != canUpdate)
            {
                _canUpdate = canUpdate;
                MainToolbar.Refresh(ElementId);
            }

            if (_initialized || EditorApplication.isCompiling || EditorApplication.isUpdating
                || EditorApplication.timeSinceStartup < _nextInitializationAttempt)
            {
                return;
            }

            _nextInitializationAttempt = EditorApplication.timeSinceStartup + 0.5;

            foreach (var window in Resources.FindObjectsOfTypeAll<EditorWindow>())
            {
                if (window.overlayCanvas == null || !window.TryGetOverlay(ElementId, out var overlay))
                {
                    continue;
                }

                // Custom toolbar elements start hidden. Show once per project, then respect the user's layout.
                overlay.displayed = true;
                EditorPrefs.SetBool(InitializationKey, true);
                _initialized = true;
                break;
            }
        }
    }
}
