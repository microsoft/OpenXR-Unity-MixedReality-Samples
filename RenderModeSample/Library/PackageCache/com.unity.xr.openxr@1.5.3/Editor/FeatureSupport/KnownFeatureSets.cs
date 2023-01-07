using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEditor.Build;
using UnityEditor.XR.OpenXR;

using UnityEngine;
using UnityEngine.XR.OpenXR;

namespace UnityEditor.XR.OpenXR.Features
{
    internal static class KnownFeatureSetsContent
    {
        internal static readonly string s_MicrosoftFeatureSetId = "com.microsoft.openxr.featureset.wmr";
        internal static readonly string s_MicrosoftWMRTitle = "Windows Mixed Reality";
        internal static readonly string s_MicrosoftInformationText = "Enable the full suite of features for Windows Mixed Reality headsets.";

        internal static readonly string s_MicrosoftHoloLensFeatureSetId = "com.microsoft.openxr.featureset.hololens";
        internal static readonly string s_MicrosoftHoloLensTitle = "Microsoft HoloLens";
        internal static readonly string s_MicrosoftHoloLensInformationText = "Enable the full suite of features for Microsoft HoloLens 2.";

        internal static readonly string s_MicrosoftDownloadText = "This package must be installed. Click this icon to visit the download page for this package.";
        internal static readonly string s_MicrosoftDownloadLink = "http://aka.ms/openxr-unity-install";
    }


    internal static class KnownFeatureSets
    {
        internal static Dictionary<BuildTargetGroup, OpenXRFeatureSetManager.FeatureSet[]> k_KnownFeatureSets =
            new Dictionary<BuildTargetGroup, OpenXRFeatureSetManager.FeatureSet[]>(){
                { BuildTargetGroup.Standalone,
                    new OpenXRFeatureSetManager.FeatureSet[]{
                        new OpenXRFeatureSetManager.FeatureSet(){
                            isEnabled = false,
                            name = KnownFeatureSetsContent.s_MicrosoftWMRTitle,
                            featureSetId = KnownFeatureSetsContent.s_MicrosoftFeatureSetId,
                            description = KnownFeatureSetsContent.s_MicrosoftInformationText,
                            downloadText = KnownFeatureSetsContent.s_MicrosoftDownloadText,
                            downloadLink = KnownFeatureSetsContent.s_MicrosoftDownloadLink,
                        },
                    }
                },
                { BuildTargetGroup.WSA,
                    new OpenXRFeatureSetManager.FeatureSet[]{
                        new OpenXRFeatureSetManager.FeatureSet(){
                            isEnabled = false,
                            name = KnownFeatureSetsContent.s_MicrosoftHoloLensTitle,
                            featureSetId = KnownFeatureSetsContent.s_MicrosoftHoloLensFeatureSetId,
                            description = KnownFeatureSetsContent.s_MicrosoftHoloLensInformationText,
                            downloadText = KnownFeatureSetsContent.s_MicrosoftDownloadText,
                            downloadLink = KnownFeatureSetsContent.s_MicrosoftDownloadLink,
                        },
                    }
                },
            };
    }
}