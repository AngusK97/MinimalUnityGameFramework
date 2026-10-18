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
            Play(layer, clip, 1f);
        }

        public void Play(SoundLayer layer, AudioClip clip, float volume, float pitch = 1f, bool isLoop = false)
        {
            if (clip == null)
            {
                return;
            }

            var agent = GetSourceAgent(layer);

            if (agent != null)
            {
                agent.Play(clip, volume, pitch, isLoop);
            }
        }

        public void StopSound(SoundLayer layer)
        {
            var agent = GetSourceAgent(layer);

            if (agent != null)
            {
                agent.StopAllSound();
            }
        }

        public void PauseSound(SoundLayer layer)
        {
            var agent = GetSourceAgent(layer);

            if (agent != null)
            {
                agent.PauseAllSound();
            }
        }

        public void UnPauseSound(SoundLayer layer)
        {
            var agent = GetSourceAgent(layer);

            if (agent != null)
            {
                agent.UnpauseAllSound();
            }
        }

        private SourceAgent GetSourceAgent(SoundLayer layer)
        {
            if (agents != null)
            {
                foreach (var agentInfo in agents)
                {
                    if (agentInfo != null && agentInfo.layer == layer && agentInfo.agent != null)
                    {
                        return agentInfo.agent;
                    }
                }
            }

            Debug.LogWarning($"SoundManager: no SourceAgent configured for layer {layer}.", this);
            return null;
        }
    }
}
