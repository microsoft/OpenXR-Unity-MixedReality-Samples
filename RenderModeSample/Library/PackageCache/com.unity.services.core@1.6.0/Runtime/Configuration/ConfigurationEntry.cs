using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Unity.Services.Core.Configuration
{
    /// <summary>
    /// Wrapper for project configuration values.
    /// </summary>
    [Serializable]
    class ConfigurationEntry
    {
        [JsonRequired]
        [SerializeField]
        string m_Value;

        /// <summary>
        /// Get the stored configuration value.
        /// </summary>
        [JsonIgnore]
        public string Value => m_Value;

        [JsonRequired]
        [SerializeField]
        bool m_IsReadOnly;

        /// <summary>
        /// If true, <see cref="Value"/> can't be changed.
        /// </summary>
        [JsonIgnore]
        public bool IsReadOnly
        {
            get => m_IsReadOnly;
            internal set => m_IsReadOnly = value;
        }

        /// <summary>
        /// Create a new instance of the <see cref="ConfigurationEntry"/> class.
        /// </summary>
        /// <remarks>
        /// Required for serialization.
        /// </remarks>
        public ConfigurationEntry() {}

        /// <summary>
        /// Create a new instance of the <see cref="ConfigurationEntry"/> class.
        /// </summary>
        /// <param name="value">
        /// The value to store.
        /// </param>
        /// <param name="isReadOnly">
        /// If true, the value can't be changed after construction.
        /// </param>
        public ConfigurationEntry(string value, bool isReadOnly = false)
        {
            m_Value = value;
            m_IsReadOnly = isReadOnly;
        }

        /// <summary>
        /// Set <see cref="Value"/> only if <see cref="IsReadOnly"/> is false.
        /// Does nothing otherwise.
        /// </summary>
        /// <param name="value">
        /// The new value to store.
        /// </param>
        /// <returns>
        /// Return true if <see cref="IsReadOnly"/> is false;
        /// return false otherwise.
        /// </returns>
        public bool TrySetValue(string value)
        {
            if (IsReadOnly)
            {
                return false;
            }

            m_Value = value;
            return true;
        }

        public static implicit operator string(ConfigurationEntry entry) => entry.Value;

        public static implicit operator ConfigurationEntry(string value) => new ConfigurationEntry(value);
    }
}
