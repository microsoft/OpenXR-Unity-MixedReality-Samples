using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.XR.ARSubsystems
{
    [Serializable]
    class SerializableDictionary<TKey, TValue>
    {
        [Serializable]
        struct KeyValuePair
        {
            public TKey key;
            public TValue value;
        }

        public Dictionary<TKey, TValue> dictionary { get; } = new Dictionary<TKey, TValue>();

        public void Serialize()
        {
            m_Storage = new KeyValuePair[dictionary.Count];
            var index = 0;
            foreach (var pair in dictionary)
            {
                m_Storage[index++] = new KeyValuePair
                {
                    key = pair.Key,
                    value = pair.Value
                };
            }
        }

        public void Deserialize()
        {
            dictionary.Clear();
            if (m_Storage == null)
                return;

            foreach (var pair in m_Storage)
            {
                dictionary.Add(pair.key, pair.value);
            }
        }

        [SerializeField]
        KeyValuePair[] m_Storage;
    }
}
