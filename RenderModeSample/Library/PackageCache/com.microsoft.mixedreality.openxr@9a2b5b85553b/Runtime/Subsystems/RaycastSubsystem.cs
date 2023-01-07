// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Unity.Collections;

namespace Microsoft.MixedReality.OpenXR
{
    internal class RaycastSubsystem : XRRaycastSubsystem
    {
        public const string Id = "OpenXR Raycasting";

        private class OpenXRProvider : Provider
        {
            ARPlaneManager m_arPlaneManager = null;
            public override bool TryAddRaycast(Vector2 screenPoint, float estimatedDistance, out XRRaycast raycast)
            {
                Debug.LogError("Persistent raycasts are not supported; use single raycasts instead. More information about single and persistent raycasts is available at https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@4.1/manual/raycast-manager.html#single-raycasts.");
                raycast = default;
                return false;
            }
            public override bool TryAddRaycast(Ray ray, float estimatedDistance, out XRRaycast raycast)
            {
                Debug.LogError("Persistent raycasts are not supported; use single raycasts instead. More information about single and persistent raycasts is available at https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@4.1/manual/raycast-manager.html#single-raycasts.");
                raycast = default;
                return false;
            }
            public override void RemoveRaycast(TrackableId trackableId)
            {
                Debug.LogError("Persistent raycasts are not supported; use single raycasts instead. More information about single and persistent raycasts is available at https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@4.1/manual/raycast-manager.html#single-raycasts.");
            }

            public override TrackableChanges<XRRaycast> GetChanges(XRRaycast defaultRaycast, Allocator allocator)
            {
                // Get the ARPlaneManager each frame to detect changes.
                // If the app Raycasts before this GetChanges, we won't forward the request to the ARPlaneManager,
                // but if the raycast request is that early on the first frame, planes likely aren't available yet.
                m_arPlaneManager = GameObject.FindObjectOfType<ARPlaneManager>();
                return base.GetChanges(defaultRaycast, allocator);
            }

            public override void Stop() => m_arPlaneManager = null; // Reset the cached ARPlaneManager.
            public override void Destroy() => m_arPlaneManager = null; // Reset the cached ARPlaneManager.

            public override NativeArray<XRRaycastHit> Raycast(
                XRRaycastHit defaultRaycastHit,
                Ray ray,
                TrackableType trackableTypeMask,
                Allocator allocator)
            {
                // If we don't have a reference to the ARPlaneManager, we don't raycast.
                // The ARPlaneManager Raycast is typically only called when it's enabled.
                if (m_arPlaneManager == null || !m_arPlaneManager.enabled)
                    return new NativeArray<XRRaycastHit>(0, allocator);

                // Use the ARPlaneManager's raycast - this is the only raycast we currently need to support.
                return m_arPlaneManager.Raycast(ray, trackableTypeMask, allocator);
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void RegisterDescriptor()
        {
            XRRaycastSubsystemDescriptor.RegisterDescriptor(new XRRaycastSubsystemDescriptor.Cinfo
            {
                id = Id,
                providerType = typeof(RaycastSubsystem.OpenXRProvider),
                subsystemTypeOverride = typeof(RaycastSubsystem),
                supportedTrackableTypes = TrackableType.Planes,
                supportsTrackedRaycasts = false,

                // If this is not supported, ARFoundation will convert the ray to world space, then call the world-based raycast
                supportsViewportBasedRaycast = false,

                // If this is not supported, ARFoundation will fallback to the ARPlaneManager's raycast.
                // On editor remoting, this fallback redirect is broken, so we redirect in Raycast above.
                supportsWorldBasedRaycast = true
            });
        }
    };

    internal class RaycastSubsystemController : SubsystemController
    {
        private readonly NativeLibToken nativeLibToken;
        private static List<XRRaycastSubsystemDescriptor> s_RaycastDescriptors = new List<XRRaycastSubsystemDescriptor>();

        public RaycastSubsystemController(NativeLibToken token, IOpenXRContext context) : base(context)
        {
            nativeLibToken = token;
        }

        public override void OnSubsystemCreate(ISubsystemPlugin plugin)
        {
            plugin.CreateSubsystem<XRRaycastSubsystemDescriptor, XRRaycastSubsystem>(s_RaycastDescriptors, RaycastSubsystem.Id);
        }

        public override void OnSubsystemDestroy(ISubsystemPlugin plugin)
        {
            plugin.DestroySubsystem<XRRaycastSubsystem>();
        }
    }
}
