using UI;
using Data;
using Event;
using Level;
using Scene;
using Sound;
using Resource;
using UnityEngine;
using Resource.ResourceModules;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Core
{
    public partial class GameCore : MonoBehaviour
    {
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

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            InitManagers();
            StartGame();
        }
        
        private void InitManagers()
        {
            UI = uiManager;
            Data = dataManager;
            Event = eventManager;
            Level = levelManager;
            Scene = sceneManager;
            Sound = soundManager;
            Resource = resourceManager;

            RegisterAllViewModules();
            RegisterAllResourceModules();
        }

        private void Update()
        {
            UI.OnUpdate(Time.deltaTime, Time.unscaledDeltaTime);
            Event.OnUpdate(Time.deltaTime, Time.unscaledDeltaTime);
            Level.OnUpdate(Time.deltaTime, Time.unscaledDeltaTime);
        }
        
        private void FixedUpdate()
        {
            Level.OnFixedUpdate();
        }

        /// <summary>
        /// game logic starts here
        /// </summary>
        private void StartGame()
        {
            // mask screen
            UI.ShowTransition();
            
            Data.Init();
            
            // load main resource module
            var mainResourceModule = Resource.GetResourceModule<MainResourceModule>(ResourceModuleName.Main);
            mainResourceModule.LoadResources(() =>
            {
                // load menu scene
                Scene.LoadSceneAsync("Assets/GameResources/Scenes/MenuScene.unity").Completed += handle =>
                {
                    if (handle.Status != AsyncOperationStatus.Succeeded)
                    {
                        return;
                    }

                    // open menu ui
                    UI.OpenUI(UILayer.First, UIName.Menu, null, _ =>
                    {
                        // unmask screen
                        UI.TransitionFadeIn();
                    });
                };
            });
        }
    }
}