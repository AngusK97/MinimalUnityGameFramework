using System;
using System.Collections.Generic;
using UnityEngine;

namespace Resource.Editor
{
    [CreateAssetMenu(fileName = "Main", menuName = "Framework/Resource Module Table")]
    public sealed class ResourceModuleTable : ScriptableObject
    {
        [Header("Set once with your programmer")]
        public string Namespace;
        public string ClassName = "MainResources";
        public string ModuleId = "MyGame.Main";

        [Header("Name, drag an asset, then generate")]
        public List<AudioClipEntry> AudioClips = new List<AudioClipEntry>();
        public List<PrefabEntry> Prefabs = new List<PrefabEntry>();
        public List<SpriteEntry> Sprites = new List<SpriteEntry>();

        [Serializable]
        public sealed class AudioClipEntry
        {
            public string Name;
            public AudioClip Asset;
        }

        [Serializable]
        public sealed class PrefabEntry
        {
            public string Name;
            public GameObject Asset;
        }

        [Serializable]
        public sealed class SpriteEntry
        {
            public string Name;
            public Sprite Asset;
        }
    }
}
