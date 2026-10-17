using UI;
using Core;
using Event;
using UnityEngine;
using UnityEngine.UI;

namespace Demo
{
    public class LevelView : UIBase
    {
        [SerializeField] private Text timerText;
        [SerializeField] private Text resultText;
        [SerializeField] private GameObject victoryPanel;
        [SerializeField] private Button restartBtn;
        [SerializeField] private Button menuBtn;
        [SerializeField] private Button settingsBtn;

        private ExampleLevelController _level;
        private EventManager _events;

        protected override void OnInit()
        {
            _level = Data as ExampleLevelController;
        }

        protected override void OnShow()
        {
            _events = GameCore.Event;
            _events.RegisterEvent(DemoEvents.LevelCompleted, OnLevelCompleted);
            restartBtn.onClick.AddListener(Restart);
            menuBtn.onClick.AddListener(ReturnToMenu);
            settingsBtn.onClick.AddListener(OpenSettings);
            victoryPanel.SetActive(false);
        }

        protected override void OnUpdate()
        {
            if (_level != null)
            {
                timerText.text = $"Time  {_level.ElapsedSeconds:0.0}s";
            }
        }

        private void OnLevelCompleted(object sender, string type, BaseEventArgs args)
        {
            if (_level == null || !ReferenceEquals(sender, _level))
            {
                return;
            }

            resultText.text = $"GOAL REACHED\n{_level.ElapsedSeconds:0.0} seconds";
            victoryPanel.SetActive(true);
        }

        private void Restart()
        {
            DemoGame.Instance.Level.PlayCue();
            DemoGame.Instance.Level.RestartLevel();
        }

        private void ReturnToMenu()
        {
            DemoGame.Instance.Level.PlayCue();
            DemoGame.Instance.Level.ReturnToMenu();
        }

        private void OpenSettings()
        {
            DemoGame.Instance.Level.PlayCue();
            DemoGame.Instance.Level.OpenSettings();
        }

        protected override void OnClose()
        {
            if (_events != null)
            {
                _events.UnRegisterEvent(DemoEvents.LevelCompleted, OnLevelCompleted);
            }

            restartBtn.onClick.RemoveListener(Restart);
            menuBtn.onClick.RemoveListener(ReturnToMenu);
            settingsBtn.onClick.RemoveListener(OpenSettings);
        }
    }
}
