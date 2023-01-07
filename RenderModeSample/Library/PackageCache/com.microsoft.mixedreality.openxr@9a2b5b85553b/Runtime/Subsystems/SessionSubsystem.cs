// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.XR.ARSubsystems;

namespace Microsoft.MixedReality.OpenXR
{
    [Preserve]
    internal class SessionSubsystem : XRSessionSubsystem
    {
        public const string Id = "OpenXR Session";

        private class OpenXRProvider : Provider
        {
            private readonly NativeLibToken m_nativeLibToken;
            private readonly Guid m_sessionGuid;

            private readonly Feature m_allSupportedFeatures = Feature.AnyTrackingMode | Feature.PlaneTracking | Feature.Raycast | Feature.Meshing;
            // The requested features, excluding the requested tracking mode
            private readonly Feature m_requestedBaseFeatures = Feature.PlaneTracking | Feature.Raycast | Feature.Meshing;

            private Feature m_requestedTrackingMode = Feature.PositionAndRotation;
            private NativeSpaceLocationFlags m_trackingStateFlags;

            public OpenXRProvider()
            {
                m_nativeLibToken = MixedRealityFeaturePlugin.nativeLibToken;
                m_sessionGuid = Guid.NewGuid();
            }
            public override void Start() { }
            public override void Stop() { }
            public override void Destroy() { }

            public override Feature currentTrackingMode
            {
                get
                {
                    if (!m_trackingStateFlags.HasFlag(NativeSpaceLocationFlags.OrientationValid) ||
                       !m_trackingStateFlags.HasFlag(NativeSpaceLocationFlags.PositionValid))
                        return Feature.None;

                    if (!m_trackingStateFlags.HasFlag(NativeSpaceLocationFlags.PositionTracked))
                        return Feature.RotationOnly;

                    return Feature.PositionAndRotation;
                }
            }

            public override Feature requestedTrackingMode
            {
                get => m_requestedTrackingMode;
                set
                {
                    if (m_requestedTrackingMode != Feature.PositionAndRotation && m_requestedTrackingMode != Feature.RotationOnly)
                    {
                        Debug.Log("Session supported requested tracking modes are PositionAndRotation and RotationOnly.");
                        return;
                    }
                    m_requestedTrackingMode = value;
                }
            }

            public override TrackingState trackingState
            {
                get
                {
                    if (!m_trackingStateFlags.HasFlag(NativeSpaceLocationFlags.PositionValid) ||
                       !m_trackingStateFlags.HasFlag(NativeSpaceLocationFlags.OrientationValid))
                        return TrackingState.None;

                    if (m_trackingStateFlags.HasFlag(NativeSpaceLocationFlags.All))
                        return TrackingState.Tracking;

                    return TrackingState.Limited;
                }
            }

            public override NotTrackingReason notTrackingReason
            {
                get
                {
                    if (!m_trackingStateFlags.HasFlag(NativeSpaceLocationFlags.PositionValid) ||
                       !m_trackingStateFlags.HasFlag(NativeSpaceLocationFlags.OrientationValid))
                        return NotTrackingReason.Initializing;

                    if (m_trackingStateFlags.HasFlag(NativeSpaceLocationFlags.All))
                        return NotTrackingReason.None;

                    return NotTrackingReason.Relocalizing;
                }
            }

            public override bool matchFrameRateEnabled { get => false; }
            public override bool matchFrameRateRequested { get => false; }
            public override int frameRate { get => 0; } // Framerate is not supported unless matchFrameRateEnabled = true

            public override Feature requestedFeatures { get => m_requestedBaseFeatures | m_requestedTrackingMode; }
            public override IntPtr nativePtr { get => IntPtr.Zero; }
            public override Guid sessionId { get => m_sessionGuid; }

            /// <summary>
            /// Get the session's availability, such as whether the platform supports XR.
            /// SessionAvailability.None: "Default value. The availability is unknown."
            /// SessionAvailability.Supported: "The current device is AR capable (but might require a software update)."
            /// SessionAvailability.Installed: "The required AR software is installed on the device."
            /// </summary>
            public override Promise<SessionAvailability> GetAvailabilityAsync()
            {
                if (OpenXRContext.Current.SystemId != 0)
                {
                    return Promise<SessionAvailability>.CreateResolvedPromise(SessionAvailability.Supported | SessionAvailability.Installed);
                }

                return Promise<SessionAvailability>.CreateResolvedPromise(SessionAvailability.None);
            }

            public override NativeArray<ConfigurationDescriptor> GetConfigurationDescriptors(Allocator allocator)
            {
                // Sessions may have multiple 'modes' of operation, each with a different set of capabilities.
                // Our session only has one such mode, so this array will always be one ConfigurationDescriptor long.
                var nativeArray = new NativeArray<ConfigurationDescriptor>(1, allocator, NativeArrayOptions.UninitializedMemory);
                nativeArray[0] = new ConfigurationDescriptor(IntPtr.Zero, m_allSupportedFeatures, 0);
                return nativeArray;
            }

            public override void OnApplicationPause() { }
            public override void OnApplicationResume() { }

            // Only one configuration is supported, so the configuration settings can be inferred in the standard update.
            public override void Update(XRSessionUpdateParams updateParams, Configuration configuration) => Update(updateParams);
            public override void Update(XRSessionUpdateParams updateParams)
            {
                m_trackingStateFlags = NativeLib.GetFoundationTrackingStateFlags(m_nativeLibToken);
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void RegisterDescriptor()
        {
            XRSessionSubsystemDescriptor.RegisterDescriptor(new XRSessionSubsystemDescriptor.Cinfo
            {
                id = Id,
                providerType = typeof(SessionSubsystem.OpenXRProvider),
                subsystemTypeOverride = typeof(SessionSubsystem),
                supportsInstall = false,
                supportsMatchFrameRate = false
            });
        }
    };

    internal class SessionSubsystemController : SubsystemController
    {
        private readonly NativeLibToken nativeLibToken;
        private static List<XRSessionSubsystemDescriptor> s_SessionDescriptors = new List<XRSessionSubsystemDescriptor>();

        public SessionSubsystemController(NativeLibToken token, IOpenXRContext context) : base(context)
        {
            nativeLibToken = token;
        }

        public override void OnSubsystemCreate(ISubsystemPlugin plugin)
        {
            plugin.CreateSubsystem<XRSessionSubsystemDescriptor, XRSessionSubsystem>(s_SessionDescriptors, SessionSubsystem.Id);
        }

        public override void OnSubsystemDestroy(ISubsystemPlugin plugin)
        {
            plugin.DestroySubsystem<XRSessionSubsystem>();
        }
    }
}
