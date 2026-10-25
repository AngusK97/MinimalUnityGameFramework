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
        [SerializeField] private int _sourceIndex;
        [SerializeField] private string _localFolder = "Editor/Tables/Source";
        private string _credentialPath = "";
        private string _settingsError = "";

        [MenuItem("Framework/Config")]
        public static void OpenSetup()
        {
            bool isOpen = HasOpenInstances<ConfigWindow>();
            var window = GetWindow<ConfigWindow>(true, "Config");
            window.minSize = new Vector2(560, 380);

            if (!isOpen)
            {
                window.position = new Rect(window.position.position, new Vector2(600, 410));
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
            EditorGUILayout.LabelField("Configuration tables", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Set up once. Then use Update Config on the left side of the top toolbar.", EditorStyles.wordWrappedLabel);
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
                    _sourceIndex = EditorGUILayout.Popup("Source", _sourceIndex, new[] { "Local Excel", "Google Sheets" });

                    if (_sourceIndex == 0)
                    {
                        _localFolder = EditorGUILayout.TextField("Excel folder", _localFolder);

                        if (GUILayout.Button("Choose folder…"))
                        {
                            string selected = EditorUtility.OpenFolderPanel("Choose an Excel folder inside this game's Editor folder",
                                Path.GetFullPath(_gameRoot + "/Editor"), "");

                            if (!string.IsNullOrEmpty(selected))
                            {
                                string root = Path.GetFullPath(_gameRoot) + Path.DirectorySeparatorChar;

                                if (selected.StartsWith(root, StringComparison.Ordinal))
                                {
                                    _localFolder = selected.Substring(root.Length).Replace('\\', '/');
                                }
                                else
                                {
                                    ShowNotification(new GUIContent("Choose a folder inside this game's Editor folder."));
                                }
                            }
                        }
                    }
                    else
                    {
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
                    }

                    EditorGUILayout.Space(8);

                    var source = new ConfigSource
                    {
                        Source = _sourceIndex == 0 ? "LocalExcel" : "GoogleSheets",
                        LocalFolder = _localFolder.Trim().TrimEnd('/'),
                        FolderId = _folderId.Trim(),
                        Namespace = _namespace.Trim()
                    };
                    bool switching = false;

                    try
                    {
                        var manifest = ConfigFiles.ReadManifest(_gameRoot);
                        switching = manifest != null && !string.IsNullOrEmpty(manifest.Identity)
                            && manifest.Identity != source.Identity;
                    }
                    catch (Exception)
                    {
                        // The update reports invalid manifests without changing any files.
                    }

                    if (switching)
                    {
                        EditorGUILayout.HelpBox("Switching checks all table names, fields and types first. "
                            + "If any table fails, your previous source and output are kept.", MessageType.Info);
                    }

                    if (GUILayout.Button(switching ? "Switch Source" : "Update Config", GUILayout.Height(32)))
                    {
                        ConfigUpdateCommand.Run(_gameRoot, source, _credentialPath, switching);
                    }
                }
            }

            EditorGUILayout.Space(8);

            if (!string.IsNullOrEmpty(_settingsError))
            {
                EditorGUILayout.HelpBox(_settingsError, MessageType.Warning);
            }

            if (_sourceIndex == 1 && !string.IsNullOrEmpty(_credentialPath) && !File.Exists(_credentialPath))
            {
                EditorGUILayout.HelpBox("The selected key file is missing. Choose it again.", MessageType.Warning);
            }

            EditorGUILayout.HelpBox(_sourceIndex == 0
                ? "Save .xlsx files directly in the Excel folder. Each worksheet is a table. Use values, not formulas."
                : "Share the Google Drive folder with your service account as Viewer. Keep its key outside the project.", MessageType.Info);

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
            _sourceIndex = 0;
            _localFolder = "Editor/Tables/Source";
            _settingsError = "";
            string path = _gameRoot + "/Editor/ConfigSource.json";

            if (File.Exists(path))
            {
                try
                {
                    var source = JsonConvert.DeserializeObject<ConfigSource>(File.ReadAllText(path));
                    _namespace = source?.Namespace ?? _namespace;
                    _folderId = source?.FolderId ?? "";
                    _sourceIndex = source?.Kind == "GoogleSheets" ? 1 : 0;
                    _localFolder = source?.LocalFolder ?? "Editor/Tables/Source";
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
