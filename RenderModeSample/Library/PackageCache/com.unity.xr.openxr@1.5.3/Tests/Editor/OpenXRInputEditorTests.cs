using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine.InputSystem;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features.Interactions;
using UnityEngine.XR.OpenXR.Tests;
using Assert = UnityEngine.Assertions.Assert;

namespace UnityEditor.XR.OpenXR.Tests
{
    internal class OpenXRInputEditorTests : OpenXRInputTestsBase
    {
        /// <summary>
        /// Tests whether or not the device layout for an interaction feature is registered/unregistered
        /// when the feature is enabled/disabled
        /// </summary>
        [Test]
        public void DeviceLayoutRegistration([ValueSource(nameof(s_InteractionFeatureLayouts))] (Type featureType, Type layoutType, string layoutNameOverride) interactionFeature)
        {
            var layoutName = interactionFeature.layoutNameOverride ?? interactionFeature.layoutType.Name;

            // Make sure the layout is not registered as it would give the test a false positive
            InputSystem.RemoveLayout(layoutName);
            Assert.IsFalse(IsLayoutRegistered(layoutName), "Layout is still registered, test will give a false positive");

            // Enabling the feature should register the layout
            EnableFeature(interactionFeature.featureType);
            Assert.IsTrue(IsLayoutRegistered(layoutName), "Layout was not registered by enabling the feature");

            // When an interaction feature is disabled its layout should be disable as well
            EnableFeature(interactionFeature.featureType, false);
            Assert.IsFalse(IsLayoutRegistered(layoutName), "Layout was not unregistered by the interaction feature");
        }

        /// <summary>
        /// Tests that interaction features enabled in multiple build targets properly registers and unregisters
        /// the device layout depending on whether the feature is enabled in at least one build target.
        /// </summary>
        [Test]
        public void InteractionFeatureLayoutRegistration()
        {
            var packageSettings = OpenXRSettings.GetPackageSettings();
            Assert.IsNotNull(packageSettings);

            // Ignore the test if there is not more than 1 build target.
            var features = packageSettings.GetFeatures<OculusTouchControllerProfile>().Select(f => f.feature).ToArray();
            if(features.Length < 2)
                return;

            // Disable all of the oculus interaction features
            foreach (var feature in features)
            {
                feature.enabled = false;
            }

            // Make sure the oculus device layout is not registered
            NUnit.Framework.Assert.Throws(typeof(ArgumentException), () => UnityEngine.InputSystem.InputSystem.LoadLayout<OculusTouchControllerProfile.OculusTouchController>());

            // Enable one of the features and make sure the layout is registered
            features[0].enabled = true;
            NUnit.Framework.Assert.DoesNotThrow(() => UnityEngine.InputSystem.InputSystem.LoadLayout<OculusTouchControllerProfile.OculusTouchController>());
            NUnit.Framework.Assert.DoesNotThrow(() => UnityEngine.InputSystem.InputSystem.LoadLayout<OculusTouchControllerProfile.OculusTouchController>());

            // Enable a second feature and make sure the layout is still enabled
            features[1].enabled = true;
            NUnit.Framework.Assert.DoesNotThrow(() => UnityEngine.InputSystem.InputSystem.LoadLayout<OculusTouchControllerProfile.OculusTouchController>());

            // Disable the first feature and make sure the layout is still enabled
            features[0].enabled = false;
            NUnit.Framework.Assert.DoesNotThrow(() => UnityEngine.InputSystem.InputSystem.LoadLayout<OculusTouchControllerProfile.OculusTouchController>());

            // Disable the second feature and make sure the layout is no longer
            features[1].enabled = false;
            NUnit.Framework.Assert.Throws(typeof(ArgumentException), () => UnityEngine.InputSystem.InputSystem.LoadLayout<OculusTouchControllerProfile.OculusTouchController>());
        }
    }
}