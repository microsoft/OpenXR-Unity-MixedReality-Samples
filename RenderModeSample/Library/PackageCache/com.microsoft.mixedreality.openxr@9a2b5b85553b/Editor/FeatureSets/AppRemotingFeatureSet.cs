// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.MixedReality.OpenXR.Remoting;
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;

namespace Microsoft.MixedReality.OpenXR.Editor
{
    [OpenXRFeatureSet(
        FeatureSetId = featureSetId,
        FeatureIds = new string[]
        {
            AppRemotingPlugin.featureId,
            MixedRealityFeaturePlugin.featureId,
            HandTrackingFeaturePlugin.featureId,
        },
        RequiredFeatureIds = new string[]
        {
            AppRemotingPlugin.featureId,
            MixedRealityFeaturePlugin.featureId,
        },
        DefaultFeatureIds = new string[]
        {
            AppRemotingPlugin.featureId,
            MixedRealityFeaturePlugin.featureId,
            HandTrackingFeaturePlugin.featureId,
        },
        UiName = "Holographic Remoting remote app",
        // This will appear as a tooltip for the (?) icon in the loader UI.
        Description = "Enable the Holographic Remoting remote app features.",
        SupportedBuildTargets = new BuildTargetGroup[] { BuildTargetGroup.Standalone, BuildTargetGroup.WSA }
    )]
    sealed class AppRemotingFeatureSet
    {
        internal const string featureSetId = "com.microsoft.openxr.featureset.appremoting";
    }
}
