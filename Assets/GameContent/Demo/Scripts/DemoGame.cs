using Core;
using UI;
using UnityEngine;

namespace Demo
{
    // Game-specific assembly belongs here, not in the framework's Core.
    public class DemoGame : MonoBehaviour
    {
        public static DemoGame Instance { get; private set; }

        public DataManager Data { get; } = new DataManager();
        public LevelManager Level { get; } = new LevelManager();
        public string StartupError => _startup?.StartupError;

        private MenuStartup _startup;
        private bool _initialized;

        internal bool IsCurrent => this != null && _initialized && Instance == this && GameCore.IsReady;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (Instance != this)
            {
                return;
            }

            if (!GameCore.IsReady)
            {
                Debug.LogError("Demo requires the shared GameCore prefab in DemoStart.", this);
                enabled = false;
                return;
            }

            Data.Init();
            DemoUI.RegisterAll();
            GameCore.Resource.RegisterResourceModule(new MainResourceModule());

            _initialized = true;
            _startup = new MenuStartup(this, GameCore.UI, GameCore.Resource, GameCore.Scene);
            _startup.StartGame();
        }

        private void Update()
        {
            if (IsCurrent)
            {
                Level.OnUpdate(Time.deltaTime, Time.unscaledDeltaTime);
            }
        }

        public bool RetryStartup()
        {
            return IsCurrent && _startup != null && _startup.RetryStartup();
        }

        private void OnDestroy()
        {
            _initialized = false;

            if (Instance == this)
            {
                Instance = null;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Instance = null;
        }
    }
}
