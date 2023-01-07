# Settings Manager

A framework for making any serializable field a setting, complete with a pre-built settings interface.

## Quick Start

Settings are stored and managed by a `Settings` instance. This class is responsible for setting and retrieving serialized values from the appropriate repository.

Settings repositories are used to save and load settingsfor a settings scope. Two are provided with this package: one for saving User preferences (`UserSettingsRepository`, backed by the `EditorPrefs` class) and one for Project settings (`ProjecSettingsRepository`, which saves a JSON file to the `ProjectSettings` directory).

Usually you will want to create and manage a singleton `Settings` instance. Ex:

```
using UnityEditor.SettingsManagement;

namespace UnityEditor.SettingsManagement.Examples
{
    static class MySettingsManager
    {
        internal const string k_PackageName = "com.unity.my-settings-example";

        static Settings s_Instance;

        internal static Settings instance
        {
            get
            {
                if (s_Instance == null)
                    s_Instance = new Settings(k_PackageName);

                return s_Instance;
            }
        }
    }
}
```

Values are set and retrieved using generic methods on on your `Settings` instance:

```
MySettingsManager.instance.Get<float>("myFloatValue", SettingsScope.Project);
```

There are two arguments: key, and scope. The `Settings` class will handle finding an appropriate `ISettingsRepository` for the scope, while `key` and `T` are used to find the value. Setting keys are unique among types, meaning you may re-use keys as long as the setting type is different.

Alternatively, you can use the `UserSetting<T>` class to manage settings. This is a wrapper class around the `Settings` get/set properties, and makes it very easy to make any field a saved setting.

```
// UserSetting<T>(Settings instance, string key, T defaultValue, SettingsScope scope = SettingsScope.Project)
Setting<int> myIntValue = new Setting<int>(MySettingsManager.instance, "int.key", 42, SettingsScope.User);
```

`UserSetting<T>` caches the current value, and keeps a copy of the default value so that it may be reset. `UserSetting<T>` fields are also eligible for use with the `[UserSettingAttribute]` attribute, which lets the `SettingsManagerProvider` automatically add it to a settings inspector.

## Settings Provider

To register your settings in the `Settings Window` you can either write your own `SettingsProvider` implementation, or use the provided `SettingsManagerProvider` and let it automatically create your interface.

Making use of `SettingsManagerProvider` comes with many benefits, including a uniform look for your settings UI, support for search, and per-field or mass reset support.

```
using UnityEngine;

namespace UnityEditor.SettingsManagement.Examples
{
	static class MySettingsProvider
	{
		const string k_PreferencesPath = "Preferences/My Settings";

		[SettingsProvider]
		static SettingsProvider CreateSettingsProvider()
		{
			// The last parameter tells the provider where to search for settings.
			var provider = new SettingsManagerProvider(k_PreferencesPath,
				MySettingsManager.instance,
				new [] { typeof(MySettingsProvider).Assembly });

			return provider;
		}
	}
}
```

To register a field with the `SettingsManagerProvider`, simply decorate it with `[UserSettingAttribute(string displayCategory, string key)]`. `[UserSettingAttribute]` is only valid for static fields.

For more complex settings that require additional UI (or simply don't have a built-in editor), you can use `UserSettingBlockAttribute`. This provides access to the settings provider GUI. See `SettingsExamples.cs` for more on this.

