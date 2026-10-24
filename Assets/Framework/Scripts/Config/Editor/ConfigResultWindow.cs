using System.IO;
using UnityEditor;
using UnityEngine;

namespace FrameworkConfig.Editor
{
    internal sealed class ConfigResultWindow : EditorWindow
    {
        [SerializeField] private string _summary;
        [SerializeField] private string _details;
        [SerializeField] private string _reportPath;
        [SerializeField] private MessageType _messageType;
        private Vector2 _scroll;

        public static void Open(ConfigUpdateResult result)
        {
            bool isOpen = HasOpenInstances<ConfigResultWindow>();
            var window = GetWindow<ConfigResultWindow>(true, "Config result");
            window.minSize = new Vector2(500, 320);

            if (!isOpen)
            {
                window.position = new Rect(window.position.position, new Vector2(620, 380));
            }

            window._summary = result.Summary;
            window._details = string.Join("\n", result.Actions)
                + (result.Issues.Count == 0 ? "" : "\n\n" + string.Join("\n\n", result.Issues));
            window._reportPath = result.ReportPath;
            window._messageType = result.Failed ? MessageType.Error
                : result.Partial || result.Issues.Count > 0 ? MessageType.Warning : MessageType.Info;
            window.ShowUtility();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox(_summary ?? "", _messageType);
            EditorGUILayout.Space(8);
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            EditorGUILayout.SelectableLabel(_details ?? "", EditorStyles.wordWrappedLabel,
                GUILayout.Height(Mathf.Max(160, EditorStyles.wordWrappedLabel.CalcHeight(
                    new GUIContent(_details), position.width - 30))));
            EditorGUILayout.EndScrollView();
            EditorGUILayout.Space(8);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Config settings"))
                {
                    ConfigWindow.OpenSetup();
                }

                using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(_reportPath) || !File.Exists(_reportPath)))
                {
                    if (GUILayout.Button("Open report"))
                    {
                        EditorUtility.RevealInFinder(_reportPath);
                    }
                }

                if (GUILayout.Button("Close"))
                {
                    Close();
                }
            }

            EditorGUILayout.Space(8);
        }
    }
}
