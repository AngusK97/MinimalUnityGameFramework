using System;
using System.Collections;
using Core;
using Event;
using Resource;
using Sound;
using UI;
using UnityEngine;

namespace Demo
{
    public class ExampleLevelController : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private Transform goal;
        [SerializeField] private Camera worldCamera;
        [SerializeField] private float moveSpeed = 4f;

        private bool _ready;
        private SoundManager _sound;

        public float ElapsedSeconds { get; private set; }
        public bool HasWon { get; private set; }

        public IEnumerator Prepare()
        {
            if (player == null || goal == null || worldCamera == null)
            {
                throw new InvalidOperationException("Example level requires player, goal and world camera.");
            }

            _sound = GameCore.Sound;
            var main = GameCore.Resource.GetResourceModule<MainResourceModule>(DemoResources.Main);

            if (main == null)
            {
                throw new InvalidOperationException("Main resources are not registered.");
            }

            var loaded = false;
            var failed = false;
            main.LoadResources(() => loaded = true, () => failed = true);

            while (!loaded && !failed)
            {
                yield return null;
            }

            if (!loaded || main.GetRes<AudioClip>(MainResource.Cue) == null)
            {
                throw new InvalidOperationException("Example sound could not be prepared.");
            }

            var finished = false;
            UIBase view = null;

            try
            {
                GameCore.UI.OpenUI(UILayer.First, DemoUI.Level, this, result =>
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
                    throw new InvalidOperationException("Example HUD could not be prepared.");
                }

                _ready = true;
            }
            finally
            {
                if (!finished && GameCore.UI != null)
                {
                    GameCore.UI.CloseUI(DemoUI.Level);
                }
            }
        }

        private void Update()
        {
            if (worldCamera != null && Screen.height > 0)
            {
                worldCamera.orthographicSize = Mathf.Max(5f, 8f / worldCamera.aspect);
            }

            if (!_ready || GameCore.Scene == null || GameCore.Scene.IsTransitioning)
            {
                return;
            }

            if (HasWon || GameCore.UI.IsOpen(DemoUI.Settings))
            {
                return;
            }

            ElapsedSeconds += Time.deltaTime;
            var direction = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
            var position = player.position + Vector3.ClampMagnitude(direction, 1f) * (moveSpeed * Time.deltaTime);
            position.x = Mathf.Clamp(position.x, -6f, 6f);
            position.z = Mathf.Clamp(position.z, -3f, 3f);
            player.position = position;
            var offset = position - goal.position;
            offset.y = 0f;

            if (offset.sqrMagnitude <= 0.75f * 0.75f)
            {
                HasWon = true;
                GameCore.Event.DispatchNow(this, DemoEvents.LevelCompleted);
                DemoGame.Instance.Level.PlayCue(SoundLayer.Event);
            }
        }

        private void OnDestroy()
        {
            if (_sound != null)
            {
                _sound.StopSound(SoundLayer.Event);
            }
        }
    }
}
