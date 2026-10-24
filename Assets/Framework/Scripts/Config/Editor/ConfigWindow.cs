using System;
using System.IO;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace FrameworkConfig.Editor
{
    internal sealed class ConfigWindow : EditorWindow
    {
        [SerializeField] private string _gameRoot = "";
        [SerializeField] private string _namespace = "";
        [SerializeField] private string _folderId = "";
        private string _credentialPath = "";
        private string _settingsError = "";

        [MenuItem("Framework/Config")]
        public static void OpenSetup()
        {
            bool isOpen = HasOpenInstances<ConfigWindow>();
            var window = GetWindow<ConfigWindow>(true, "Config");
            window.minSize = new Vector2(520, 320);

            if (!isOpen)
            {
                window.position = new Rect(window.position.position, new Vector2(560, 360));
            }

            window.ShowUtility();
        }

        private void OnEnable()
        {
            _gameRoot = ConfigUpdateCommand.SelectedGame();
            ReadSettings();
        }

        private void OnGUI()
        {
            if (!string.IsNullOrEmpty(_gameRoot) && !ConfigUpdateCommand.IsGameFolder(_gameRoot))
            {
                _gameRoot = "";
                ReadSettings();
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Connect Google Sheets", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Set up once. Then use Update Config beside Play.", EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space(10);

            using (new EditorGUI.DisabledScope(!ConfigUpdateCommand.CanUpdate))
            {
                var current = AssetDatabase.LoadAssetAtPath<DefaultAsset>(_gameRoot);
                var game = (DefaultAsset)EditorGUILayout.ObjectField("Game", current, typeof(DefaultAsset), false);

                if (game != current)
                {
                    string path = AssetDatabase.GetAssetPath(game);

                    if (game == null || ConfigUpdateCommand.IsGameFolder(path))
                    {
                        _gameRoot = path;
                        ConfigUpdateCommand.SelectGame(path);
                        ReadSettings();
                    }
                    else
                    {
                        ShowNotification(new GUIContent("Choose a game folder directly inside Assets/GameContent."));
                    }
                }

                using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(_gameRoot)))
                {
                    _namespace = EditorGUILayout.TextField("Namespace", _namespace);
                    _folderId = EditorGUILayout.TextField("Google Drive Folder ID", _folderId);

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.PrefixLabel("Service Account JSON");

                        if (GUILayout.Button(string.IsNullOrEmpty(_credentialPath) ? "Choose file…" : Path.GetFileName(_credentialPath)))
                        {
                            string selected = EditorUtility.OpenFilePanel("Choose a service-account key outside the project", "", "json");

                            if (!string.IsNullOrEmpty(selected))
                            {
                                _credentialPath = selected;
                                EditorPrefs.SetString(ConfigUpdateCommand.CredentialKey(_gameRoot), selected);
                            }
                        }
                    }

                    EditorGUILayout.Space(8);

                    if (GUILayout.Button("Update Config", GUILayout.Height(32)))
                    {
                        var source = new ConfigSource { FolderId = _folderId.Trim(), Namespace = _namespace.Trim() };
                        ConfigUpdateCommand.Run(_gameRoot, source, _credentialPath);
                    }
                }
            }

            EditorGUILayout.Space(8);

            if (!string.IsNullOrEmpty(_settingsError))
            {
                EditorGUILayout.HelpBox(_settingsError, MessageType.Warning);
            }

            if (!string.IsNullOrEmpty(_credentialPath) && !File.Exists(_credentialPath))
            {
                EditorGUILayout.HelpBox("The selected key file is missing. Choose it again.", MessageType.Warning);
            }

            EditorGUILayout.HelpBox("Share the Google Drive folder with your service account as Viewer. "
                + "Keep its key outside the project. This computer remembers the game and key path.", MessageType.Info);

            if (!ConfigUpdateCommand.CanUpdate)
            {
                EditorGUILayout.HelpBox("Wait for the current update, import or compilation to finish, and stop Play Mode.", MessageType.Info);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Open setup guide"))
                {
                    Application.OpenURL("https://github.com/AngusK97/MinimalUnityGameFramework/blob/main/docs/config-setup.md");
                }

                using (new EditorGUI.DisabledScope(!File.Exists("Library/FrameworkConfig/Reports/latest.txt")))
                {
                    if (GUILayout.Button("Open latest report"))
                    {
                        EditorUtility.RevealInFinder("Library/FrameworkConfig/Reports/latest.txt");
                    }
                }
            }
        }

        private void ReadSettings()
        {
            _namespace = string.IsNullOrEmpty(_gameRoot) ? "" : Path.GetFileName(_gameRoot);
            _folderId = "";
            _settingsError = "";
            string path = _gameRoot + "/Editor/ConfigSource.json";

            if (File.Exists(path))
            {
                try
                {
                    var source = JsonConvert.DeserializeObject<ConfigSource>(File.ReadAllText(path));
                    _namespace = source?.Namespace ?? _namespace;
                    _folderId = source?.FolderId ?? "";
                }
                catch (Exception)
                {
                    _settingsError = "ConfigSource.json could not be read. Restore it from Git or enter the setup fields again.";
                }
            }

            _credentialPath = EditorPrefs.GetString(ConfigUpdateCommand.CredentialKey(_gameRoot), "");
        }
    }
}
