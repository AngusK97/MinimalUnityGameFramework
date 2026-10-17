using UnityEngine;

namespace Demo
{
    public class DataManager
    {
        public const string VolumePreferenceKey = "MinimalUnityGameFramework.MasterVolume";

        public float MasterVolume { get; private set; } = 1f;

        public void Init()
        {
            SetMasterVolume(PlayerPrefs.GetFloat(VolumePreferenceKey, 1f));
        }

        public void SetMasterVolume(float volume)
        {
            if (float.IsNaN(volume) || float.IsInfinity(volume))
            {
                volume = 1f;
            }

            MasterVolume = Mathf.Clamp01(volume);
            AudioListener.volume = MasterVolume;
        }

        public void SaveSettings()
        {
            PlayerPrefs.SetFloat(VolumePreferenceKey, MasterVolume);
            PlayerPrefs.Save();
        }
    }
}
