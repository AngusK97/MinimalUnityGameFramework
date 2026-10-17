using System;
using System.Collections;
using Core;
using Resource;
using Sound;
using UI;
using UnityEngine;
using UnityScene = UnityEngine.SceneManagement.Scene;

namespace Demo
{
    // This template's navigation example. Replace its content when making your game.
    public class LevelManager
    {
        public const string ExampleSceneKey = "Assets/GameContent/Demo/Content/Scenes/ExampleLevel.unity";
        private const string MenuSceneKey = "Assets/GameContent/Demo/Content/Scenes/MenuScene.unity";

        public void EnterLevel()
        {
            GameCore.Scene.TryTransition(ExampleSceneKey, PrepareLevel);
        }

        public void RestartLevel()
        {
            EnterLevel();
        }

        public void ReturnToMenu()
        {
            GameCore.Scene.TryTransition(MenuSceneKey, scene => PrepareMenu());
        }

        public void OpenSettings()
        {
            if (!GameCore.Scene.IsTransitioning)
            {
                GameCore.UI.OpenUI(UILayer.Second, DemoUI.Settings);
            }
        }

        public void PlayCue(SoundLayer layer = SoundLayer.UI)
        {
            var main = GameCore.Resource.GetResourceModule<MainResourceModule>(DemoResources.Main);
            var clip = main?.GetRes<AudioClip>(MainResource.Cue);
            GameCore.Sound.PlaySound(layer, clip);
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
                GameCore.UI.OpenUI(UILayer.First, DemoUI.Menu, callback: result =>
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
                    GameCore.UI.CloseUI(DemoUI.Menu);
                }
            }
        }

        public void OnUpdate(float deltaTime, float unscaledDeltaTime)
        {
            if (!Input.GetKeyDown(KeyCode.Escape) || GameCore.Scene.IsTransitioning)
            {
                return;
            }

            if (GameCore.UI.IsOpen(DemoUI.Settings))
            {
                GameCore.UI.CloseUI(DemoUI.Settings);
            }
            else
            {
                OpenSettings();
            }
        }

    }
}
