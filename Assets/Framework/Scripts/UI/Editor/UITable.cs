using System;
using System.Collections.Generic;
using UnityEngine;

namespace UI.Editor
{
    [CreateAssetMenu(fileName = "UI", menuName = "Framework/UI Table")]
    public sealed class UITable : ScriptableObject
    {
        public string Namespace;
        public string ClassName = "GameUI";
        public string IdPrefix = "MyGame";
        public List<Entry> Entries = new List<Entry>();

        [Serializable]
        public sealed class Entry
        {
            public string Name;
            public UIBase Prefab;
        }
    }
}
