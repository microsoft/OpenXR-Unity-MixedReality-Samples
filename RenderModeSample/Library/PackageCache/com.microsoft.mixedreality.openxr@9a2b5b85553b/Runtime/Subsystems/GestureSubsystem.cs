// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.XR.OpenXR;

namespace Microsoft.MixedReality.OpenXR
{
    [Flags]
    internal enum NativeDirectionFlags
    {
        X = 1,
        Y = 2,
        Z = 4,
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    internal struct NativeGesturePoseData
    {
        public ulong gestureTime;
        public Pose headPose;
        public NativeSpaceLocationFlags headPoseFlags;
        public Pose eyeGazePose;
        public NativeSpaceLocationFlags eyeGazePoseFlags;
        public Pose handAimPose;
        public NativeSpaceLocationFlags handAimPoseFlags;
        public Pose handGripPose;
        public NativeSpaceLocationFlags handGripPoseFlags;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    internal struct NativeGestureEventData
    {
        public GestureEventType eventType;
        public GestureHandedness handedness;
        public NativeGesturePoseData poseData;
        public TappedEventData tappedData;
        public ManipulationEventData manipulationData;
        public NavigationEventData navigationData;
    }

    internal static class GestureSubsystemExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValid(this NativeSpaceLocationFlags flags)
        {
            return flags.HasFlag(NativeSpaceLocationFlags.OrientationValid) &&
            flags.HasFlag(NativeSpaceLocationFlags.PositionValid);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsTracked(this NativeSpaceLocationFlags flags)
        {
            return flags.HasFlag(NativeSpaceLocationFlags.OrientationTracked) &&
            flags.HasFlag(NativeSpaceLocationFlags.PositionTracked);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsTappedEvent(this NativeGestureEventData eventData)
        {
            return eventData.eventType.HasFlag(GestureEventType.Tapped);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsManipulationEvent(this NativeGestureEventData eventData)
        {
            var eventType = eventData.eventType;
            return eventType.HasFlag(GestureEventType.ManipulationStarted) ||
            eventType.HasFlag(GestureEventType.ManipulationUpdated) ||
            eventType.HasFlag(GestureEventType.ManipulationCompleted);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNavigationEvent(this NativeGestureEventData eventData)
        {
            var eventType = eventData.eventType;
            return eventType.HasFlag(GestureEventType.NavigationStarted) ||
            eventType.HasFlag(GestureEventType.NavigationUpdated) ||
            eventType.HasFlag(GestureEventType.NavigationCompleted);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T? Get<T>(this NativeGestureEventData eventData, T value, bool hasValue) where T : struct
        {
            if (hasValue)
            {
                return value;
            }
            return null;
        }
    }
    internal class GestureSubsystem : Disposable
    {
        private readonly static NativeLibToken nativeLibToken = MixedRealityFeaturePlugin.nativeLibToken;
        private readonly ulong m_gestureRecognizerHandle = 0;
        private GestureSettings m_gestureSettings = GestureSettings.None;
        private bool m_running = false;
        private readonly object m_runningLock = new object();

        internal static GestureSubsystem TryCreateGestureSubsystem(GestureSettings settings)
        {
            MixedRealityFeaturePlugin feature = OpenXRSettings.Instance.GetFeature<MixedRealityFeaturePlugin>();
            if (feature == null || !feature.enabled)
            {
                Debug.LogError($"{MixedRealityFeaturePlugin.featureName} is not enabled.");
                return null;
            }

            ulong handle = NativeLib.TryCreateGestureRecognizer(nativeLibToken, settings);
            if (handle == 0)
            {
                Debug.LogError($"GestureSubsystem failed to initialize with settings: {settings}.");
            }

            return new GestureSubsystem(settings, handle);
        }

        private GestureSubsystem(GestureSettings settings, ulong handle)
        {
            m_gestureRecognizerHandle = handle;
            m_gestureSettings = settings;
        }

        internal GestureSettings GestureSettings
        {
            get { return m_gestureSettings; }
            set
            {
                if (m_gestureSettings != value)
                {
                    if (NativeLib.TrySetGestureSettings(nativeLibToken, m_gestureRecognizerHandle, value))
                    {
                        m_gestureSettings = value;
                    }
                    else
                    {
                        Debug.LogWarning($"Cannot set gesture setting to {value}");
                    }
                }
            }
        }

        internal bool TryGetNextEvent(ref GestureEventData eventData)
        {
            return NativeLib.TryGetNextEventData(nativeLibToken, m_gestureRecognizerHandle, ref eventData);
        }

        internal void CancelPendingGestures()
        {
            NativeLib.CancelPendingGesture(nativeLibToken, m_gestureRecognizerHandle);
        }

        protected override void DisposeNativeResources()
        {
            base.DisposeNativeResources();
            NativeLib.DestroyGestureRecognizer(nativeLibToken, m_gestureRecognizerHandle);
        }

        internal void Start()
        {
            lock (m_runningLock)
            {
                if (m_running)
                {
                    Debug.LogError($"GestureSubsystem is already started.");
                    return;
                }
                NativeLib.StartGestureRecognizer(nativeLibToken, m_gestureRecognizerHandle);
                m_running = true;
            }
        }

        internal void Stop()
        {
            lock (m_runningLock)
            {
                if (!m_running)
                {
                    Debug.LogError($"GestureSubsystem cannot be stopped before started.");
                    return;
                }
                m_running = false;
                NativeLib.StopGestureRecognizer(nativeLibToken, m_gestureRecognizerHandle);
            }
        }
    }
}