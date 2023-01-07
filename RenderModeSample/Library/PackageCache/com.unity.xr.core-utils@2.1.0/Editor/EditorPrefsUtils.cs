using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace Unity.XR.CoreUtils.Editor
{
    /// <summary>
    /// Utilities for getting and setting editor preferences that caches the values of those preferences.
    /// </summary>
    public static class EditorPrefsUtils
    {
        static readonly Dictionary<string, object> k_EditorPrefsValueSessionCache = new Dictionary<string, object>();

        /// <summary>
        /// Gets the Editor Preference Key by combining the parent object type's full name and the property name
        /// </summary>
        /// <param name="typeName">The name of the type which defines the property</param>
        /// <param name="propertyName">Name of calling property</param>
        /// <returns>Editor Preference Key for property</returns>
        public static string GetPrefKey(string typeName, string propertyName)
        {
            return $"{typeName}.{propertyName}";
        }

        /// <summary>
        /// Get the bool value stored in the Editor Preferences for the calling property.
        /// </summary>
        /// <param name="typeName">The name of the type which defines the property</param>
        /// <param name="defaultValue">Value to be used as default.</param>
        /// <param name="propertyName">Name of calling Property</param>
        /// <returns>The bool value stored in the Editor Preferences for the calling property.</returns>
        public static bool GetBool(string typeName, bool defaultValue = false,
            [CallerMemberName] string propertyName = null)
        {
            var prefsKey = GetPrefKey(typeName, propertyName);
            return GetEditorPrefsValueOrDefault(prefsKey, defaultValue);
        }

        /// <summary>
        /// Sets the bool value to the Editor Preferences stored value for the calling property.
        /// </summary>
        /// <param name="typeName">The name of the type which defines the property</param>
        /// <param name="value">Value to set in Editor Preferences</param>
        /// <param name="propertyName">Name of calling Property</param>
        public static void SetBool(string typeName, bool value,
            [CallerMemberName] string propertyName = null)
        {
            var prefsKey = GetPrefKey(typeName, propertyName);
            SetEditorPrefsValue(prefsKey, value);
        }

        /// <summary>
        /// Get the float value stored in the Editor Preferences for the calling property.
        /// </summary>
        /// <param name="typeName">The name of the type which defines the property</param>
        /// <param name="defaultValue">Value to be used as default.</param>
        /// <param name="propertyName">Name of calling Property</param>
        /// <returns>The float value stored in the Editor Preferences for the calling property.</returns>
        public static float GetFloat(string typeName, float defaultValue = 0f,
            [CallerMemberName] string propertyName = null)
        {
            var prefsKey = GetPrefKey(typeName, propertyName);
            return GetEditorPrefsValueOrDefault(prefsKey, defaultValue);
        }

        /// <summary>
        /// Sets the float value to the Editor Preferences stored value for the calling property.
        /// </summary>
        /// <param name="typeName">The name of the type which defines the property</param>
        /// <param name="value">Value to set in Editor Preferences</param>
        /// <param name="propertyName">Name of calling Property</param>
        public static void SetFloat(string typeName, float value,
            [CallerMemberName] string propertyName = null)
        {
            var prefsKey = GetPrefKey(typeName, propertyName);
            SetEditorPrefsValue(prefsKey, value);
        }

        /// <summary>
        /// Get the int value stored in the Editor Preferences for the calling property.
        /// </summary>
        /// <param name="typeName">The name of the type which defines the property</param>
        /// <param name="defaultValue">Value to be used as default.</param>
        /// <param name="propertyName">Name of calling Property</param>
        /// <returns>The int value stored in the Editor Preferences for the calling property.</returns>
        public static int GetInt(string typeName, int defaultValue = 0,
            [CallerMemberName] string propertyName = null)
        {
            var prefsKey = GetPrefKey(typeName, propertyName);
            return GetEditorPrefsValueOrDefault(prefsKey, defaultValue);
        }

        /// <summary>
        /// Sets the int value to the Editor Preferences stored value for the calling property.
        /// </summary>
        /// <param name="typeName">The name of the type which defines the property</param>
        /// <param name="value">Value to set in Editor Preferences</param>
        /// <param name="propertyName">Name of calling Property</param>
        public static void SetInt(string typeName, int value,
            [CallerMemberName] string propertyName = null)
        {
            var prefsKey = GetPrefKey(typeName, propertyName);
            SetEditorPrefsValue(prefsKey, value);
        }

        /// <summary>
        /// Get the string value stored in the Editor Preferences for the calling property.
        /// </summary>
        /// <param name="typeName">The name of the type which defines the property</param>
        /// <param name="defaultValue">Value to be used as default.</param>
        /// <param name="propertyName">Name of calling Property</param>
        /// <returns>The string value stored in the Editor Preferences for the calling property.</returns>
        public static string GetString(string typeName, string defaultValue = "",
            [CallerMemberName] string propertyName = null)
        {
            var prefsKey = GetPrefKey(typeName, propertyName);
            return GetEditorPrefsValueOrDefault(prefsKey, defaultValue);
        }

        /// <summary>
        /// Sets the string value to the Editor Preferences stored value for the calling property.
        /// </summary>
        /// <param name="typeName">The name of the type which defines the property</param>
        /// <param name="value">Value to set in Editor Preferences</param>
        /// <param name="propertyName">Name of calling Property</param>
        public static void SetString(string typeName, string value,
            [CallerMemberName] string propertyName = null)
        {
            var prefsKey = GetPrefKey(typeName, propertyName);
            SetEditorPrefsValue(prefsKey, value);
        }

        /// <summary>
        /// Get the color value stored in the Editor Preferences for the calling property.
        /// </summary>
        /// <param name="typeName">The name of the type which defines the property</param>
        /// <param name="defaultValue">Value to be used as default.</param>
        /// <param name="propertyName">Name of calling Property</param>
        /// <returns>The color value stored in the Editor Preferences for the calling property.</returns>
        public static Color GetColor(string typeName, Color defaultValue,
            [CallerMemberName] string propertyName = null)
        {
            var prefsKey = GetPrefKey(typeName, propertyName);
            return GetEditorPrefsValueOrDefault(prefsKey, defaultValue);
        }

        /// <summary>
        /// Sets the color value to the Editor Preferences stored value for the calling property.
        /// </summary>
        /// <param name="typeName">The name of the type which defines the property</param>
        /// <param name="value">Value to set in Editor Preferences</param>
        /// <param name="propertyName">Name of calling Property</param>
        public static void SetColor(string typeName, Color value,
            [CallerMemberName] string propertyName = null)
        {
            var prefsKey = GetPrefKey(typeName, propertyName);
            SetEditorPrefsValue(prefsKey, value);
        }

        /// <summary>
        /// Resets the cached Editor Prefs Values stored in the Editor Prefs Utils
        /// </summary>
        internal static void ResetEditorPrefsValueSessionCache()
        {
            k_EditorPrefsValueSessionCache.Clear();
        }

        static void SetEditorPrefsValue<T>(string prefsKey, T value)
        {
            if (TryGetCachedEditorPrefsValue(prefsKey, out T cachedValue) && cachedValue.Equals(value))
                return;

            var type = typeof(T);

            if (type == typeof(bool))
            {
                EditorPrefs.SetBool(prefsKey, (bool)(object)value);
            }
            else if (type == typeof(int) && value is int)
            {
                EditorPrefs.SetInt(prefsKey, (int)(object)value);
            }
            else if (type == typeof(float) && value is float)
            {
                EditorPrefs.SetFloat(prefsKey, (float)(object)value);
            }
            else if (type == typeof(string) && value is string)
            {
                EditorPrefs.SetString(prefsKey, (string)(object)value);
            }
            else if (type.IsAssignableFromOrSubclassOf(typeof(Enum))
                && value.GetType().IsAssignableFromOrSubclassOf(typeof(Enum)))
            {
                EditorPrefs.SetInt(prefsKey, (int)(object)value);
            }
            else if (type == typeof(Color) && value is Color)
            {
                EditorPrefs.SetString(prefsKey, ColorToColorPref(prefsKey, (Color)(object)value));
            }
            else
            {
                Debug.LogError($"Could not set Editor Preference Value of type : {type} with value {value} !");
                return;
            }

            if (k_EditorPrefsValueSessionCache.ContainsKey(prefsKey))
                k_EditorPrefsValueSessionCache[prefsKey] = value;
            else
                k_EditorPrefsValueSessionCache.Add(prefsKey, value);
        }

        static void GetEditorPrefsValue<T>(string prefsKey, out T prefValue)
        {
            if (TryGetCachedEditorPrefsValue(prefsKey, out prefValue))
                return;

            var type = typeof(T);
            var prefsSet = false;
            if (type == typeof(bool))
            {
                prefValue = (T)(object)EditorPrefs.GetBool(prefsKey);
                prefsSet = true;
            }
            else if (type == typeof(int))
            {
                prefValue = (T)(object)EditorPrefs.GetInt(prefsKey);
                prefsSet = true;
            }
            else if (type == typeof(float))
            {
                prefValue = (T)(object)EditorPrefs.GetFloat(prefsKey);
                prefsSet = true;
            }
            else if (type == typeof(string))
            {
                prefValue = (T)(object)EditorPrefs.GetString(prefsKey);
                prefsSet = true;
            }
            else if (type.IsAssignableFromOrSubclassOf(typeof(Enum)))
            {
                prefValue = (T)(object)EditorPrefs.GetInt(prefsKey);
                prefsSet = true;
            }
            else if (type == typeof(Color))
            {
                prefValue = (T)(object)PrefToColor(EditorPrefs.GetString(prefsKey));
                prefsSet = true;
            }
            else
            {
                Debug.LogError($"Could not get Editor Preference Default of type : {type} Type is not supported!");
            }

            if (prefsSet && prefValue != null)
            {
                SetEditorPrefsValue(prefsKey, prefValue);
                return;
            }

            SetEditorPrefsValue(prefsKey, default(T));
            prefValue = default;
        }

        static bool TryGetCachedEditorPrefsValue<T>(string prefsKey, out T prefValue)
        {
            if (k_EditorPrefsValueSessionCache.TryGetValue(prefsKey, out var cachedObj))
            {
                if (cachedObj is T || cachedObj.GetType().IsAssignableFromOrSubclassOf(typeof(T)))
                {
                    prefValue = (T)cachedObj;
                    return true;
                }
            }

            prefValue = default;
            return false;
        }

        static T GetEditorPrefsValueOrDefault<T>(string prefsKey, T defaultValue = default)
        {
            var value = defaultValue;
            if (!EditorPrefs.HasKey(prefsKey))
                SetEditorPrefsValue(prefsKey, value);
            else
                GetEditorPrefsValue(prefsKey, out value);

            return value;
        }

        /// <summary>
        /// Used to get editor preference colors setting
        /// </summary>
        /// <param name="pref">Name of color preference inf the from of `EditorPrefs.GetString("HEADER/PARAMETER")`</param>
        /// <returns>Color form Unity Editor Preferences</returns>
        public static Color PrefToColor(string pref)
        {
            var split = pref.Split(';');
            if (split.Length != 5)
            {
                Debug.LogWarningFormat("Parsing PrefColor failed on {0}", pref);
                return default;
            }

            split[1] = split[1].Replace(',', '.');
            split[2] = split[2].Replace(',', '.');
            split[3] = split[3].Replace(',', '.');
            split[4] = split[4].Replace(',', '.');
            var success = float.TryParse(split[1], NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat, out var r);
            success &= float.TryParse(split[2], NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat, out var g);
            success &= float.TryParse(split[3], NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat, out var b);
            success &= float.TryParse(split[4], NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat, out var a);

            if (success)
                return new Color(r, g, b, a);

            Debug.LogWarningFormat("Parsing PrefColor failed on {0}", pref);
            return default;
        }

        /// <summary>
        /// Return a string which matches the format for a color in Editor Prefs
        /// </summary>
        /// <param name="path">The preference key/path</param>
        /// <param name="value">The color value</param>
        /// <returns>Formatted string representing the color value for Editor Prefs</returns>
        public static string ColorToColorPref(string path, Color value)
        {
            var colorString = $"{value.r:0.000};{value.g:0.000};{value.b:0.000};{value.a:0.000}".Replace('.', ',');
            return $"{path};{colorString}";
        }
    }
}
