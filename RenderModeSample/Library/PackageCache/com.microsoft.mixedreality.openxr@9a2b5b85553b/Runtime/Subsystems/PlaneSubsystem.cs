// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;

namespace Microsoft.MixedReality.OpenXR
{
    internal enum XrSceneObjectTypeMSFT
    {
        XR_SCENE_OBJECT_TYPE_UNCATEGORIZED_MSFT = -1,
        XR_SCENE_OBJECT_TYPE_BACKGROUND_MSFT = 1,
        XR_SCENE_OBJECT_TYPE_WALL_MSFT = 2,
        XR_SCENE_OBJECT_TYPE_FLOOR_MSFT = 3,
        XR_SCENE_OBJECT_TYPE_CEILING_MSFT = 4,
        XR_SCENE_OBJECT_TYPE_PLATFORM_MSFT = 5,
        XR_SCENE_OBJECT_TYPE_INFERRED_MSFT = 6,
        XR_SCENE_OBJECT_TYPE_MAX_ENUM_MSFT = 0x7FFFFFFF
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    internal struct NativePlane
    {
        public Guid id;
        public Vector3 position;
        public Quaternion rotation;
        public TrackingState trackingState;
        public Vector2 size;
        public XrSceneObjectTypeMSFT type;
    }

    internal class PlaneSubsystem : XRPlaneSubsystem
    {
        public const string Id = "OpenXR Planefinding";

        private class OpenXRProvider : Provider
        {
            private readonly NativeLibToken nativeLibToken;
            private PlaneDetectionMode m_planeDetectionMode = PlaneDetectionMode.Vertical & PlaneDetectionMode.Horizontal;

            public OpenXRProvider()
            {
                nativeLibToken = MixedRealityFeaturePlugin.nativeLibToken;
            }
            public override void Start()
            {
                NativeLib.StartPlaneSubsystem(nativeLibToken);
            }
            public override void Stop()
            {
                NativeLib.StopPlaneSubsystem(nativeLibToken);
            }
            public override void Destroy()
            {
                NativeLib.DestroyPlaneSubsystem(nativeLibToken);
            }

            public override PlaneDetectionMode currentPlaneDetectionMode { get => m_planeDetectionMode; }
            public override PlaneDetectionMode requestedPlaneDetectionMode
            {
                get => m_planeDetectionMode;
                set
                {
                    m_planeDetectionMode = value;
                    NativeLib.SetPlaneDetectionMode(nativeLibToken, m_planeDetectionMode);
                }
            }

            public unsafe override TrackableChanges<BoundedPlane> GetChanges(BoundedPlane defaultPlane, Allocator allocator)
            {
                uint numAddedPlanes = 0;
                uint numUpdatedPlanes = 0;
                uint numRemovedPlanes = 0;
                NativeLib.GetNumPlaneChanges(nativeLibToken, FrameTime.OnUpdate, ref numAddedPlanes, ref numUpdatedPlanes, ref numRemovedPlanes);

                using (var addedNativePlanes = new NativeArray<NativePlane>((int)numAddedPlanes, allocator, NativeArrayOptions.UninitializedMemory))
                using (var updatedNativePlanes = new NativeArray<NativePlane>((int)numUpdatedPlanes, allocator, NativeArrayOptions.UninitializedMemory))
                using (var removedNativePlanes = new NativeArray<Guid>((int)numRemovedPlanes, allocator, NativeArrayOptions.UninitializedMemory))
                {
                    if (numAddedPlanes + numUpdatedPlanes + numRemovedPlanes > 0)
                    {
                        NativeLib.GetPlaneChanges(nativeLibToken,
                            (uint)(numAddedPlanes * sizeof(NativePlane)),
                            NativeArrayUnsafeUtility.GetUnsafePtr(addedNativePlanes),
                            (uint)(numUpdatedPlanes * sizeof(NativePlane)),
                            NativeArrayUnsafeUtility.GetUnsafePtr(updatedNativePlanes),
                            (uint)(numRemovedPlanes * sizeof(NativePlane)),
                            NativeArrayUnsafeUtility.GetUnsafePtr(removedNativePlanes));
                    }

                    // Added Planes
                    var addedPlanes = Array.Empty<BoundedPlane>();
                    if (numAddedPlanes > 0)
                    {
                        addedPlanes = new BoundedPlane[numAddedPlanes];
                        for (int i = 0; i < numAddedPlanes; ++i)
                            addedPlanes[i] = ToBoundedPlane(addedNativePlanes[i], defaultPlane);
                    }

                    // Updated Planes
                    var updatedPlanes = Array.Empty<BoundedPlane>();
                    if (numUpdatedPlanes > 0)
                    {
                        updatedPlanes = new BoundedPlane[numUpdatedPlanes];
                        for (int i = 0; i < numUpdatedPlanes; ++i)
                            updatedPlanes[i] = ToBoundedPlane(updatedNativePlanes[i], defaultPlane);
                    }

                    // Removed Planes
                    var removedPlanes = Array.Empty<TrackableId>();
                    if (numRemovedPlanes > 0)
                    {
                        removedPlanes = new TrackableId[numRemovedPlanes];
                        for (int i = 0; i < numRemovedPlanes; ++i)
                            removedPlanes[i] = FeatureUtils.ToTrackableId(removedNativePlanes[i]);
                    }

                    return TrackableChanges<BoundedPlane>.CopyFrom(
                        new NativeArray<BoundedPlane>(addedPlanes, allocator),
                        new NativeArray<BoundedPlane>(updatedPlanes, allocator),
                        new NativeArray<TrackableId>(removedPlanes, allocator),
                        allocator);
                }
            }

            private PlaneClassification ToPlaneClassification(XrSceneObjectTypeMSFT type)
            {
                switch (type)
                {
                    case XrSceneObjectTypeMSFT.XR_SCENE_OBJECT_TYPE_WALL_MSFT:
                        return PlaneClassification.Wall;

                    case XrSceneObjectTypeMSFT.XR_SCENE_OBJECT_TYPE_FLOOR_MSFT:
                        return PlaneClassification.Floor;

                    case XrSceneObjectTypeMSFT.XR_SCENE_OBJECT_TYPE_CEILING_MSFT:
                        return PlaneClassification.Ceiling;

                    case XrSceneObjectTypeMSFT.XR_SCENE_OBJECT_TYPE_PLATFORM_MSFT:
                        return PlaneClassification.Table;

                    case XrSceneObjectTypeMSFT.XR_SCENE_OBJECT_TYPE_UNCATEGORIZED_MSFT:
                    case XrSceneObjectTypeMSFT.XR_SCENE_OBJECT_TYPE_BACKGROUND_MSFT:
                    case XrSceneObjectTypeMSFT.XR_SCENE_OBJECT_TYPE_INFERRED_MSFT:
                    case XrSceneObjectTypeMSFT.XR_SCENE_OBJECT_TYPE_MAX_ENUM_MSFT:
                    default:
                        return PlaneClassification.None;
                }
            }

            private BoundedPlane ToBoundedPlane(NativePlane nativePlane, BoundedPlane defaultPlane)
            {
                return new BoundedPlane(
                    FeatureUtils.ToTrackableId(nativePlane.id),
                    TrackableId.invalidId,
                    new Pose(nativePlane.position, nativePlane.rotation),
                    Vector2.zero,
                    nativePlane.size,
                    PlaneAlignment.HorizontalUp,
                    nativePlane.trackingState,
                    defaultPlane.nativePtr,
                    ToPlaneClassification(nativePlane.type)); // TODO: Replace the nativePtr
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void RegisterDescriptor()
        {
            XRPlaneSubsystemDescriptor.Create(new XRPlaneSubsystemDescriptor.Cinfo
            {
                id = Id,
                providerType = typeof(PlaneSubsystem.OpenXRProvider),
                subsystemTypeOverride = typeof(PlaneSubsystem),
                supportsArbitraryPlaneDetection = true,
                supportsBoundaryVertices = false,
                supportsClassification = true,
                supportsHorizontalPlaneDetection = true,
                supportsVerticalPlaneDetection = true,
            });
        }
    };

    internal class PlaneSubsystemController : SubsystemController
    {
        private readonly NativeLibToken nativeLibToken;
        private static List<XRPlaneSubsystemDescriptor> s_PlaneDescriptors = new List<XRPlaneSubsystemDescriptor>();

        public PlaneSubsystemController(NativeLibToken token, IOpenXRContext context) : base(context)
        {
            nativeLibToken = token;
        }

        public override void OnSubsystemCreate(ISubsystemPlugin plugin)
        {
            plugin.CreateSubsystem<XRPlaneSubsystemDescriptor, XRPlaneSubsystem>(s_PlaneDescriptors, PlaneSubsystem.Id);
        }

        public override void OnSubsystemDestroy(ISubsystemPlugin plugin)
        {
            plugin.DestroySubsystem<XRPlaneSubsystem>();
        }
    }
}
