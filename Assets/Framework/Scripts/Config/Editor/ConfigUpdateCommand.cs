using System;
using System.IO;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace FrameworkConfig.Editor
{
    [InitializeOnLoad]
    internal static class ConfigUpdateCommand
    {
        private const string PendingResultKey = "FrameworkConfig.PendingResult";
        private static CancellationTokenSource _cancellation;
        private static string _progressMessage;
        private static float _progress;

        public static bool IsBusy => _cancellation != null;
        public static bool CanUpdate => !IsBusy && !EditorApplication.isPlayingOrWillChangePlaymode
            && !EditorApplication.isCompiling && !EditorApplication.isUpdating;

        private static string GameKey => "FrameworkConfig.Game." + ConfigFiles.Hash(Application.dataPath);

        static ConfigUpdateCommand()
        {
            if (!string.IsNullOrEmpty(SessionState.GetString(PendingResultKey, "")))
            {
                EditorApplication.delayCall += ShowPendingResult;
            }
        }

        public static string CredentialKey(string gameRoot)
        {
            return "FrameworkConfig.Key." + ConfigFiles.Hash(Application.dataPath + gameRoot);
        }

        public static string SelectedGame()
        {
            if (EditorPrefs.HasKey(GameKey))
            {
                string saved = EditorPrefs.GetString(GameKey);
                return IsGameFolder(saved) ? saved : "";
            }

            if (!Directory.Exists("Assets/GameContent"))
            {
                return "";
            }

            var games = Directory.GetDirectories("Assets/GameContent")
                .Where(path => File.Exists(path + "/Editor/ConfigSource.json")).ToArray();
            return games.Length == 1 ? games[0].Replace('\\', '/') : "";
        }

        public static bool IsGameFolder(string path)
        {
            return !string.IsNullOrEmpty(path) && path.StartsWith("Assets/GameContent/", StringComparison.Ordinal)
                && path.Split('/').Length == 3 && !path.Contains("..") && AssetDatabase.IsValidFolder(path);
        }

        public static void SelectGame(string gameRoot)
        {
            EditorPrefs.SetString(GameKey, gameRoot);
        }

        public static bool TryReadSettings(string gameRoot, out ConfigSource source, out string credentialPath)
        {
            source = null;
            credentialPath = EditorPrefs.GetString(CredentialKey(gameRoot), "");

            if (!IsGameFolder(gameRoot))
            {
                return false;
            }

            try
            {
                source = JsonConvert.DeserializeObject<ConfigSource>(File.ReadAllText(gameRoot + "/Editor/ConfigSource.json"));
                if (source == null || !ConfigCompiler.ValidNamespace(source.Namespace))
                {
                    return false;
                }

                return source.Kind == "LocalExcel"
                    ? Directory.Exists(ConfigFiles.LocalSourceFolder(gameRoot, source))
                    : source.Kind == "GoogleSheets" && !string.IsNullOrWhiteSpace(source.FolderId) && File.Exists(credentialPath);
            }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException || exception is JsonException
                || exception is InvalidOperationException || exception is ArgumentException)
            {
                return false;
            }
        }

        public static void RunFromToolbar()
        {
            if (!CanUpdate)
            {
                return;
            }

            string gameRoot = SelectedGame();

            if (!TryReadSettings(gameRoot, out var source, out var credentialPath))
            {
                ConfigWindow.OpenSetup();
                return;
            }

            Run(gameRoot, source, credentialPath);
        }

        public static async void Run(string gameRoot, ConfigSource source, string credentialPath, bool switchSource = false)
        {
            if (!CanUpdate)
            {
                return;
            }

            _cancellation = new CancellationTokenSource();
            _progressMessage = "Checking settings…";
            _progress = 0;
            EditorApplication.LockReloadAssemblies();
            EditorApplication.update += DrawProgress;
            EditorApplication.quitting += Cancel;

            try
            {
                DrawProgress();

                if (!IsGameFolder(gameRoot))
                {
                    throw new InvalidOperationException("Choose an existing game folder directly inside Assets/GameContent.");
                }

                string path = gameRoot + "/Editor/ConfigSource.json";
                ConfigFiles.GameRoot(path);
                ConfigUpdater.ValidateSource(gameRoot, source);

                if (!ConfigCompiler.ValidNamespace(source.Namespace))
                {
                    throw new InvalidOperationException("Use a C# namespace such as MyGame or MyGame.Config.");
                }

                var manifest = ConfigFiles.ReadManifest(gameRoot);
                ConfigUpdater.ValidateSourceChange(manifest, source, switchSource);
                SelectGame(gameRoot);
                var result = await ConfigUpdater.UpdateAsync(path, credentialPath, _cancellation.Token, SetProgress,
                    source, switchSource);
                SessionState.SetString(PendingResultKey, ConfigFiles.Json(result));
            }
            catch (Exception exception)
            {
                SessionState.SetString(PendingResultKey, ConfigFiles.Json(ConfigUpdater.ReportFailure(exception)));
            }
            finally
            {
                EditorApplication.update -= DrawProgress;
                EditorApplication.quitting -= Cancel;
                EditorUtility.ClearProgressBar();
                _cancellation.Dispose();
                _cancellation = null;
                EditorApplication.UnlockReloadAssemblies();
                EditorApplication.delayCall += ShowPendingResult;
            }
        }

        private static void SetProgress(string message, float value)
        {
            _progressMessage = message;
            _progress = value;
            DrawProgress();
        }

        private static void DrawProgress()
        {
            if (_progress >= 0.85f)
            {
                // Applying files is a transaction: cancellation is only offered before it starts.
                EditorUtility.DisplayProgressBar("Update Config", _progressMessage, _progress);
            }
            else if (EditorUtility.DisplayCancelableProgressBar("Update Config",
                _cancellation.IsCancellationRequested ? "Cancelling…" : _progressMessage, _progress))
            {
                _cancellation.Cancel();
            }
        }

        private static void Cancel()
        {
            _cancellation?.Cancel();
            EditorUtility.ClearProgressBar();
        }

        private static void ShowPendingResult()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += ShowPendingResult;
                return;
            }

            string json = SessionState.GetString(PendingResultKey, "");

            if (string.IsNullOrEmpty(json))
            {
                return;
            }

            SessionState.EraseString(PendingResultKey);

            try
            {
                var result = JsonConvert.DeserializeObject<ConfigUpdateResult>(json);

                if (result != null && result.Actions != null && result.Issues != null)
                {
                    ConfigResultWindow.Open(result);
                }
            }
            catch (JsonException)
            {
                Debug.LogWarning("The saved result could not be opened. Read the latest report in Framework > Config.");
            }
        }
    }
}
