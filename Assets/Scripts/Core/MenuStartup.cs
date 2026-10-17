using System;
using UI;
using Resource;
using Resource.ResourceModules;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace Core
{
    internal sealed class MenuStartup
    {
        private readonly GameCore _owner;
        private readonly UIManager _uiManager;
        private readonly ResourceManager _resourceManager;
        private readonly Scene.SceneManager _sceneManager;

        private bool _startupStarted;
        private int _startupAttempt;
        private UnityEngine.SceneManagement.Scene _menuScene;

        public string StartupError { get; private set; }

        public MenuStartup(GameCore owner, UIManager uiManager,
            ResourceManager resourceManager, Scene.SceneManager sceneManager)
        {
            _owner = owner;
            _uiManager = uiManager;
            _resourceManager = resourceManager;
            _sceneManager = sceneManager;
        }

        public bool RetryStartup()
        {
            if (!_owner.IsCurrent || _startupStarted || StartupError == null)
            {
                return false;
            }

            StartGame();
            return true;
        }

        public void StartGame()
        {
            if (!_owner.IsCurrent || _startupStarted)
            {
                return;
            }

            _startupStarted = true;
            StartupError = null;
            var attempt = ++_startupAttempt;

            try
            {
                _uiManager.ShowTransition();

                if (!IsCurrentStartup(attempt))
                {
                    return;
                }

                var main = _resourceManager.GetResourceModule<MainResourceModule>(ResourceModuleName.Main);

                if (main == null)
                {
                    FailStartup(attempt, "Startup resource module is not registered.");
                    return;
                }

                main.LoadResources(() => LoadMenuScene(attempt),
                    () => FailStartup(attempt, "Startup resources failed to load or were cancelled."));
            }
            catch (Exception exception)
            {
                FailStartup(attempt, $"Startup failed: {exception}");
            }
        }

        private void LoadMenuScene(int attempt)
        {
            if (!IsCurrentStartup(attempt))
            {
                return;
            }

            if (_menuScene.IsValid() && _menuScene.isLoaded)
            {
                OpenMenu(attempt);
                return;
            }

            try
            {
                _sceneManager.LoadSceneAsync("Assets/GameResources/Scenes/MenuScene.unity")
                    .Completed += handle => OnMenuSceneLoaded(handle, attempt);
            }
            catch (Exception exception)
            {
                FailStartup(attempt, $"Startup menu scene failed to load: {exception}");
            }
        }

        private void OnMenuSceneLoaded(AsyncOperationHandle<SceneInstance> handle, int attempt)
        {
            if (!handle.IsValid())
            {
                FailStartup(attempt, "Startup menu scene returned an invalid handle.");
                return;
            }

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                var reason = handle.OperationException?.Message;
                Addressables.Release(handle);
                FailStartup(attempt, $"Startup menu scene failed to load: {reason}");
                return;
            }

            // Successful scenes remain tracked by Addressables until the scene is actually unloaded.
            if (!IsCurrentStartup(attempt))
            {
                return;
            }

            _menuScene = handle.Result.Scene;
            OpenMenu(attempt);
        }

        private void OpenMenu(int attempt)
        {
            if (!IsCurrentStartup(attempt))
            {
                return;
            }

            try
            {
                _uiManager.OpenUI(UILayer.First, UIName.Menu,
                    callback: view => OnMenuOpened(view, attempt));
            }
            catch (Exception exception)
            {
                FailStartup(attempt, $"Startup menu failed to open: {exception}");
            }
        }

        private void OnMenuOpened(UIBase view, int attempt)
        {
            if (!IsCurrentStartup(attempt))
            {
                return;
            }

            if (view == null)
            {
                FailStartup(attempt, "Startup menu failed to open or was cancelled.");
                return;
            }

            try
            {
                _uiManager.TransitionFadeIn();
            }
            catch (Exception exception)
            {
                FailStartup(attempt, $"Startup transition failed: {exception}");
            }
        }

        private void FailStartup(int attempt, string message)
        {
            if (!IsCurrentStartup(attempt))
            {
                return;
            }

            // Finish old visual cleanup before a log listener or UI can request another attempt.
            _uiManager.HideTransition();

            StartupError = message;
            _startupStarted = false;

            Debug.LogError(message, _owner);
        }

        private bool IsCurrentStartup(int attempt)
        {
            return _owner.IsCurrent && _startupStarted && _startupAttempt == attempt;
        }
    }
}
