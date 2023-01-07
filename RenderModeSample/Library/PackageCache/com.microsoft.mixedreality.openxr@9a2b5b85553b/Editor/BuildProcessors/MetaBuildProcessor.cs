// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Xml;
using UnityEditor.Build.Reporting;
using UnityEditor.XR.OpenXR.Features;
using static Microsoft.MixedReality.OpenXR.Editor.BuildProcessorHelpers;
using static Microsoft.MixedReality.OpenXR.Editor.BuildProcessorHelpers.AndroidManifest;

namespace Microsoft.MixedReality.OpenXR.Editor
{
    internal class MetaBuildProcessor : OpenXRFeatureBuildHooks
    {
        public override int callbackOrder => 1;

        public override Type featureType =>
#if UNITY_OPENXR_1_6_OR_NEWER
            typeof(UnityEngine.XR.OpenXR.Features.MetaQuestSupport.MetaQuestFeature);
#else
            typeof(UnityEngine.XR.OpenXR.Features.OculusQuestSupport.OculusQuestFeature);
#endif

        protected override void OnPreprocessBuildExt(BuildReport report) { }

        protected override void OnPostGenerateGradleAndroidProjectExt(string path)
        {
            HandTrackingFeaturePlugin handTrackingFeaturePlugin = GetOpenXRFeature<HandTrackingFeaturePlugin>();
            bool handTrackingEnabled = handTrackingFeaturePlugin != null && handTrackingFeaturePlugin.enabled;
            bool motionControllerModelEnabled = IsFeatureEnabled<MotionControllerFeaturePlugin>();

            if (!handTrackingEnabled && !motionControllerModelEnabled)
            {
                return;
            }

            AndroidManifest androidManifest = new AndroidManifest(GetManifestPath(path));

            if (handTrackingEnabled)
            {
                androidManifest.EnsurePermission("com.oculus.permission.HAND_TRACKING");
                androidManifest.EnsureFeature("oculus.software.handtracking", false);

                if (handTrackingFeaturePlugin.QuestHandTrackingMode == HandTrackingFeaturePlugin.QuestHandTracking.v2)
                {
                    androidManifest.EnsureMetaData("com.oculus.handtracking.version", "V2.0");
                }
                else if (handTrackingFeaturePlugin.QuestHandTrackingMode == HandTrackingFeaturePlugin.QuestHandTracking.v1)
                {
                    androidManifest.EnsureMetaData("com.oculus.handtracking.version", "V1.0");
                }
            }

            if (motionControllerModelEnabled)
            {
                androidManifest.EnsurePermission("com.oculus.permission.RENDER_MODEL");
                androidManifest.EnsureFeature("com.oculus.feature.RENDER_MODEL");
            }

            androidManifest.Save();
        }

        protected override void OnPostprocessBuildExt(BuildReport report) { }
    }

    internal static class AndroidManifestExtensions
    {
        internal static void EnsurePermission(this AndroidManifest manifest, string permissionString)
        {
            XmlNode usesPermission = null;
            foreach (XmlNode child in manifest.RootElement.ChildNodes)
            {
                if (child.Name == "uses-permission" &&
                    HasAttribute(child, "android:name", permissionString))
                {
                    usesPermission = child;

                    if (usesPermission != null)
                    {
                        break;
                    }
                }
            }

            if (usesPermission == null)
            {
                usesPermission = manifest.RootElement.AppendChild(manifest.CreateElement("uses-permission"));
                usesPermission.Attributes.Append(manifest.CreateAndroidAttribute("name", permissionString));
            }
        }

        internal static void EnsureFeature(this AndroidManifest manifest, string featureString, bool? required = null)
        {
            XmlNode usesFeature = null;
            foreach (XmlNode child in manifest.RootElement.ChildNodes)
            {
                if (child.Name == "uses-feature" &&
                    HasAttribute(child, "android:name", featureString))
                {
                    usesFeature = child;

                    if (usesFeature != null)
                    {
                        break;
                    }
                }
            }

            if (usesFeature == null)
            {
                usesFeature = manifest.RootElement.AppendChild(manifest.CreateElement("uses-feature"));
                usesFeature.Attributes.Append(manifest.CreateAndroidAttribute("name", featureString));
            }

            if (required.HasValue && !SetAttribute(usesFeature, "android:required", required.Value.ToString()))
            {
                usesFeature.Attributes.Append(manifest.CreateAndroidAttribute("required", required.Value.ToString()));
            }
        }

        internal static void EnsureMetaData(this AndroidManifest manifest, string nameString, string valueString)
        {
            XmlNode metaData = null;
            foreach (XmlNode child in manifest.ApplicationElement.ChildNodes)
            {
                if (child.Name == "meta-data" &&
                    HasAttribute(child, "android:name", nameString))
                {
                    metaData = child;

                    if (metaData != null)
                    {
                        break;
                    }
                }
            }

            if (metaData == null)
            {
                metaData = manifest.ApplicationElement.AppendChild(manifest.CreateElement("meta-data"));
                metaData.Attributes.Append(manifest.CreateAndroidAttribute("name", nameString));
            }

            if (!SetAttribute(metaData, "android:value", valueString))
            {
                metaData.Attributes.Append(manifest.CreateAndroidAttribute("value", valueString));
            }
        }
    }
}
