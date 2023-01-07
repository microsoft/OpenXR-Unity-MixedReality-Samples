using System;
using UnityEngine;

namespace UnityEditor.SettingsManagement.Examples
{
	/// <summary>
	/// To create an entry in the Preferences window, define a new SettingsProvider inheriting <see cref="UserSettingsProvider"/>.
	/// You can also choose to implement your own SettingsProvider and ignore this implementation. The benefit of using
	/// <see cref="UserSettingsProvider"/> is that all <see cref="UserSetting{T}"/> fields in the assembly are automatically
	/// populated within the preferences, with support for search and resetting default values.
	/// </summary>
	static class MySettingsProvider
	{
		const string k_PreferencesPath = "Preferences/Package With Project and User Settings";

#if UNITY_2018_3_OR_NEWER
		[SettingsProvider]
		static SettingsProvider CreateSettingsProvider()
		{
			var provider = new UserSettingsProvider(k_PreferencesPath,
				MySettingsManager.instance,
				new [] { typeof(MySettingsProvider).Assembly });

			return provider;
		}
#else

	// For backwards compatibility it is possible to create an instance of UserSettingsProvider and invoke OnGUI manually.
	[NonSerialized]
	static UserSettingsProvider s_SettingsProvider;

	[PreferenceItem("ProBuilder")]
	static void ProBuilderPreferencesGUI()
	{
		if (s_SettingsProvider == null)
			s_SettingsProvider = new UserSettingsProvider(MySettingsManager.instance, new[] { typeof(MySettingsProvider).Assembly });

		s_SettingsProvider.OnGUI(null);
	}

#endif
	}
}
