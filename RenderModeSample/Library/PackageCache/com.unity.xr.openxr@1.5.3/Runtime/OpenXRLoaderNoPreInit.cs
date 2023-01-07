#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.Management;
#endif

namespace UnityEngine.XR.OpenXR
{
    /// <summary>
    /// Loader for managing OpenXR devices that don't support PreInit.
    /// </summary>
    public class OpenXRLoaderNoPreInit : OpenXRLoaderBase
    {
    }
}