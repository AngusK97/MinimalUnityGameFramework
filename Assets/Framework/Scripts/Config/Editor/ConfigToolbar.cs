using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.UIElements;

namespace FrameworkConfig.Editor
{
    [InitializeOnLoad]
    internal static class ConfigToolbar
    {
        // A new ID lets existing layouts adopt the left dock without resetting other controls.
        private const string ElementId = "Framework/Update configuration";
        private static readonly string InitializationKey = "FrameworkConfig.ToolbarLeftInitialized."
            + ConfigFiles.Hash(Application.dataPath);
        private static VisualElement _styledRoot;
        private static bool _canUpdate;
        private static double _nextInitializationAttempt;

        static ConfigToolbar()
        {
            if (Application.isBatchMode)
            {
                return;
            }

            _canUpdate = ConfigUpdateCommand.CanUpdate;
            EditorApplication.update += Update;
        }

        [MainToolbarElement(ElementId, defaultDockPosition = MainToolbarDockPosition.Left, defaultDockIndex = 101)]
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

            if ((_styledRoot != null && _styledRoot.panel != null)
                || EditorApplication.isCompiling || EditorApplication.isUpdating
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

                // Keep the native button's hover, pressed and disabled states over a visible backplate.
                var style = overlay.rootVisualElement.style;
                style.alignSelf = Align.Center;
                style.height = 20;
                style.backgroundColor = EditorGUIUtility.isProSkin
                    ? new Color(0.25f, 0.25f, 0.25f)
                    : new Color(0.82f, 0.82f, 0.82f);
                style.borderTopLeftRadius = 3;
                style.borderTopRightRadius = 3;
                style.borderBottomLeftRadius = 3;
                style.borderBottomRightRadius = 3;

                // Show once per project, then respect the user's visibility and layout choices.
                if (!EditorPrefs.GetBool(InitializationKey, false))
                {
                    overlay.displayed = true;
                    EditorPrefs.SetBool(InitializationKey, true);
                }

                _styledRoot = overlay.rootVisualElement;
                break;
            }
        }
    }
}
