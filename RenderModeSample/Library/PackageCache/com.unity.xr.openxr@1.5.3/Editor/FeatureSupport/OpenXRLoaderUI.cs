using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.XR.Management;

using UnityEngine;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;

namespace UnityEditor.XR.OpenXR.Features
{
    static class Content
    {
        public const float k_IconSize = 16.0f;

        public static readonly GUIContent k_LoaderName = new GUIContent("OpenXR");
        public static readonly GUIContent k_OpenXRHelp = new GUIContent("You may need to configure additional settings for OpenXR to enable features and interactions for different runtimes.");
        public static readonly GUIContent k_OpenXRHelpIcon = new GUIContent("", CommonContent.k_HelpIcon.image, k_OpenXRHelp.text);
    }


    [XRCustomLoaderUI("UnityEngine.XR.OpenXR.OpenXRLoader", BuildTargetGroup.Standalone)]
    [XRCustomLoaderUI("UnityEngine.XR.OpenXR.OpenXRLoader", BuildTargetGroup.Android)]
    [XRCustomLoaderUI("UnityEngine.XR.OpenXR.OpenXRLoader", BuildTargetGroup.WSA)]
    internal class OpenXRLoaderUI : IXRCustomLoaderUI
    {

        protected bool shouldApplyFeatureSetChanges = false;

        protected List<OpenXRFeatureSetManager.FeatureSetInfo> featureSets { get; set; }
        protected float renderLineHeight = 0;

        private List<OpenXRFeature.ValidationRule> _validationRules = new List<OpenXRFeature.ValidationRule>();

        /// <inheritdoc/>
        public bool IsLoaderEnabled { get; set; }

        public string[] IncompatibleLoaders => new string[] {
            "UnityEngine.XR.WindowsMR.WindowsMRLoader",
            "Unity.XR.Oculus.OculusLoader",
            };

        /// <inheritdoc/>
        public float RequiredRenderHeight { get; protected set; }

        /// <inheritdoc/>
        public virtual void SetRenderedLineHeight(float height)
        {
            renderLineHeight = height;
            RequiredRenderHeight = height;

            if (IsLoaderEnabled && featureSets != null)
            {
                RequiredRenderHeight += featureSets.Count * height;
            }
        }

        BuildTargetGroup activeBuildTargetGroup;
        /// <inheritdoc/>
        public BuildTargetGroup ActiveBuildTargetGroup
        {
            get => activeBuildTargetGroup;
            set
            {
                if (value != activeBuildTargetGroup)
                {
                    activeBuildTargetGroup = value;
                    this.featureSets = OpenXRFeatureSetManager.FeatureSetInfosForBuildTarget(activeBuildTargetGroup);
                    foreach (var featureSet in this.featureSets)
                    {
                        featureSet.isEnabled = OpenXREditorSettings.Instance.IsFeatureSetSelected(activeBuildTargetGroup, featureSet.featureSetId);
                    }
                }
            }
        }

        protected Rect CalculateRectForContent(float xMin, float yMin, GUIStyle style, GUIContent content)
        {
            var size = style.CalcSize(content);
            var rect = new Rect();
            rect.xMin = xMin;
            rect.yMin = yMin;
            rect.width = size.x;
            rect.height = renderLineHeight;
            return rect;
        }

        void RenderFeatureSet(ref OpenXRFeatureSetManager.FeatureSetInfo featureSet, Rect rect)
        {
            float xMin = rect.xMin;
            float yMin = rect.yMin;

            var labelRect = CalculateRectForContent(xMin, yMin, EditorStyles.toggle, featureSet.uiLongName);
            labelRect.width += renderLineHeight;

            EditorGUI.BeginDisabledGroup(!featureSet.isInstalled);
            var newToggled = EditorGUI.ToggleLeft(labelRect, featureSet.uiLongName, featureSet.isEnabled);
            if (newToggled != featureSet.isEnabled)
            {
                featureSet.isEnabled = newToggled;
                shouldApplyFeatureSetChanges = true;
            }

            EditorGUI.EndDisabledGroup();
            xMin = labelRect.xMax + 1;

            if (featureSet.helpIcon != null)
            {
                var iconRect = CalculateRectForContent(xMin, yMin, EditorStyles.label, CommonContent.k_HelpIcon);

                if (GUI.Button(iconRect, featureSet.helpIcon, EditorStyles.label))
                {
                    if (!String.IsNullOrEmpty(featureSet.downloadLink)) UnityEngine.Application.OpenURL(featureSet.downloadLink);
                }
                xMin = iconRect.xMax + 1;
            }
        }

        /// <inheritdoc/>
        public virtual void OnGUI(Rect rect)
        {
            // If this editor is rendering then the make sure the project validator is showing
            // issues for the same build target.
            OpenXRProjectValidationRulesSetup.SetSelectedBuildTargetGroup(activeBuildTargetGroup);

            Vector2 oldIconSize = EditorGUIUtility.GetIconSize();
            EditorGUIUtility.SetIconSize(new Vector2(Content.k_IconSize, Content.k_IconSize));
            shouldApplyFeatureSetChanges = false;

            float xMin = rect.xMin;
            float yMin = rect.yMin;

            var labelRect = CalculateRectForContent(xMin, yMin, EditorStyles.toggle, Content.k_LoaderName);
            var newToggled = EditorGUI.ToggleLeft(labelRect, Content.k_LoaderName, IsLoaderEnabled);
            if (newToggled != IsLoaderEnabled)
            {
                IsLoaderEnabled = newToggled;
            }

            xMin = labelRect.xMax + 1.0f;

            if (IsLoaderEnabled)
            {
                var iconRect = CalculateRectForContent(xMin, yMin, EditorStyles.label, Content.k_OpenXRHelpIcon);
                EditorGUI.LabelField(iconRect, Content.k_OpenXRHelpIcon);
                xMin += Content.k_IconSize + 1.0f;

                OpenXRProjectValidation.GetCurrentValidationIssues(_validationRules, activeBuildTargetGroup);
                if (_validationRules.Count > 0)
                {
                    bool anyErrors = _validationRules.Any(rule => rule.error);
                    GUIContent icon = anyErrors ? CommonContent.k_ValidationErrorIcon : CommonContent.k_ValidationWarningIcon;
                    iconRect = CalculateRectForContent(xMin, yMin, EditorStyles.label, icon);

                    if (GUI.Button(iconRect, icon, EditorStyles.label))
                    {
                        OpenXRProjectValidationRulesSetup.ShowWindow(activeBuildTargetGroup);
                    }
                }
            }


            xMin = rect.xMin;
            yMin += renderLineHeight;
            Rect featureSetRect = new Rect(xMin, yMin, rect.width, renderLineHeight);

            if (featureSets != null && featureSets.Count > 0 && IsLoaderEnabled)
            {
                EditorGUI.indentLevel++;
                for (int i = 0; i < featureSets.Count; i++)
                {
                    var featureSet = featureSets[i];
                    RenderFeatureSet(ref featureSet, featureSetRect);
                    featureSets[i] = featureSet;
                    yMin += renderLineHeight;
                    featureSetRect.yMin = yMin;
                    featureSetRect.height = renderLineHeight;
                }
                EditorGUI.indentLevel--;
            }

            if (shouldApplyFeatureSetChanges)
            {
                OpenXRFeatureSetManager.SetFeaturesFromEnabledFeatureSets(ActiveBuildTargetGroup);
                shouldApplyFeatureSetChanges = false;
            }

            EditorGUIUtility.SetIconSize(oldIconSize);

        }
    }
}
