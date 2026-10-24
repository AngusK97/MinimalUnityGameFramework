using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace FrameworkConfig
{
    /// <summary>Stores one loaded table. The list and index share the same immutable row instances.</summary>
    public sealed class ConfigTable<TKey, TRow>
    {
        private readonly Dictionary<TKey, TRow> _byId = new Dictionary<TKey, TRow>();
        private readonly ReadOnlyCollection<TRow> _all;

        public IReadOnlyList<TRow> All => _all;

        public ConfigTable(IEnumerable<TRow> rows, Func<TRow, TKey> getId)
        {
            var ordered = new List<TRow>();

            foreach (var row in rows)
            {
                _byId.Add(getId(row), row);
                ordered.Add(row);
            }

            _all = ordered.AsReadOnly();
        }

        public TRow Get(TKey id)
        {
            if (_byId.TryGetValue(id, out var row))
            {
                return row;
            }

            throw new KeyNotFoundException(typeof(TRow).Name + " Id '" + id + "' was not found.");
        }

        public bool TryGet(TKey id, out TRow row)
        {
            return _byId.TryGetValue(id, out row);
        }
    }

    /// <summary>Reads generated local JSON only when a game's explicit configuration loader calls it.</summary>
    public static class ConfigData
    {
        public static T LoadJson<T>(string resourcePath) where T : class
        {
            var asset = Resources.Load<TextAsset>(resourcePath);

            if (asset == null)
            {
                throw new InvalidOperationException("Config data is missing at Resources/" + resourcePath + ".json. Update config before starting the game.");
            }

            try
            {
                var data = JsonUtility.FromJson<T>(asset.text);

                if (data == null)
                {
                    throw new InvalidOperationException("Config JSON is invalid: " + resourcePath + ". Update config again.");
                }

                return data;
            }
            finally
            {
                Resources.UnloadAsset(asset);
            }
        }
    }
}
