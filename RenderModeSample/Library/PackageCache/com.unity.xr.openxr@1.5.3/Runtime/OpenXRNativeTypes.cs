using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Unity.XR.OpenXR.Features.MockRuntime")]
[assembly: InternalsVisibleTo("Unity.XR.OpenXR.Features.ConformanceAutomation")]

namespace UnityEngine.XR.OpenXR.NativeTypes
{
    /// <summary>
    /// Runtime XR Environment Blend Mode. <see cref="Features.OpenXRFeature.SetEnvironmentBlendMode"/>
    /// </summary>
    public enum XrEnvironmentBlendMode
    {
        /// <summary>
        /// Select XR_ENVIRONMENT_BLEND_MODE_OPAQUE for runtime.
        /// </summary>
        Opaque = 1,
        /// <summary>
        /// Select XR_ENVIRONMENT_BLEND_MODE_ADDITIVE for runtime.
        /// </summary>
        Additive = 2,
        /// <summary>
        /// Select XR_ENVIRONMENT_BLEND_MODE_ALPHA_BLEND for runtime.
        /// </summary>
        AlphaBlend = 3
    }

    internal enum XrResult
    {
        Success = 0,
        TimeoutExpored = 1,
        LossPending = 3,
        EventUnavailable = 4,
        SpaceBoundsUnavailable = 7,
        SessionNotFocused = 8,
        FrameDiscarded = 9,
        ValidationFailure = -1,
        RuntimeFailure = -2,
        OutOfMemory = -3,
        ApiVersionUnsupported = -4,
        InitializationFailed = -6,
        FunctionUnsupported = -7,
        FeatureUnsupported = -8,
        ExtensionNotPresent = -9,
        LimitReached = -10,
        SizeInsufficient = -11,
        HandleInvalid = -12,
        InstanceLOst = -13,
        SessionRunning = -14,
        SessionNotRunning = -16,
        SessionLost = -17,
        SystemInvalid = -18,
        PathInvalid = -19,
        PathCountExceeded = -20,
        PathFormatInvalid = -21,
        PathUnsupported = -22,
        LayerInvalid = -23,
        LayerLimitExceeded = -24,
        SpwachainRectInvalid = -25,
        SwapchainFormatUnsupported = -26,
        ActionTypeMismatch = -27,
        SessionNotReady = -28,
        SessionNotStopping = -29,
        TimeInvalid = -30,
        ReferenceSpaceUnsupported = -31,
        FileAccessError = -32,
        FileContentsInvalid = -33,
        FormFactorUnsupported = -34,
        FormFactorUnavailable = -35,
        ApiLayerNotPresent = -36,
        CallOrderInvalid = -37,
        GraphicsDeviceInvalid = -38,
        PoseInvalid = -39,
        IndexOutOfRange = -40,
        ViewConfigurationTypeUnsupported = -41,
        EnvironmentBlendModeUnsupported = -42,
        NameDuplicated = -44,
        NameInvalid = -45,
        ActionsetNotAttached = -46,
        ActionsetsAlreadyAttached = -47,
        LocalizedNameDuplicated = -48,
        LocalizedNameInvalid = -49,
        AndroidThreadSettingsIdInvalidKHR = -1000003000,
        AndroidThreadSettingsdFailureKHR = -1000003001,
        CreateSpatialAnchorFailedMSFT = -1000039001,
        SecondaryViewConfigurationTypeNotEnabledMSFT = -1000053000,
        MaxResult = 0x7FFFFFFF
    }

    internal enum XrViewConfigurationType
    {
        PrimaryMono = 1,
        PrimaryStereo = 2,
        PrimaryQuadVarjo = 1000037000,
        SecondaryMonoFirstPersonObserver = 1000054000,
        SecondaryMonoThirdPersonObserver = 1000145000
    }

    [Flags]
    internal enum XrSpaceLocationFlags
    {
        None = 0,
        OrientationValid = 1,
        PositionValid = 2,
        OrientationTracked = 4,
        PositionTracked = 8
    }

    [Flags]
    internal enum XrViewStateFlags
    {
        None = 0,
        OrientationValid = 1,
        PositionValid = 2,
        OrientationTracked = 4,
        PositionTracked = 8
    }

    [Flags]
    internal enum XrReferenceSpaceType
    {
        View = 1,
        Local = 2,
        Stage = 3,
        UnboundedMsft = 1000038000,
        CombinedEyeVarjo = 1000121000
    }

    internal enum XrSessionState
    {
        Unknown = 0,
        Idle = 1,
        Ready = 2,
        Synchronized = 3,
        Visible = 4,
        Focused = 5,
        Stopping = 6,
        LossPending = 7,
        Exiting = 8,
    }

    internal struct XrVector2f
    {
        float x;
        float y;

        public XrVector2f(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public XrVector2f(Vector2 value)
        {
            x = value.x;
            y = value.y;
        }
    };

    internal struct XrVector3f
    {
        float x;
        float y;
        float z;

        public XrVector3f(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = -z;
        }

        public XrVector3f(Vector3 value)
        {
            x = value.x;
            y = value.y;
            z = -value.z;
        }
    };

    internal struct XrQuaternionf
    {
        float x;
        float y;
        float z;
        float w;

        public XrQuaternionf(float x, float y, float z, float w)
        {
            this.x = -x;
            this.y = -y;
            this.z = z;
            this.w = w;
        }

        public XrQuaternionf(Quaternion quaternion)
        {
            this.x = -quaternion.x;
            this.y = -quaternion.y;
            this.z = quaternion.z;
            this.w = quaternion.w;
        }
    };

    internal struct XrPosef
    {
        XrQuaternionf orientation;
        XrVector3f position;

        public XrPosef(Vector3 vec3, Quaternion quaternion)
        {
            this.position = new XrVector3f(vec3);
            this.orientation = new XrQuaternionf(quaternion);
        }
    };
}
