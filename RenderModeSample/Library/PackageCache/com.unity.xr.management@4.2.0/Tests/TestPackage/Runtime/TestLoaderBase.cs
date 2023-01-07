using System.Collections.Generic;

using UnityEngine.XR.Management;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.Management;
#endif

using UnityEngine.XR.Management.Tests.Standalone;

namespace Unity.XR.Management.TestPackage
{
    public class TestLoaderBase : XRLoaderHelper
    {
#if UNITY_EDITOR
        public TestLoaderBase()
        {
            WasAssigned = false;
            WasUnassigned = false;
        }

        public static bool WasAssigned { get; set; }
        public static bool WasUnassigned { get; set; }
#endif
        static List<StandaloneSubsystemDescriptor> s_StandaloneSubsystemDescriptors =
            new List<StandaloneSubsystemDescriptor>();

        public StandaloneSubsystem inputSubsystem
        {
            get { return GetLoadedSubsystem<StandaloneSubsystem>(); }
        }

        TestSettings GetSettings()
        {
            TestSettings settings = null;
            // When running in the Unity Editor, we have to load user's customization of configuration data directly from
            // EditorBuildSettings. At runtime, we need to grab it from the static instance field instead.
#if UNITY_EDITOR
            UnityEditor.EditorBuildSettings.TryGetConfigObject(Constants.k_SettingsKey, out settings);
#else
            settings = TestSettings.s_RuntimeInstance;
#endif
            return settings;
        }

#region XRLoader API Implementation

        /// <summary>Implementaion of <see cref="XRLoader.Initialize"/></summary>
        /// <returns>True if successful, false otherwise</returns>
        public override bool Initialize()
        {
            TestSettings settings = GetSettings();
            if (settings != null)
            {
                // TODO: Pass settings off to plugin prior to subsystem init.
            }

            CreateSubsystem<StandaloneSubsystemDescriptor, StandaloneSubsystem>(s_StandaloneSubsystemDescriptors, "Standalone Subsystem");

            return false;
        }

        /// <summary>Implementaion of <see cref="XRLoader.Start"/></summary>
        /// <returns>True if successful, false otherwise</returns>
        public override bool Start()
        {
            StartSubsystem<StandaloneSubsystem>();
            return true;
        }

        /// <summary>Implementaion of <see cref="XRLoader.Stop"/></summary>
        /// <returns>True if successful, false otherwise</returns>
        public override bool Stop()
        {
            StopSubsystem<StandaloneSubsystem>();
            return true;
        }

        /// <summary>Implementaion of <see cref="XRLoader.Deinitialize"/></summary>
        /// <returns>True if successful, false otherwise</returns>
        public override bool Deinitialize()
        {
            DestroySubsystem<StandaloneSubsystem>();
            return true;
        }

#if UNITY_EDITOR
        public override void WasAssignedToBuildTarget(BuildTargetGroup buildTargetGroup)
        {
            WasAssigned = true;
        }

        public override void WasUnassignedFromBuildTarget(BuildTargetGroup buildTargetGroup)
        {
            WasUnassigned = true;
        }
#endif
#endregion
    }
}
