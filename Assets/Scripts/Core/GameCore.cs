using UI;
using Data;
using Event;
using Level;
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
        public static DataManager Data { get; private set; }
        public static EventManager Event { get; private set; }
        public static LevelManager Level { get; private set; }
        public static SceneManager Scene { get; private set; }
        public static SoundManager Sound { get; private set; }
        public static ResourceManager Resource { get; private set; }

        [SerializeField] private UIManager uiManager;
        [SerializeField] private DataManager dataManager;
        [SerializeField] private EventManager eventManager;
        [SerializeField] private LevelManager levelManager;
        [SerializeField] private SceneManager sceneManager;
        [SerializeField] private SoundManager soundManager;
        [SerializeField] private ResourceManager resourceManager;
        [SerializeField] private bool autoStartGame = true;

        private bool _initialized;
        private MenuStartup _menuStartup;

        public string StartupError => _menuStartup?.StartupError;

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
                _menuStartup = new MenuStartup(this, uiManager, resourceManager, sceneManager);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
                ReleaseOwnership();
                RejectCore();
            }
        }

        private void Start()
        {
            if (autoStartGame && IsCurrent)
            {
                _menuStartup.StartGame();
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

            if (IsCurrent)
            {
                levelManager.OnUpdate(Time.deltaTime, Time.unscaledDeltaTime);
            }
        }

        private void FixedUpdate()
        {
            if (IsCurrent)
            {
                levelManager.OnFixedUpdate();
            }
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

            if (uiManager == null || dataManager == null || eventManager == null || levelManager == null
                || sceneManager == null || soundManager == null || resourceManager == null)
            {
                Debug.LogError("GameCore requires all seven manager references.", this);
                return false;
            }

            Component[] managers = { uiManager, dataManager, eventManager, levelManager,
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
            Data = dataManager;
            Event = eventManager;
            Level = levelManager;
            Scene = sceneManager;
            Sound = soundManager;
            Resource = resourceManager;

            resourceManager.Init();
            dataManager.Init();
            uiManager.Init();

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
            Data = null;
            Event = null;
            Level = null;
            Scene = null;
            Sound = null;
            Resource = null;
        }

        public bool RetryStartup()
        {
            return _menuStartup != null && _menuStartup.RetryStartup();
        }
    }
}
