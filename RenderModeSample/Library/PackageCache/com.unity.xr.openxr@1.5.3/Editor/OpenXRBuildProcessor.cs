using UnityEditor.XR.Management;
using UnityEngine.XR.OpenXR;

namespace UnityEditor.XR.OpenXR
{
    internal class OpenXRBuildProcessor : XRBuildHelper<OpenXRSettings>
    {
        public override string BuildSettingsKey => Constants.k_SettingsKey;

        public override UnityEngine.Object SettingsForBuildTargetGroup(BuildTargetGroup buildTargetGroup)
        {
            EditorBuildSettings.TryGetConfigObject(Constants.k_SettingsKey, out OpenXRPackageSettings packageSettings);
            if (packageSettings == null)
                return null;
            return packageSettings.GetSettingsForBuildTargetGroup(buildTargetGroup);
        }
    }
}