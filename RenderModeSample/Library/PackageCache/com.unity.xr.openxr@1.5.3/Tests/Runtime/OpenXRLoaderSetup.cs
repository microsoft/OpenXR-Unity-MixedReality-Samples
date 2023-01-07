using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;
#if UNITY_EDITOR
using UnityEditor.XR.OpenXR.Features;
#endif
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR.Features;
using UnityEngine.XR.TestTooling;
using UnityEngine.XR.OpenXR.Features.Mock;
using UnityEngine.XR.OpenXR.NativeTypes;
using Assert = UnityEngine.Assertions.Assert;

[assembly: InternalsVisibleTo("Unity.XR.OpenXR.Tests.Editor")]
[assembly:UnityPlatform(RuntimePlatform.WindowsPlayer, RuntimePlatform.WindowsEditor, RuntimePlatform.Android)]

namespace UnityEngine.XR.OpenXR.Tests
{
    internal class OpenXRLoaderSetup : LoaderTestSetup<OpenXRLoader, OpenXRSettings>
    {
        protected override string settingsKey => "OpenXRTestSettings";

        private OpenXRFeature[] savedFeatures = null;

        /// <summary>
        /// Helper method to return a feature of the given type
        /// </summary>
        /// <param name="featureType">Feature type</param>
        /// <returns>Reference to the requested feature or null if not found</returns>
        protected OpenXRFeature GetFeature(Type featureType) =>
            OpenXRSettings.ActiveBuildTargetInstance.GetFeature(featureType);

        /// <summary>
        /// Helper method to return a feature of the given type
        /// </summary>
        /// <typeparam name="T">Feature Type</typeparam>
        /// <returns>Reference to the requested feature or null if not found</returns>
        protected T GetFeature<T>() where T : OpenXRFeature => GetFeature(typeof(T)) as T;

        /// <summary>
        /// Enables a required feature of a given type.
        /// </summary>
        /// <param name="featureType">Type of feature to enable</param>
        /// <returns>Feature that was enabled or null</returns>
        protected OpenXRFeature EnableFeature(Type featureType, bool enable=true)
        {
            var feature = GetFeature(featureType);
            Assert.IsNotNull(feature);
            feature.enabled = enable;
            return feature;
        }

        /// <summary>
        /// Enables a required feature of a given type.
        /// </summary>
        /// <typeparam name="T">Type of feature to enable</typeparam>
        /// <returns>Feature that was enabled or null</returns>
        protected T EnableFeature<T>(bool enable=true) where T : OpenXRFeature => EnableFeature(typeof(T), enable) as T;

        protected bool EnableMockRuntime(bool enable = true)
        {
            var feature = MockRuntime.Instance;
            if(null == feature)
                return false;

            if (feature.enabled == enable)
                return true;

            feature.enabled = enable;
            feature.openxrExtensionStrings = MockRuntime.XR_UNITY_null_gfx + " " + MockRuntime.XR_UNITY_android_present;
            feature.priority = 0;
            feature.required = false;
            feature.ignoreValidationErrors = true;

            return true;
        }

        protected void AddExtension(string extensionName)
        {
            MockRuntime.Instance.openxrExtensionStrings += $" {extensionName}";
        }

        private void DisableAllFeatures()
        {
            foreach (var ext in OpenXRSettings.ActiveBuildTargetInstance.features)
            {
                ext.enabled = false;
            }
        }

#pragma warning disable CS0618
        public OpenXRLoader Loader => XRGeneralSettings.Instance?.Manager?.loaders[0] as OpenXRLoader;
#pragma warning restore CS0618


        public override void SetupTest()
        {
            base.SetupTest();

#if UNITY_EDITOR
            UnityEditor.XR.OpenXR.Features.FeatureHelpers.RefreshFeatures(BuildTargetGroup.Standalone);
            UnityEditor.XR.OpenXR.Features.FeatureHelpers.RefreshFeatures(BuildPipeline.GetBuildTargetGroup(UnityEditor.EditorUserBuildSettings.activeBuildTarget));
#endif

            // Enable all build features
            var featureTypes = new List<Type>();
            QueryBuildFeatures(featureTypes);
            featureTypes.Add(typeof(MockRuntime));
            foreach (var feature in featureTypes.Select(featureType => OpenXRSettings.ActiveBuildTargetInstance.GetFeature(featureType)).Where(feature => null != feature))
            {
                feature.enabled = true;
            }
        }

        /// <summary>
        /// Override to return a list of feature types that should be enabled in the build
        /// </summary>
        protected virtual void QueryBuildFeatures(List<Type> featureTypes)
        {
        }

        // NOTE: If you override this function, do NOT add the SetUp test attribute.
        // If you do the overriden function and this function will be called separately
        // and will most likely invalidate your test or even crash Unity.
        [SetUp]
        public virtual void BeforeTest()
        {
            // Make sure we are not running
            if (OpenXRLoaderBase.Instance != null)
                StopAndShutdown();

            // Cache off the features before we start
            savedFeatures = (OpenXRFeature[])OpenXRSettings.ActiveBuildTargetInstance.features.Clone();

            // Disable all features to make sure the feature list is clean before tests start.
            DisableAllFeatures();

            // Enable the mock runtime and reset it back to default state
            Assert.IsTrue(EnableMockRuntime());
            MockRuntime.ResetDefaults();
            OpenXRRuntime.ClearEvents();
            OpenXRRestarter.Instance.ResetCallbacks();

#pragma warning disable CS0618
            loader = XRGeneralSettings.Instance?.Manager?.loaders[0] as OpenXRLoader;
#pragma warning restore CS0618

            #if UNITY_EDITOR && OPENXR_USE_KHRONOS_LOADER
            var features = FeatureHelpersInternal.GetAllFeatureInfo(BuildTargetGroup.Standalone);
            foreach (var f in features.Features)
            {
                if (f.Feature.nameUi == "Mock Runtime")
                {
                    var path = Path.GetFullPath(f.PluginPath + "/unity-mock-runtime.json");
                    Environment.SetEnvironmentVariable("XR_RUNTIME_JSON", path);
                }
            }
            #endif
        }

        [UnityTearDown]
        public IEnumerator TearDown ()
        {
            // It is possible that a test may have done something to initiate the OpenXRRestarter.  To ensure
            // that the restarter does not impact other tests we must make sure it finishes before continuing.
            yield return new WaitForRestarter();

            AfterTest();

            yield return null;
        }

        // NOTE: If you override this function, do NOT add the SetUp test attribute.
        // If you do the overriden function and this function will be called separately
        // and will most likely invalidate your test or even crash Unity.
        public virtual void AfterTest()
        {
#pragma warning disable CS0618
            var curLoader = XRGeneralSettings.Instance?.Manager?.activeLoader;
            // Restore the original loader if it got removed during the test
            if (curLoader == null)
                XRGeneralSettings.Instance?.Manager?.TryAddLoader(loader);
#pragma warning restore CS0618

            OpenXRRestarter.Instance.ResetCallbacks();
            StopAndShutdown();
            EnableMockRuntime(false);
            MockRuntime.Instance.TestCallback = (methodName, param) => true;
            MockRuntime.KeepFunctionCallbacks = false;
            MockRuntime.ClearFunctionCallbacks();

            // Replace the features with the saved features
            OpenXRSettings.ActiveBuildTargetInstance.features = savedFeatures;
        }

        public override void Setup()
        {
            SetupTest();
            EnableMockRuntime();
            base.Setup();
        }

        public override void Cleanup()
        {
            base.Cleanup();
            TearDownTest();
            EnableMockRuntime(false);
        }

        static Dictionary<XrSessionState, HashSet<XrSessionState>> s_AllowedStateTransitions = new Dictionary<XrSessionState, HashSet<XrSessionState>>()
        {
            {XrSessionState.Unknown, new HashSet<XrSessionState>() {XrSessionState.Unknown}},
            {XrSessionState.Idle, new HashSet<XrSessionState>() {XrSessionState.Unknown, XrSessionState.Unknown, XrSessionState.Exiting, XrSessionState.LossPending, XrSessionState.Stopping}},
            {XrSessionState.Ready, new HashSet<XrSessionState>() {XrSessionState.Idle}},
            {XrSessionState.Synchronized, new HashSet<XrSessionState>() {XrSessionState.Ready, XrSessionState.Visible}},
            {XrSessionState.Visible, new HashSet<XrSessionState>() {XrSessionState.Synchronized, XrSessionState.Focused}},
            {XrSessionState.Focused, new HashSet<XrSessionState>() {XrSessionState.Visible}},
            {XrSessionState.Stopping, new HashSet<XrSessionState>() {XrSessionState.Synchronized}},
            {XrSessionState.LossPending, new HashSet<XrSessionState>() {XrSessionState.Unknown, XrSessionState.Idle, XrSessionState.Ready, XrSessionState.Synchronized, XrSessionState.Visible, XrSessionState.Focused, XrSessionState.Stopping, XrSessionState.Exiting, XrSessionState.LossPending}},
            {XrSessionState.Exiting, new HashSet<XrSessionState>() {XrSessionState.Idle}},
        };

        public void CheckValidStateTransition(XrSessionState oldState, XrSessionState newState)
        {
            bool hasNewState = s_AllowedStateTransitions.ContainsKey(newState);
            bool canTransitionTo = s_AllowedStateTransitions[newState].Contains(oldState);

            Debug.LogWarning($"Attempting to transition from {oldState} to {newState}");
            if (!hasNewState)
                Debug.LogError($"Has {newState} : {hasNewState}");

            if (!canTransitionTo)
                Debug.LogError($"Can transition from {oldState} to {newState} : {canTransitionTo}");


            NUnit.Framework.Assert.IsTrue(hasNewState);
            NUnit.Framework.Assert.IsTrue(canTransitionTo);
        }

        /// <summary>
        /// Return true if the diagnostic report contains text that matches the given regex
        /// </summary>
        /// <param name="match">Regex to match</param>
        /// <returns>True if the report matches the regex</returns>
        protected bool DoesDiagnosticReportContain(Regex match) =>
            match.IsMatch(DiagnosticReport.GenerateReport());

        /// <summary>
        /// Return true if the diagnostic report contains the given text
        /// </summary>
        /// <param name="match">String to search for</param>
        /// <returns>True if the report contains the given text</returns>
        protected bool DoesDiagnosticReportContain(string match) =>
            DiagnosticReport.GenerateReport().Contains(match);

        protected void ProcessOpenXRMessageLoop() => loader.ProcessOpenXRMessageLoop();
    }
}
