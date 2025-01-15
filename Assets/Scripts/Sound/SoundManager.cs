using System;
using UnityEngine;
using System.Collections.Generic;

namespace Sound
{
    public class SoundManager : MonoBehaviour
    {
        [Serializable]
        public class AgentInfo
        {
            public SoundLayer layer;
            public SourceAgent agent;
        }

        public List<AgentInfo> agents;

        public void PlaySound(SoundLayer layer, AudioClip clip)
        {
            if (clip == null)
            {
                return;
            }

            var agent = GetSourceAgent(layer);
            var audioSource = agent.GetAudioSource();
            audioSource.clip = clip;
            audioSource.Play();
        }

        public void Play(SoundLayer layer, AudioClip clip, float volume, float pitch = 1f, bool isLoop = false)
        {
            if (clip == null)
            {
                return;
            }

            var agent = GetSourceAgent(layer);
            var audioSource = agent.GetAudioSource();
            audioSource.loop = isLoop;
            audioSource.pitch = pitch;
            audioSource.volume = volume;
            audioSource.clip = clip;
            audioSource.Play();
        }

        public void StopSound(SoundLayer layer)
        {
            var agent = GetSourceAgent(layer);
            agent.StopAllSound();
        }

        public void PauseSound(SoundLayer layer)
        {
            var agent = GetSourceAgent(layer);
            agent.PauseAllSound();
        }

        public void UnPauseSound(SoundLayer layer)
        {
            var agent = GetSourceAgent(layer);
            agent.UnpauseAllSound();
        }

        private SourceAgent GetSourceAgent(SoundLayer layer)
        {
            foreach (var agentInfo in agents)
            {
                if (agentInfo.layer == layer)
                {
                    return agentInfo.agent;
                }
            }

            return null;
        }
    }
}