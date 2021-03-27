// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using UnityEngine;
using UnityEngine.SpatialTracking;

namespace Microsoft.MixedReality.OpenXR.Samples
{
    public enum OpenXRSpaceType
    {
        View,

        LeftHandGrip,
        RightHandGrip,

        LeftHandAim,
        RightHandAim,

        EyeGaze,
    }

    [UnityEngine.Scripting.Preserve]
    public class OpenXRSpace : MonoBehaviour
    {

        [SerializeField, Tooltip("Attach object placement to OpenXR space.")]
        private OpenXRSpaceType spaceType = OpenXRSpaceType.View;

        private TrackedPoseDriver trackedPoseDriver;

        // Start is called before the first frame update
        void Start()
        {
            SetTrackerSpace();
            Application.onBeforeRender += Application_onBeforeRender;
        }

        private void Application_onBeforeRender()
        {
            // Apply pose before rendering
        }

        void SetTrackerSpace()
        {
            switch (spaceType)
            {
                case OpenXRSpaceType.View:
                    EnsureTrackedPoseDriver();
                    trackedPoseDriver.SetPoseSource(TrackedPoseDriver.DeviceType.GenericXRDevice, TrackedPoseDriver.TrackedPose.Center);
                    break;
                case OpenXRSpaceType.LeftHandGrip:
                    EnsureTrackedPoseDriver();
                    trackedPoseDriver.SetPoseSource(TrackedPoseDriver.DeviceType.GenericXRController, TrackedPoseDriver.TrackedPose.LeftPose);
                    break;
                case OpenXRSpaceType.RightHandGrip:
                    EnsureTrackedPoseDriver();
                    trackedPoseDriver.SetPoseSource(TrackedPoseDriver.DeviceType.GenericXRController, TrackedPoseDriver.TrackedPose.RightPose);
                    break;
            }
        }

        void EnsureTrackedPoseDriver()
        {
            trackedPoseDriver = gameObject.GetComponent<TrackedPoseDriver>();
            if (trackedPoseDriver == null)
            {
                trackedPoseDriver = gameObject.AddComponent<TrackedPoseDriver>();
            }
        }

        void RemoveTrackedPosedDriver()
        {
            trackedPoseDriver = gameObject.GetComponent<TrackedPoseDriver>();
            if (trackedPoseDriver != null)
            {
                Destroy(trackedPoseDriver);
                trackedPoseDriver = null;
            }
        }
    }

}

