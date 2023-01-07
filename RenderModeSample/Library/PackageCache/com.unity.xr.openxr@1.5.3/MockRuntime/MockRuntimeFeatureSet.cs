#if USE_MOCK_FEATURE_SET

using System;

#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.XR.OpenXR.Features;

namespace UnityEngine.XR.OpenXR.Features.Mock
{
    [OpenXRFeatureSet(
        FeatureIds = new string[] {
            MockRuntime.featureId,
            Interactions.KHRSimpleControllerProfile.featureId,
            },
        UiName = "Mock Runtime",
        Description = "Mock Runtime Feature set support. Enabling this will override any current OpenXR runtime selection.",
        FeatureSetId = "com.unity.openxr.featureset.mockruntime",
        SupportedBuildTargets = new BuildTargetGroup[]{ BuildTargetGroup.Standalone }
    )]
    sealed class MockRuntimeFeatureSets {}
}

#endif //UNITY_EDITOR

#endif //USE_MOCK_FEATURE_SET
