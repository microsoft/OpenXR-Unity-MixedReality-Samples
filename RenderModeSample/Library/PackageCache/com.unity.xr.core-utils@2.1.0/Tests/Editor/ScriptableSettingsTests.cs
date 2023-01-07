using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace Unity.XR.CoreUtils.EditorTests
{
    class ScriptableSettingsTests
    {
        // These fields are accessed via reflection using TestCaseSource
#pragma warning disable 414
        static readonly IEnumerable k_ValidatePathValidData = new[]
        {
            new TestCaseData("Some/Path", "Some/Path/"),
            new TestCaseData(@"  Some/Path/Two ", "Some/Path/Two/"),
            new TestCaseData("Something////", "Something/"),
            new TestCaseData(@"Some///Path//Two/", "Some/Path/Two/")
        };

        static readonly IEnumerable k_AbsolutePathData = new[]
        {
            new TestCaseData("C:/Some/Absolute/Windows/Path"),
            new TestCaseData("/Some/Absolute/Path")
        };
#pragma warning restore 414

        [TestCaseSource(typeof(ScriptableSettingsTests), nameof(k_ValidatePathValidData))]
        public void ValidatePath_ValidPath(string path, string expectedCleanedPath)
        {
            var valid = ScriptableSettingsBase.ValidatePath(path, out var cleanedPath);
            Assert.True(valid);
            Assert.AreEqual(expectedCleanedPath, cleanedPath);
        }

        [Test]
        public void ValidatePath_NullPath()
        {
            LogAssert.Expect(LogType.Warning, ScriptableSettingsBase.NullPathMessage);
            ScriptableSettingsBase.ValidatePath(null, out _);
        }

        [Test]
        public void ValidatePath_PathWithPeriod()
        {
            LogAssert.Expect(LogType.Warning, ScriptableSettingsBase.PathWithPeriodMessage);
            ScriptableSettingsBase.ValidatePath("../Some/Path", out _);
        }

        [TestCaseSource(typeof(ScriptableSettingsTests), nameof(k_AbsolutePathData))]
        public void ValidatePath_AbsolutePath(string path)
        {
            Assert.IsFalse(ScriptableSettingsBase.ValidatePath(path, out _));
        }

        [Test]
        public void ValidatePath_InvalidCharacter()
        {
            LogAssert.Expect(LogType.Warning, ScriptableSettingsBase.PathWithInvalidCharacterMessage);
            ScriptableSettingsBase.ValidatePath(@"Some/Path/With""Quote\\s", out _);
        }
    }
}
