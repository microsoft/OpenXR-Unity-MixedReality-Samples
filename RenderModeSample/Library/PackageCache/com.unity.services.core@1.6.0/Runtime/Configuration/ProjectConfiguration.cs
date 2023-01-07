using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using Unity.Services.Core.Configuration.Internal;

namespace Unity.Services.Core.Configuration
{
    class ProjectConfiguration : IProjectConfiguration
    {
        string m_JsonCache;
        readonly IReadOnlyDictionary<string, ConfigurationEntry> m_ConfigValues;

        public ProjectConfiguration(IReadOnlyDictionary<string, ConfigurationEntry> configValues)
        {
            m_ConfigValues = configValues;
        }

        public bool GetBool(string key, bool defaultValue = default)
        {
            var stringConfig = GetString(key);
            if (bool.TryParse(stringConfig, out var parsedValue))
            {
                return parsedValue;
            }

            return defaultValue;
        }

        public int GetInt(string key, int defaultValue = default)
        {
            var stringConfig = GetString(key);
            if (int.TryParse(stringConfig, out var parsedValue))
            {
                return parsedValue;
            }

            return defaultValue;
        }

        public float GetFloat(string key, float defaultValue = default)
        {
            var stringConfig = GetString(key);
            if (float.TryParse(stringConfig, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedValue))
            {
                return parsedValue;
            }

            return defaultValue;
        }

        public string GetString(string key, string defaultValue = default)
        {
            if (m_ConfigValues.TryGetValue(key, out var configValue))
            {
                return configValue.Value;
            }

            return defaultValue;
        }

        public string ToJson()
        {
            if (m_JsonCache == null)
            {
                var dict = m_ConfigValues.ToDictionary(pair => pair.Key, pair => pair.Value.Value);
                m_JsonCache = JsonConvert.SerializeObject(dict);
            }
            return m_JsonCache;
        }
    }
}
