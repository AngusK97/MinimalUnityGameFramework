using UI;
using Core;
using UnityEngine;
using UnityEngine.UI;

namespace Demo
{
    public class SettingsView : UIBase
    {
        [SerializeField] private Slider volumeSlider;
        [SerializeField] private Text volumeText;
        [SerializeField] private Button testBtn;
        [SerializeField] private Button closeBtn;

        private DataManager _settings;

        protected override void OnShow()
        {
            _settings = DemoGame.Instance.Data;
            volumeSlider.SetValueWithoutNotify(_settings.MasterVolume);
            volumeText.text = $"Master volume  {Mathf.RoundToInt(_settings.MasterVolume * 100f)}%";
            volumeSlider.onValueChanged.AddListener(SetVolume);
            testBtn.onClick.AddListener(TestSound);
            closeBtn.onClick.AddListener(CloseSelf);
        }

        private void SetVolume(float value)
        {
            _settings.SetMasterVolume(value);
            volumeText.text = $"Master volume  {Mathf.RoundToInt(_settings.MasterVolume * 100f)}%";
        }

        private void TestSound()
        {
            DemoGame.Instance.Level.PlayCue();
        }

        protected override void OnClose()
        {
            volumeSlider.onValueChanged.RemoveListener(SetVolume);
            testBtn.onClick.RemoveListener(TestSound);
            closeBtn.onClick.RemoveListener(CloseSelf);

            if (_settings != null)
            {
                _settings.SaveSettings();
            }
        }
    }
}
