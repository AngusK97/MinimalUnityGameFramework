using Core;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Views
{
    public class MenuView : UIBase
    {
        [SerializeField] private Button startBtn;
        [SerializeField] private Button resetBtn;
        [SerializeField] private Button quitBtn;

        protected override void OnShow()
        {
            startBtn.onClick.AddListener(StartLevel);
            resetBtn.onClick.AddListener(ResetGame);
            quitBtn.onClick.AddListener(QuiteGame);
        }

        protected override void OnClose()
        {
            startBtn.onClick.RemoveListener(StartLevel);
            resetBtn.onClick.RemoveListener(ResetGame);
            quitBtn.onClick.RemoveListener(QuiteGame);
        }

        private void StartLevel()
        {
            GameCore.Level.EnterLevel();
        }
        
        private void ResetGame()
        {
            GameCore.Data.Init();
        }

        private void QuiteGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}