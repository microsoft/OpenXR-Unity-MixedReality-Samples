using System;
using UnityEngine;

namespace UnityEditor.SettingsManagement.Examples
{
	[Serializable]
	class FooClass
	{
		public int intValue;
		public string stringValue;

		public FooClass()
		{
			intValue = 42;
			stringValue = "I'm some text";
		}
	}

	class MySettingsExamples : EditorWindow
	{
#pragma warning disable 414
		// [UserSetting] attribute registers this setting with the UserSettingsProvider so that it can be automatically
		// shown in the UI.
		[UserSetting("General Settings", "Days Without Incident")]
		static MySetting<int> s_NumberOfDaysWithoutIncident = new MySetting<int>("general.daysWithoutIncident", 0, SettingsScope.User);

		[UserSetting("General Settings", "Favorite Color")]
		static MySetting<Color> s_FavoriteColor = new MySetting<Color>("general.favoriteColor", Color.magenta);

		[UserSetting("General Settings", "Vector2 Field")]
		static MySetting<Vector2> s_Vector2Value = new MySetting<Vector2>("general.vector2Value", new Vector2(2f, 4f));

		[UserSetting("General Settings", "Editor Flags")]
		static MySetting<StaticEditorFlags> s_EditorFlags = new MySetting<StaticEditorFlags>("general.editorFlags", StaticEditorFlags.BatchingStatic);
#pragma warning restore 414

		// [UserSetting] with no arguments simply registers the key with UserSettingsProvider so that it can be included
		// in debug views and reset with the options gizmo. Usually this is used in conjunction with [UserSettingsBlock].
		[UserSetting]
		static MySetting<FooClass> s_Foo = new MySetting<FooClass>("general.foo", new FooClass(), SettingsScope.Project);

		[UserSetting]
		static MySetting<int> s_NumberWithSlider = new MySetting<int>("general.conditionalValue", 5, SettingsScope.Project);

		// A UserSettingBlock is a callback invoked from the UserSettingsProvider. It allows you to draw more complicated
		// UI elements without the need to create a new SettingsProvider. Parameters are "category" and "search keywords."
		// For maximum compatibility, use `SettingsGUILayout` searchable and settings fields to get features like search
		// and per-setting reset with a context click.
		[UserSettingBlock("Custom GUI Settings")]
		static void ConditionalValueGUI(string searchContext)
		{
			EditorGUI.BeginChangeCheck();

			s_NumberWithSlider.value = SettingsGUILayout.SettingsSlider("Number With Slider", s_NumberWithSlider, 0, 10, searchContext);

			var foo = s_Foo.value;

			using(new SettingsGUILayout.IndentedGroup("Foo Class"))
			{
				EditorGUI.BeginChangeCheck();

				foo.intValue = SettingsGUILayout.SearchableIntField("Int Value", foo.intValue, searchContext);
				foo.stringValue = SettingsGUILayout.SearchableTextField("String Value", foo.stringValue, searchContext);

				// Because FooClass is a reference type, we need to apply the changes to the backing repository (SetValue
				// would also work here).
				if (EditorGUI.EndChangeCheck())
					s_Foo.ApplyModifiedProperties();
			}

			SettingsGUILayout.DoResetContextMenuForLastRect(s_Foo);

			if (EditorGUI.EndChangeCheck())
				MySettingsManager.Save();
		}

		const string k_ColorInstanceFieldKey = "MySettingsExamples.m_ColorField";

		// It is also possible to forego the UserSetting<T> wrapper and use a settings instance directly. To register
		// a setting with the "Reset All" option of UserSettingsProvider, apply the [UserSetting] (or for instance fields [SettingsKey]) attribute.
		Color m_ColorField;

		[MenuItem("Window/Show Settings Examples")]
		static void Init()
		{
			GetWindow<MySettingsExamples>();
		}

		void OnEnable()
		{
			m_ColorField = MySettingsManager.Get<Color>(k_ColorInstanceFieldKey);
		}

		void OnGUI()
		{
			EditorGUI.BeginChangeCheck();

			m_ColorField = EditorGUILayout.ColorField("Color", m_ColorField);

			if (EditorGUI.EndChangeCheck())
				MySettingsManager.Set<Color>(k_ColorInstanceFieldKey, m_ColorField);
		}
	}
}
