using UI;
using Event;
using Scene;
using Sound;
using System;
using Resource;
using UnityEngine;

namespace Core
{
    [DisallowMultipleComponent]
    public partial class GameCore : MonoBehaviour
    {
        private static GameCore _instance;

        public static UIManager UI { get; private set; }
        public static EventManager Event { get; private set; }
        public static SceneManager Scene { get; private set; }
        public static SoundManager Sound { get; private set; }
        public static ResourceManager Resource { get; private set; }

        [SerializeField] private UIManager uiManager;
        [SerializeField] private EventManager eventManager;
        [SerializeField] private SceneManager sceneManager;
        [SerializeField] private SoundManager soundManager;
        [SerializeField] private ResourceManager resourceManager;

        private bool _initialized;

        public static bool IsReady => _instance != null && _instance.IsCurrent;

        internal bool IsCurrent => this != null && _initialized && ReferenceEquals(_instance, this);

        private void Awake()
        {
            if (ReferenceEquals(_instance, this))
            {
                return;
            }

            if (_instance != null)
            {
                RejectCore();
                return;
            }

            if (!ValidateManagers())
            {
                RejectCore();
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            try
            {
                InitManagers();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
                ReleaseOwnership();
                RejectCore();
            }
        }

        private void Update()
        {
            if (!IsCurrent)
            {
                return;
            }

            uiManager.OnUpdate(Time.deltaTime, Time.unscaledDeltaTime);

            if (!IsCurrent)
            {
                return;
            }

            eventManager.OnUpdate(Time.deltaTime, Time.unscaledDeltaTime);
        }

        private void OnDestroy()
        {
            ReleaseOwnership();
        }

        private bool ValidateManagers()
        {
            if (transform.parent != null)
            {
                Debug.LogError("GameCore must be on a root GameObject.", this);
                return false;
            }

            if (uiManager == null || eventManager == null
                || sceneManager == null || soundManager == null || resourceManager == null)
            {
                Debug.LogError("GameCore requires all five manager references.", this);
                return false;
            }

            Component[] managers = { uiManager, eventManager,
                sceneManager, soundManager, resourceManager };

            foreach (var manager in managers)
            {
                if (manager.transform != transform && !manager.transform.IsChildOf(transform))
                {
                    Debug.LogError($"GameCore: {manager.GetType().Name} must belong to this Core's hierarchy.", this);
                    return false;
                }
            }

            return true;
        }

        private void InitManagers()
        {
            if (_initialized || !ReferenceEquals(_instance, this))
            {
                return;
            }

            UI = uiManager;
            Event = eventManager;
            Scene = sceneManager;
            Sound = soundManager;
            Resource = resourceManager;

            resourceManager.Init();
            uiManager.Init();
            uiManager.HideTransition();

            _initialized = true;
        }

        private void RejectCore()
        {
            enabled = false;

            // An extra Core on a service child (or an ancestor of the owner) must not destroy valid services.
            if (transform.parent != null || (_instance != null
                && (_instance.transform == transform || _instance.transform.IsChildOf(transform))))
            {
                Destroy(this);
                return;
            }

            gameObject.SetActive(false);
            Destroy(gameObject);
        }

        private void ReleaseOwnership()
        {
            _initialized = false;

            if (ReferenceEquals(_instance, this))
            {
                ResetStatics();
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _instance = null;
            UI = null;
            Event = null;
            Scene = null;
            Sound = null;
            Resource = null;
        }
    }
}
