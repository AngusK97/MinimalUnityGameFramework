using System;
using System.Collections;
using Core;
using Sound;
using UI;
using UnityEngine;
using UnityScene = UnityEngine.SceneManagement.Scene;

namespace Demo
{
    // This template's navigation example. Replace its content when making your game.
    public class LevelManager
    {

        private readonly MainResources _main;

        public LevelManager(MainResources main)
        {
            _main = main ?? throw new ArgumentNullException(nameof(main));
        }

        public void EnterLevel()
        {
            GameCore.Scene.TryTransition(SceneName.Example, PrepareLevel);
        }

        public void RestartLevel()
        {
            EnterLevel();
        }

        public void ReturnToMenu()
        {
            GameCore.Scene.TryTransition(SceneName.Menu, scene => PrepareMenu());
        }

        public void OpenSettings()
        {
            if (!GameCore.Scene.IsTransitioning)
            {
                GameCore.UI.OpenUI(UILayer.Second, UIName.Settings);
            }
        }

        public void PlayCue(SoundLayer layer = SoundLayer.UI)
        {
            GameCore.Sound.PlaySound(layer, _main.Cue);
        }

        private static IEnumerator PrepareLevel(UnityScene scene)
        {
            ExampleLevelController controller = null;

            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var found in root.GetComponentsInChildren<ExampleLevelController>())
                {
                    if (controller != null)
                    {
                        throw new InvalidOperationException("Example scene requires exactly one controller.");
                    }

                    controller = found;
                }
            }

            if (controller == null)
            {
                throw new InvalidOperationException("Example scene has no ExampleLevelController.");
            }

            yield return controller.Prepare();
        }

        private static IEnumerator PrepareMenu()
        {
            var finished = false;
            UIBase view = null;

            try
            {
                GameCore.UI.OpenUI(UILayer.First, UIName.Menu, callback: result =>
                {
                    view = result;
                    finished = true;
                });

                while (!finished)
                {
                    yield return null;
                }

                if (view == null)
                {
                    throw new InvalidOperationException("Menu could not be prepared.");
                }
            }
            finally
            {
                if (!finished && GameCore.UI != null)
                {
                    GameCore.UI.CloseUI(UIName.Menu);
                }
            }
        }

        public void OnUpdate(float deltaTime, float unscaledDeltaTime)
        {
            if (!Input.GetKeyDown(KeyCode.Escape) || GameCore.Scene.IsTransitioning)
            {
                return;
            }

            if (GameCore.UI.IsOpen(UIName.Settings))
            {
                GameCore.UI.CloseUI(UIName.Settings);
            }
            else
            {
                OpenSettings();
            }
        }

    }
}
