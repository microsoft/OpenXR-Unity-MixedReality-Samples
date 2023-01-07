using UnityEngine;
using UnityEngine.XR.Management;

namespace Unity.XR.Management.TestPackage
{
    [XRConfigurationData("Test Settings", Constants.k_SettingsKey)]
    public class TestSettings : ScriptableObject
    {

#if !UNITY_EDITOR
        internal static TestSettings s_Settings;

        public void Awake()
        {
            s_Settings = this;
        }
#endif
    }
}
