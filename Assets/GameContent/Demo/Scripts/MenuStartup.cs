using System;
using System.Collections;
using UI;
using Resource;
using UnityScene = UnityEngine.SceneManagement.Scene;
using UnityEngine;

namespace Demo
{
    internal sealed class MenuStartup
    {
        private readonly DemoGame _owner;
        private readonly UIManager _uiManager;
        private readonly ResourceManager _resourceManager;
        private readonly Scene.SceneManager _sceneManager;

        private bool _startupStarted;
        private int _startupAttempt;
        private UnityScene _menuScene;

        public string StartupError { get; private set; }

        public MenuStartup(DemoGame owner, UIManager uiManager,
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
            return _startupStarted;
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
            var accepted = _sceneManager.TryTransition("Assets/GameContent/Demo/Content/Scenes/MenuScene.unity",
                _menuScene, () => PrepareResources(attempt), scene => OpenMenu(scene, attempt),
                (succeeded, error) =>
                {
                    if (!succeeded)
                    {
                        FailStartup(attempt, error);
                    }
                }, RetryStartup);

            if (!accepted)
            {
                FailStartup(attempt, "Startup transition could not be accepted; check Core/UI or an active transition.");
            }
        }

        private IEnumerator PrepareResources(int attempt)
        {
            var main = _resourceManager.GetResourceModule<MainResources>(MainResources.Id);

            if (main == null)
            {
                throw new InvalidOperationException("Startup resource module is not registered.");
            }

            var finished = false;
            var succeeded = false;
            main.LoadResources(() =>
            {
                succeeded = true;
                finished = true;
            }, () => finished = true);

            while (!finished && IsCurrentStartup(attempt))
            {
                yield return null;
            }

            if (!IsCurrentStartup(attempt) || !succeeded)
            {
                throw new InvalidOperationException("Startup resources failed to load or were cancelled.");
            }
        }

        private IEnumerator OpenMenu(UnityScene scene, int attempt)
        {
            _menuScene = scene;
            var finished = false;
            UIBase view = null;

            try
            {
                _uiManager.OpenUI(UILayer.First, DemoUI.Menu, callback: result =>
                {
                    view = result;
                    finished = true;
                });

                while (!finished && IsCurrentStartup(attempt))
                {
                    yield return null;
                }

                if (!IsCurrentStartup(attempt) || view == null)
                {
                    throw new InvalidOperationException("Startup menu failed to open or was cancelled.");
                }
            }
            finally
            {
                if (!finished && _uiManager != null)
                {
                    _uiManager.CloseUI(DemoUI.Menu);
                }
            }
        }

        private void FailStartup(int attempt, string message)
        {
            if (!IsCurrentStartup(attempt))
            {
                return;
            }

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
