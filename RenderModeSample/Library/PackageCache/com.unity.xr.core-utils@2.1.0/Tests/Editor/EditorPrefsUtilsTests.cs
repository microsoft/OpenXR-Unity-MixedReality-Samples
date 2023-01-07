using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.XR.CoreUtils.Editor;
using UnityEditor;
using UnityEngine;

namespace Unity.XR.CoreUtils.EditorTests
{
    class EditorPrefsUtilsTests
    {
        class DummyClass
        {
            public static bool boolTest
            {
                get => EditorPrefsUtils.GetBool(k_TypeName, k_BoolTestValue);
                set => EditorPrefsUtils.SetBool(k_TypeName, value);
            }

            public static float floatTest
            {
                get => EditorPrefsUtils.GetFloat(k_TypeName, k_FloatTestValue);
                set => EditorPrefsUtils.SetFloat(k_TypeName, value);
            }

            public static int intTest
            {
                get => EditorPrefsUtils.GetInt(k_TypeName, k_IntTestValue);
                set => EditorPrefsUtils.SetInt(k_TypeName, value);
            }

            public static string stringTest
            {
                get => EditorPrefsUtils.GetString(k_TypeName, k_StringTestValue);
                set => EditorPrefsUtils.SetString(k_TypeName, value);
            }

            public static DummyEnum enumTest
            {
                get => (DummyEnum)EditorPrefsUtils.GetInt(k_TypeName, (int)k_EnumTestValue);
                set => EditorPrefsUtils.SetInt(k_TypeName, (int)value);
            }

            public static Color colorTest
            {
                get => EditorPrefsUtils.GetColor(k_TypeName, Color.cyan);
                set => EditorPrefsUtils.SetColor(k_TypeName, value);
            }

            public static Color color1 = new Color(1f, 0f, 0f, 0.3f);
            public static Color color2 = Color.blue;
            public static Color color3 = new Color(0.123f, 0.55f, 1.00f);
            public static Color color4 = new Color(0f, .5f, .05f, 1f);
        }

        static readonly string k_TypeName = typeof(DummyClass).FullName;

        static class TestCaseStructs
        {
            public static object[] structTests =
            {
                new object[]{ nameof(DummyClass.colorTest), DummyClass.colorTest},
                new object[]{ nameof(DummyClass.color1), DummyClass.color1 },
                new object[]{ nameof(DummyClass.color2), DummyClass.color2 },
                new object[]{ nameof(DummyClass.color3), DummyClass.color3 },
                new object[]{ nameof(DummyClass.color4), DummyClass.color4 },
            };

            public static object[] defaultStructTests =
            {
                new object[] { nameof(DummyClass.colorTest), typeof(Color), DummyClass.colorTest }
            };
        }

        enum DummyEnum
        {
            NegOne = -1,
            Zero = 0,
            One,
            Two,
            Three,
            Four
        }

        const bool k_BoolTestValue = true;
        const float k_FloatTestValue = 9876.54321f;
        const int k_IntTestValue = 56;
        const string k_StringTestValue = "Made in Unity";
        const DummyEnum k_EnumTestValue = DummyEnum.Four;

        List<string> m_PropertyNames;

        [OneTimeSetUp]
        public void Setup()
        {
            m_PropertyNames = typeof(DummyClass).GetProperties(BindingFlags.Public | BindingFlags.Static
                | BindingFlags.Instance | BindingFlags.NonPublic).Select(n => n.Name).ToList();
        }

        [SetUp]
        public void BeforeEach()
        {
            EditorPrefsUtils.ResetEditorPrefsValueSessionCache();
            foreach (var propertyName in m_PropertyNames)
            {
                var prefKey = EditorPrefsUtils.GetPrefKey(k_TypeName, propertyName);
                if (EditorPrefs.HasKey(prefKey))
                    EditorPrefs.DeleteKey(prefKey);
            }
        }

        [TestCase(nameof(DummyClass.boolTest))]
        [TestCase(nameof(DummyClass.floatTest))]
        [TestCase(nameof(DummyClass.intTest))]
        [TestCase(nameof(DummyClass.stringTest))]
        public void GetPrefsKey_ParentType(string propertyName)
        {
            var prefsKey = EditorPrefsUtils.GetPrefKey(k_TypeName, propertyName);
            Assert.False(string.IsNullOrEmpty(prefsKey));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void GetSetBool(bool value)
        {
            DummyClass.boolTest = value;
            Assert.True(DummyClass.boolTest == value);
        }

        [TestCase(nameof(DummyClass.boolTest), typeof(bool), k_BoolTestValue)]
        [TestCase(nameof(DummyClass.floatTest), typeof(float), k_FloatTestValue)]
        [TestCase(nameof(DummyClass.intTest), typeof(int), k_IntTestValue)]
        [TestCase(nameof(DummyClass.stringTest), typeof(string), k_StringTestValue)]
        [TestCase(nameof(DummyClass.enumTest), typeof(Enum), k_EnumTestValue)]
        public void SetEditorPrefsDefault_Value(string propertyName, Type valueType, object targetValue)
        {
            var propertyInfo = typeof(DummyClass).GetProperty(propertyName, BindingFlags.Static
                | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.NotNull(propertyInfo);
            var propValue = propertyInfo.GetValue(typeof(DummyClass), null);
            ValueChecker(valueType, propValue, targetValue);
        }

        [TestCase(nameof(DummyClass.boolTest), typeof(bool), k_BoolTestValue)]
        [TestCase(nameof(DummyClass.floatTest), typeof(float), k_FloatTestValue)]
        [TestCase(nameof(DummyClass.intTest), typeof(int), k_IntTestValue)]
        [TestCase(nameof(DummyClass.stringTest), typeof(string), k_StringTestValue)]
        [TestCase(nameof(DummyClass.enumTest), typeof(Enum), k_EnumTestValue)]
        public void EditorPrefsGetSet(string propertyName, Type valueType, object targetValue)
        {
            var propertyInfo = typeof(DummyClass).GetProperty(propertyName, BindingFlags.Static
                | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.NotNull(propertyInfo);
            propertyInfo.SetValue(typeof(DummyClass), targetValue);
            var propValue = propertyInfo.GetValue(typeof(DummyClass));
            ValueChecker(valueType, propValue, targetValue);
        }

        [TestCaseSource(typeof(TestCaseStructs), nameof(TestCaseStructs.structTests))]
        public static void EditorPrefsGetSetColor(string propertyName, Color targetValue)
        {
            EditorPrefsUtils.SetColor(k_TypeName, targetValue, propertyName);
            var propValue = EditorPrefsUtils.GetColor(k_TypeName, Color.black, propertyName);
            ValueChecker(typeof(Color), propValue, targetValue);
        }

        [TestCaseSource(typeof(TestCaseStructs), nameof(TestCaseStructs.defaultStructTests))]
        public void SetEditorPrefsDefaultColor(string propertyName, Type valueType, object targetValue)
        {
            var propertyInfo = typeof(DummyClass).GetProperty(propertyName, BindingFlags.Static
                | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.NotNull(propertyInfo);
            var propValue = propertyInfo.GetValue(typeof(DummyClass), null);
            ValueChecker(valueType, propValue, targetValue);
        }

        static void ValueChecker(Type type, object valueA, object valueB)
        {
            if (type == typeof(bool) && valueA is bool && valueB is bool)
            {
                Assert.True((bool)valueA == (bool)valueB);
            }
            else if (type == typeof(float) && valueA is float && valueB is float)
            {
                Assert.True(Mathf.Approximately((float)valueA, (float)valueB));
            }
            else if (type == typeof(int) && valueA is int && valueB is int)
            {
                Assert.True((int)valueA == (int)valueB);
            }
            else if (type == typeof(string) && valueA is string && valueB is string)
            {
                Assert.True((string)valueA == (string)valueB);
            }
            else if (type == typeof(Color) && valueA is Color && valueB is Color)
            {
                Assert.True((Color)valueA == (Color)valueB);
            }
            else if (type.IsAssignableFromOrSubclassOf(typeof(Enum))
                && valueA.GetType().IsAssignableFromOrSubclassOf(type)
                && valueB.GetType().IsAssignableFromOrSubclassOf(type))
            {
                Assert.True((int)valueA == (int)valueB);
            }
            else
            {
                Assert.Fail();
            }
        }
    }
}
