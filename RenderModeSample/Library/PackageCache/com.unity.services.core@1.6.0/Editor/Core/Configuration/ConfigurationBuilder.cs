using System.Collections.Generic;
using System.Globalization;
using Unity.Services.Core.Configuration.Internal;

namespace Unity.Services.Core.Configuration.Editor
{
    /// <summary>
    /// Container for configuration values that need to be passed to
    /// the <see cref="IProjectConfiguration"/> component at runtime.
    /// </summary>
    public class ConfigurationBuilder
    {
        internal IDictionary<string, ConfigurationEntry> Values { get; }

        internal ConfigurationBuilder()
            : this(new Dictionary<string, ConfigurationEntry>()) {}

        internal ConfigurationBuilder(IDictionary<string, ConfigurationEntry> values)
        {
            Values = values;
        }

        /// <summary>
        /// Stores the given <paramref name="value"/> for the given <paramref name="key"/>.
        /// </summary>
        /// <param name="key">
        /// The identifier of the configuration entry.
        /// </param>
        /// <param name="value">
        /// The value to store.
        /// It is stored as a string using <see cref="CultureInfo.InvariantCulture"/>.
        /// </param>
        /// <param name="isReadOnly">
        /// Set to true to forbid game developers to override this setting.
        /// </param>
        /// <returns>
        /// Return this instance.
        /// </returns>
        public ConfigurationBuilder SetBool(string key, bool value, bool isReadOnly = false)
        {
            if (!string.IsNullOrEmpty(key))
            {
                Values[key] = new ConfigurationEntry(value.ToString(CultureInfo.InvariantCulture), isReadOnly);
            }
            return this;
        }

        /// <summary>
        /// Try to get configuration entry as bool value from an identifier.
        /// </summary>
        /// <param name="key">The identifier of the configuration entry.</param>
        /// <param name="value">The value to retrieve. If fail to retrieve the value is set to default.</param>
        /// <returns>
        /// True if the configuration exist and is boolean.
        /// False otherwise.
        /// </returns>
        public bool TryGetBool(string key, out bool value)
        {
            value = default;
            return Values.TryGetValue(key, out var entry)
                && bool.TryParse(entry.Value, out value);
        }

        /// <summary>
        /// Stores the given <paramref name="value"/> for the given <paramref name="key"/>.
        /// </summary>
        /// <param name="key">
        /// The identifier of the configuration entry.
        /// </param>
        /// <param name="value">
        /// The value to store.
        /// It is stored as a string.
        /// </param>
        /// <param name="isReadOnly">
        /// Set to true to forbid game developers to override this setting.
        /// </param>
        /// <returns>
        /// Return this instance.
        /// </returns>
        public ConfigurationBuilder SetInt(string key, int value, bool isReadOnly = false)
        {
            if (!string.IsNullOrEmpty(key))
            {
                Values[key] = new ConfigurationEntry(value.ToString(), isReadOnly);
            }
            return this;
        }

        /// <summary>
        /// Try to get configuration entry as int value from an identifier.
        /// </summary>
        /// <param name="key">The identifier of the configuration entry.</param>
        /// <param name="value">The value to retrieve. If fail to retrieve the value is set to default.</param>
        /// <returns>
        /// True if the configuration exist and is integer.
        /// False otherwise.
        /// </returns>
        public bool TryGetInt(string key, out int value)
        {
            value = default;
            return Values.TryGetValue(key, out var entry)
                && int.TryParse(entry.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
        }

        /// <summary>
        /// Stores the given <paramref name="value"/> for the given <paramref name="key"/>.
        /// </summary>
        /// <param name="key">
        /// The identifier of the configuration entry.
        /// </param>
        /// <param name="value">
        /// The value to store.
        /// It is stored as a string using <see cref="CultureInfo.InvariantCulture"/>.
        /// </param>
        /// <param name="isReadOnly">
        /// Set to true to forbid game developers to override this setting.
        /// </param>
        /// <returns>
        /// Return this instance.
        /// </returns>
        public ConfigurationBuilder SetFloat(string key, float value, bool isReadOnly = false)
        {
            if (!string.IsNullOrEmpty(key))
            {
                Values[key] = new ConfigurationEntry(value.ToString(CultureInfo.InvariantCulture), isReadOnly);
            }
            return this;
        }

        /// <summary>
        /// Try to get configuration entry as float value from an identifier.
        /// </summary>
        /// <param name="key">The identifier of the configuration entry.</param>
        /// <param name="value">The value to retrieve. If fail to retrieve the value is set to default.</param>
        /// <returns>
        /// True if the configuration exist and is float.
        /// False otherwise.
        /// </returns>
        public bool TryGetFloat(string key, out float value)
        {
            value = default;
            return Values.TryGetValue(key, out var entry)
                && float.TryParse(entry.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
        }

        /// <summary>
        /// Stores the given <paramref name="value"/> for the given <paramref name="key"/>.
        /// </summary>
        /// <param name="key">
        /// The identifier of the configuration entry.
        /// </param>
        /// <param name="value">
        /// The value to store.
        /// </param>
        /// <param name="isReadOnly">
        /// Set to true to forbid game developers to override this setting.
        /// </param>
        /// <returns>
        /// Return this instance.
        /// </returns>
        public ConfigurationBuilder SetString(string key, string value, bool isReadOnly = false)
        {
            if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
            {
                Values[key] = new ConfigurationEntry(value, isReadOnly);
            }
            return this;
        }

        /// <summary>
        /// Try to get configuration entry as string value from an identifier.
        /// </summary>
        /// <param name="key">The identifier of the configuration entry.</param>
        /// <param name="value">The value to retrieve. If fail to retrieve the value is set to default.</param>
        /// <returns>
        /// True if the configuration exist.
        /// False otherwise.
        /// </returns>
        public bool TryGetString(string key, out string value)
        {
            value = default;
            if (!Values.TryGetValue(key, out var entry))
            {
                return false;
            }

            value = entry.Value;
            return true;
        }
    }
}
