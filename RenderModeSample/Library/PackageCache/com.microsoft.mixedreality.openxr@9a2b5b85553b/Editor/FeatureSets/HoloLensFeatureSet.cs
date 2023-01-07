// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using UnityEditor;
using UnityEditor.XR.OpenXR.Features;

namespace Microsoft.MixedReality.OpenXR.Editor
{
    [OpenXRFeatureSet(
        FeatureSetId = featureSetId,
        FeatureIds = new string[]
        {
            MixedRealityFeaturePlugin.featureId,
            HandTrackingFeaturePlugin.featureId,
            MotionControllerFeaturePlugin.featureId,
        },
        RequiredFeatureIds = new string[]
        {
            MixedRealityFeaturePlugin.featureId,
        },
        DefaultFeatureIds = new string[]
        {
            MixedRealityFeaturePlugin.featureId,
            HandTrackingFeaturePlugin.featureId,
            MotionControllerFeaturePlugin.featureId,
        },
        UiName = "Microsoft HoloLens",
        // This will appear as a tooltip for the (?) icon in the loader UI.
        Description = "Enable the full suite of features for Microsoft HoloLens 2.",
        SupportedBuildTargets = new BuildTargetGroup[] { BuildTargetGroup.WSA }
    )]
    sealed class HoloLensFeatureSet
    {
        internal const string featureSetId = "com.microsoft.openxr.featureset.hololens";
    }
}
