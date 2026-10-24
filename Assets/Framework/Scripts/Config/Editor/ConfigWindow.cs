using System;
using System.IO;
using System.Linq;
using System.Threading;
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
        [SerializeField] private bool _showSetup;
        private string _credentialPath = "";
        private string _report = "";
        private Vector2 _scroll;
        private CancellationTokenSource _cancellation;
        private bool _busy;

        private string CredentialKey => "FrameworkConfig.Key." + ConfigFiles.Hash(Application.dataPath + _gameRoot);

        [MenuItem("Framework/Config")]
        private static void Open()
        {
            var window = GetWindow<ConfigWindow>("Config");
            window.minSize = new Vector2(480, 380);
            window.Show();
        }

        private void OnEnable()
        {
            if (string.IsNullOrEmpty(_gameRoot) && Directory.Exists("Assets/GameContent"))
            {
                string source = Directory.GetFiles("Assets/GameContent", "ConfigSource.json", SearchOption.AllDirectories)
                    .OrderBy(path => path, StringComparer.Ordinal).FirstOrDefault();

                if (source != null)
                {
                    _gameRoot = ConfigFiles.GameRoot(source);
                }
            }

            ReadSettings();
            string reportPath = "Library/FrameworkConfig/Reports/latest.txt";

            if (File.Exists(reportPath))
            {
                _report = File.ReadAllText(reportPath);
            }
        }

        private void OnDisable()
        {
            _cancellation?.Cancel();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Game config", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Edit Google Sheets. Update here. Use the generated code.", EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space(10);

            using (new EditorGUI.DisabledScope(_busy))
            {
                var current = AssetDatabase.LoadAssetAtPath<DefaultAsset>(_gameRoot);
                var game = (DefaultAsset)EditorGUILayout.ObjectField("Game", current, typeof(DefaultAsset), false);

                if (game != current)
                {
                    string path = AssetDatabase.GetAssetPath(game);

                    if (game == null || (AssetDatabase.IsValidFolder(path) && path.StartsWith("Assets/GameContent/", StringComparison.Ordinal)
                        && path.Split('/').Length == 3))
                    {
                        _gameRoot = path;
                        ReadSettings();
                    }
                    else
                    {
                        ShowNotification(new GUIContent("Choose a game folder directly inside Assets/GameContent."));
                    }
                }

                _showSetup = EditorGUILayout.Foldout(_showSetup, "First-time setup", true);

                if (_showSetup)
                {
                    EditorGUI.indentLevel++;
                    _namespace = EditorGUILayout.TextField("Namespace", _namespace);
                    _folderId = EditorGUILayout.TextField("Google Drive Folder ID", _folderId);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel("Service Account JSON");

                    if (GUILayout.Button(string.IsNullOrEmpty(_credentialPath) ? "Choose file…" : Path.GetFileName(_credentialPath)))
                    {
                        string selected = EditorUtility.OpenFilePanel("Choose a service-account key outside the project", "", "json");

                        if (!string.IsNullOrEmpty(selected))
                        {
                            _credentialPath = selected;
                            EditorPrefs.SetString(CredentialKey, selected);
                        }
                    }

                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.HelpBox("Share the source folder with your service account as Viewer. "
                        + "The key stays outside the project. Only its local path is remembered.", MessageType.Info);

                    if (GUILayout.Button("Open setup guide"))
                    {
                        Application.OpenURL("https://github.com/AngusK97/MinimalUnityGameFramework/blob/main/docs/config.md#first-time-setup");
                    }

                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.Space(8);

                using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(_gameRoot)
                    || EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling))
                {
                    if (GUILayout.Button(_busy ? "Updating…" : "Update Config", GUILayout.Height(32)))
                    {
                        UpdateConfig();
                    }
                }
            }

            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
            {
                EditorGUILayout.HelpBox("Stop Play Mode and wait for compilation before updating.", MessageType.Info);
            }

            if (_busy && GUILayout.Button("Cancel"))
            {
                _cancellation.Cancel();
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Latest result", EditorStyles.boldLabel);
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            EditorGUILayout.SelectableLabel(string.IsNullOrEmpty(_report)
                ? "The Demo includes generated config. You can run it before connecting Google."
                : _report, EditorStyles.wordWrappedLabel,
                GUILayout.MinHeight(160), GUILayout.Height(Mathf.Max(160, EditorStyles.wordWrappedLabel.CalcHeight(
                    new GUIContent(_report), position.width - 35))));
            EditorGUILayout.EndScrollView();

            if (File.Exists("Library/FrameworkConfig/Reports/latest.txt") && GUILayout.Button("Open latest report"))
            {
                EditorUtility.RevealInFinder("Library/FrameworkConfig/Reports/latest.txt");
            }
        }

        private void ReadSettings()
        {
            _namespace = string.IsNullOrEmpty(_gameRoot) ? "" : Path.GetFileName(_gameRoot);
            _folderId = "";
            string path = _gameRoot + "/Editor/ConfigSource.json";

            if (File.Exists(path))
            {
                try
                {
                    var source = JsonConvert.DeserializeObject<ConfigSource>(File.ReadAllText(path));
                    _namespace = source.Namespace;
                    _folderId = source.FolderId;
                }
                catch (Exception)
                {
                    _report = "ConfigSource.json could not be read. Restore it from Git or enter the setup fields again.";
                }
            }

            _credentialPath = EditorPrefs.GetString(CredentialKey, "");
            _showSetup = string.IsNullOrEmpty(_folderId) || string.IsNullOrEmpty(_credentialPath);
        }

        private async void UpdateConfig()
        {
            _busy = true;
            _cancellation = new CancellationTokenSource();

            try
            {
                string path = _gameRoot + "/Editor/ConfigSource.json";
                ConfigFiles.GameRoot(path);
                var source = new ConfigSource { FolderId = _folderId.Trim(), Namespace = _namespace.Trim() };
                ConfigUpdater.ValidateSource(_gameRoot, source);

                if (!ConfigCompiler.ValidNamespace(source.Namespace))
                {
                    throw new InvalidOperationException("Use a C# namespace such as MyGame or MyGame.Config.");
                }

                var manifest = ConfigFiles.ReadManifest(_gameRoot);

                if (manifest != null && (manifest.Namespace != source.Namespace
                    || (!string.IsNullOrEmpty(manifest.FolderId) && manifest.FolderId != source.FolderId)))
                {
                    throw new InvalidOperationException("This game already has a different source or namespace. "
                        + "Keep its settings or choose a new game folder.");
                }

                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path, ConfigFiles.Json(source), ConfigFiles.Utf8);
                var result = await ConfigUpdater.UpdateAsync(path, _credentialPath, _cancellation.Token);
                _report = result.Describe() + "\n\nReport: " + result.ReportPath;
            }
            catch (Exception exception)
            {
                var result = ConfigUpdater.ReportFailure(exception);
                _report = result.Describe() + "\n\nReport: " + result.ReportPath;
            }
            finally
            {
                _busy = false;
                _cancellation.Dispose();
                _cancellation = null;

                if (this != null)
                {
                    Repaint();
                }
            }
        }
    }
}
