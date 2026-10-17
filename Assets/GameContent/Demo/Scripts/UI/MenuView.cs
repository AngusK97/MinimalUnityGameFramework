using UI;
using Core;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Demo
{
    public class MenuView : UIBase
    {
        [SerializeField] private Button startBtn;
        [FormerlySerializedAs("resetBtn")]
        [SerializeField] private Button settingsBtn;
        [SerializeField] private Button quitBtn;

        protected override void OnShow()
        {
            startBtn.onClick.AddListener(StartLevel);
            settingsBtn.onClick.AddListener(OpenSettings);
            quitBtn.onClick.AddListener(QuitGame);
        }

        protected override void OnClose()
        {
            startBtn.onClick.RemoveListener(StartLevel);
            settingsBtn.onClick.RemoveListener(OpenSettings);
            quitBtn.onClick.RemoveListener(QuitGame);
        }

        private void StartLevel()
        {
            DemoGame.Instance.Level.PlayCue();
            DemoGame.Instance.Level.EnterLevel();
        }

        private void OpenSettings()
        {
            DemoGame.Instance.Level.PlayCue();
            DemoGame.Instance.Level.OpenSettings();
        }

        private void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
