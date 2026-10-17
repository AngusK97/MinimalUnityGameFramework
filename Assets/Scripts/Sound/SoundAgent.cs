using System;
using UnityEngine;
using System.Collections.Generic;

namespace Sound
{
    public class SourceAgent : MonoBehaviour
    {
        [SerializeField, Min(1)] private int maxSources = 16;

        private readonly List<AudioSource> _audioSources = new();
        private bool _initialized;
        private bool _isPaused;

        private void Awake()
        {
            EnsureInitialized();
        }

        public bool Play(AudioClip clip, float volume = 1f, float pitch = 1f, bool isLoop = false)
        {
            if (clip == null || !gameObject.activeInHierarchy
                || float.IsNaN(volume) || float.IsInfinity(volume)
                || float.IsNaN(pitch) || float.IsInfinity(pitch))
            {
                return false;
            }

            var source = GetAudioSource();

            if (source == null)
            {
                return false;
            }

            source.clip = clip;
            source.volume = Mathf.Clamp01(volume);
            source.pitch = Mathf.Clamp(pitch, -3f, 3f);
            source.loop = isLoop;
            source.Play();
            return true;
        }

        public AudioSource GetAudioSource()
        {
            EnsureInitialized();

            if (_isPaused)
            {
                return null;
            }

            foreach (var source in _audioSources)
            {
                if (!source.enabled || source.isPlaying
                    || (AudioListener.pause && !source.ignoreListenerPause))
                {
                    continue;
                }

                ResetPlayback(source);
                return source;
            }

            if (_audioSources.Count >= Mathf.Max(1, maxSources))
            {
                return null;
            }

            var template = _audioSources.Count > 0 ? _audioSources[0] : null;

            if (AudioListener.pause && (template == null || !template.ignoreListenerPause))
            {
                return null;
            }

            var newSource = gameObject.AddComponent<AudioSource>();

            if (template != null)
            {
                CopyConfiguration(template, newSource);
            }

            _audioSources.Add(newSource);
            ResetPlayback(newSource);
            return newSource;
        }

        public void StopAllSound()
        {
            EnsureInitialized();

            foreach (var source in _audioSources)
            {
                ResetPlayback(source);
            }
        }

        public void PauseAllSound()
        {
            EnsureInitialized();

            if (_isPaused)
            {
                return;
            }

            _isPaused = true;

            foreach (var source in _audioSources)
            {
                source.Pause();
            }
        }

        public void UnpauseAllSound()
        {
            EnsureInitialized();

            if (!_isPaused)
            {
                return;
            }

            _isPaused = false;

            foreach (var source in _audioSources)
            {
                source.UnPause();
            }
        }

        private void EnsureInitialized()
        {
            if (!_initialized)
            {
                _audioSources.AddRange(GetComponents<AudioSource>());
                _initialized = true;
            }

            for (var i = _audioSources.Count - 1; i >= 0; i--)
            {
                if (_audioSources[i] == null)
                {
                    _audioSources.RemoveAt(i);
                }
            }
        }

        private static void ResetPlayback(AudioSource source)
        {
            source.Stop();
            source.clip = null;
            source.loop = false;
            source.pitch = 1f;
            source.volume = 1f;
            source.playOnAwake = false;
        }

        private static void CopyConfiguration(AudioSource template, AudioSource source)
        {
            source.outputAudioMixerGroup = template.outputAudioMixerGroup;
            source.mute = template.mute;
            source.bypassEffects = template.bypassEffects;
            source.bypassListenerEffects = template.bypassListenerEffects;
            source.bypassReverbZones = template.bypassReverbZones;
            source.priority = template.priority;
            source.panStereo = template.panStereo;
            source.spatialBlend = template.spatialBlend;
            source.reverbZoneMix = template.reverbZoneMix;
            source.dopplerLevel = template.dopplerLevel;
            source.spread = template.spread;
            source.minDistance = template.minDistance;
            source.maxDistance = template.maxDistance;
            source.spatialize = template.spatialize;
            source.spatializePostEffects = template.spatializePostEffects;
            source.velocityUpdateMode = template.velocityUpdateMode;
            source.ignoreListenerPause = template.ignoreListenerPause;
            source.ignoreListenerVolume = template.ignoreListenerVolume;

            foreach (AudioSourceCurveType curveType in Enum.GetValues(typeof(AudioSourceCurveType)))
            {
                var curve = template.GetCustomCurve(curveType);

                if (curve != null)
                {
                    source.SetCustomCurve(curveType, curve);
                }
            }

            source.rolloffMode = template.rolloffMode;
        }
    }
}
