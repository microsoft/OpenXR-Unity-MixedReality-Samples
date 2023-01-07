
using NUnit.Framework;
using System.Runtime.InteropServices;

namespace UnityEngine.XR.OpenXR.Tests
{
    internal class UnityVersionTests
    {
        public static readonly string[] s_ValidStrings =
        {
            "2020.3.17f1",
            "2022.1.0a12",
            "2020.2.0b16",
            "2020.2.0rc16",
            "2020.2.0rc1",
            "2020.2.0p1",
            "2020.3.17F1"
        };

        public static readonly string[] s_InvalidStrings =
        {
            "20.20.3.17f1",
            "2022.1.0x12",
            "2020.2.b16",
            "a.1.1rc1",
            "1.a.1rc1",
            "1.1.1rc",
            "2020.2,1p1"
        };

        private static readonly string[] s_SequentialVersions =
        {
            "2019.4.0a1",
            "2020.4.0a1",
            "2020.5.0a1",
            "2020.5.0b1",
            "2020.5.0rc1",
            "2020.5.0f1",
            "2020.5.0p1",
            "2020.5.1p1",
            "2020.5.1p2"
        };

        [Test]
        public void ValidStrings ([ValueSource(nameof(s_ValidStrings))] string versionString)
        {
            var version = Internal_GetUnityVersion(versionString);
            Assert.IsTrue(version != 0);
        }

        [Test]
        public void InvalidStrings([ValueSource(nameof(s_InvalidStrings))] string versionString)
        {
            var version = Internal_GetUnityVersion(versionString);
            Assert.IsTrue(version == 0);
        }


        [Test]
        public void NumericalCorrectness ()
        {
            // Convert all of the version strings to numbers
            var versions = new ulong[s_SequentialVersions.Length];
            for(int i=0; i<versions.Length; i++)
            {
                versions[i] = Internal_GetUnityVersion(s_SequentialVersions[i]);
                Assert.IsFalse(versions[i] == 0, $"StringToVersion failed on `{s_SequentialVersions[i]}`");
            }

            // Make sure all versions are greater than all versions before them in the list
            for(int i=1; i<versions.Length; i++)
            {
                for(int j=i-1; j>=0; j--)
                {
                    Assert.IsTrue(versions[i] > versions[j], $"{s_SequentialVersions[i]} was not greater than {s_SequentialVersions[j]}");
                }
            }
        }

        private const string LibraryName = "UnityOpenXR";

        [DllImport(LibraryName, EntryPoint = "NativeConfig_GetUnityVersion", CharSet = CharSet.Ansi)]
        static extern uint Internal_GetUnityVersion(string unityVersion);
    }
}
