using UnityEngine;
using System.Collections.Generic;

namespace Sound
{
    public class SourceAgent : MonoBehaviour
    {
        private List<AudioSource> _audioSources;

        private void Start()
        {
            _audioSources = new List<AudioSource>();
            var audioSources = GetComponents<AudioSource>();
            _audioSources.AddRange(audioSources);
        }

        public AudioSource GetAudioSource()
        {
            // has valid one
            foreach (var audioSource in _audioSources)
            {
                if (!audioSource.isPlaying)
                    return audioSource;
            }

            // does not has any valid, instantiate a new one
            var newAudioSource = gameObject.AddComponent<AudioSource>();
            _audioSources.Add(newAudioSource);

            return newAudioSource;
        }

        public void StopAllSound()
        {
            foreach (var audioSource in _audioSources)
            {
                audioSource.Stop();
            }
        }

        public void PauseAllSound()
        {
            foreach (var audioSource in _audioSources)
            {
                audioSource.Pause();
            }
        }

        public void UnpauseAllSound()
        {
            foreach (var audioSource in _audioSources)
            {
                audioSource.UnPause();
            }
        }
    }
}