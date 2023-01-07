using System;
using System.Collections.Generic;
using System.Globalization;
using Unity.Services.Core.Internal;
using UnityEngine;

namespace Unity.Services.Core.Configuration
{
    static class ConfigurationCollectionHelper
    {
        public static void FillWith(
            this IDictionary<string, ConfigurationEntry> self, SerializableProjectConfiguration config)
        {
            for (var i = 0; i < config.Keys.Length; i++)
            {
                var entryKey = config.Keys[i];
                var entryValue = config.Values[i];

                self.SetOrCreateEntry(entryKey, entryValue);
            }
        }

        public static void FillWith(
            this IDictionary<string, ConfigurationEntry> self, InitializationOptions options)
        {
            foreach (var option in options.Values)
            {
                var optionValue = Convert.ToString(option.Value, CultureInfo.InvariantCulture);
                self.SetOrCreateEntry(option.Key, optionValue);
            }
        }

        static void SetOrCreateEntry(
            this IDictionary<string, ConfigurationEntry> self, string key, ConfigurationEntry entry)
        {
            if (self.TryGetValue(key, out var existingEntry))
            {
                if (!existingEntry.TrySetValue(entry))
                {
                    CoreLogger.LogWarning(
                        $"You are attempting to initialize Operate Solution SDK with an option \"{key}\"" +
                        " which is readonly at runtime and can be modified only through Project Settings." +
                        " The value provided as initialization option will be ignored." +
                        $" Please update {nameof(InitializationOptions)} in order to remove this warning.");
                }
            }
            else
            {
                self[key] = entry;
            }
        }
    }
}
