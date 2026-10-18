using UI;
using Core;
using Event;
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
        private EventManager _events;

        protected override void OnShow()
        {
            _settings = DemoGame.Instance.Data;
            _events = GameCore.Event;
            _events.RegisterEvent(DemoEvents.VolumeChanged, RefreshVolume);
            RefreshVolume();

            volumeSlider.onValueChanged.AddListener(SetVolume);
            testBtn.onClick.AddListener(TestSound);
            closeBtn.onClick.AddListener(CloseSelf);
        }

        private void SetVolume(float value)
        {
            _settings.SetMasterVolume(value);
        }

        private void RefreshVolume()
        {
            volumeSlider.SetValueWithoutNotify(_settings.MasterVolume);
            volumeText.text = $"Master volume  {Mathf.RoundToInt(_settings.MasterVolume * 100f)}%";
        }

        private void TestSound()
        {
            DemoGame.Instance.Level.PlayCue();
        }

        protected override void OnClose()
        {
            if (_events != null)
            {
                _events.UnRegisterEvent(DemoEvents.VolumeChanged, RefreshVolume);
            }

            _events = null;
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
