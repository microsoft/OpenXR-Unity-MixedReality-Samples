// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace Microsoft.MixedReality.OpenXR.Tests
{
    public class AnchorConverterTests : MonoBehaviour
    {
        void Start()
        {
            //StartCoroutine(TestTrackableIdAndGuidConversion());
            //StartCoroutine(TestAnchorRoundTripToOpenXRHandle());
            StartCoroutine(TestAnchorCreationAndDestroy());
        }

        //IEnumerator TestAnchorRoundTripToOpenXRHandle()
        //{
        //    yield return new WaitForSeconds(2); // wait for some time for frame rendering to settle

        //    Test.PrintTestMessage("Waiting for head tracking ...");
        //    yield return new WaitUntil(() => { return Test.GetTrackingState(XRNode.Head); });

        //    Test.PrintTestMessage("Find the ARAnchorManager ...");
        //    var anchorManagers = FindObjectsOfType<ARAnchorManager>();
        //    Test.VerifyTrue(anchorManagers != null && anchorManagers.Length == 1 && anchorManagers[0] != null && anchorManagers[0].enabled,
        //        "The scene should contain exactly one valid and enabled ARAnchorManager.");
        //    var anchorManager = anchorManagers[0];

        //    if (anchorManager.subsystem == null)
        //    {
        //        Debug.LogWarning("Anchor subsystem not supported on this runtime. Tests not running.");
        //        yield break;
        //    }

        //    AnchorChangedValidator anchorChangedValidator = new AnchorChangedValidator(anchorManager);
        //    Pose anchorPose = new Pose(new Vector3(0, 0.1f, 0), new Quaternion(0.5f, 0.5f, 0.5f, 0.5f));

        //    Test.PrintTestMessage("Creating an new anchor ...");
        //    XRAnchor anchor = default;
        //    bool boolResult = anchorManager.subsystem?.TryAddAnchor(anchorPose, out anchor) ?? false;
        //    Test.VerifyTrue(boolResult, "Creating anchor should succeed when head tracking is working.");
        //    Test.VerifyTrue(anchor != default && anchor.trackableId != TrackableId.invalidId, "Newly created anchor should be valid.");

        //    // NOTE: change expectation to validate the newly created anchor pose because
        //    // using XRAnchorSubsystem.TryAddAnchor() is adding parent GameObject's transform
        //    // But when using ARAnchorManager.AddAnchor() the pose is in the world.
        //    anchorPose = gameObject.transform.TransformPose(anchorPose);
        //    Test.PrintTestMessage($"Adjusted expected anchor pose {anchorPose}.");

        //    yield return anchorChangedValidator.ValidateAnchorCreation(anchor.trackableId, anchorPose, wasCreatedImmediately: false);

        //    boolResult = anchorManager.trackables.TryGetTrackable(anchor.trackableId, out ARAnchor arAnchor);
        //    Test.VerifyTrue(boolResult, "New anchor should have been added to ARAnchorManager.trackables collection");
        //    Test.VerifyTrue(arAnchor != null && arAnchor.nativePtr == anchor.nativePtr, "NativePtr should be the same cross ARAnchor and XRAnchor.");

        //    Test.PrintTestMessage("Converting anchor to OpenXR handle ...");
        //    ulong openxrAnchorHandle = AnchorConverter.ToOpenXRHandle(arAnchor != null ? arAnchor.nativePtr : IntPtr.Zero);
        //    Test.VerifyTrue(openxrAnchorHandle != 0, "Added anchor should contain a valid OpenXR handle.");

        //    Test.PrintTestMessage("Creating anchor from OpenXR handle ...");
        //    TrackableId convertedAnchorId = AnchorConverter.CreateFromOpenXRHandle(openxrAnchorHandle);
        //    Test.VerifyTrue(convertedAnchorId != TrackableId.invalidId, "Newly created anchor id should be valid.");

        //    yield return anchorChangedValidator.ValidateAnchorCreation(convertedAnchorId, anchorPose, wasCreatedImmediately: false);

        //    yield return new WaitForSeconds(1);

        //    TrackableId removedId = arAnchor != null ? arAnchor.trackableId : TrackableId.invalidId;
        //    anchorManager.subsystem.TryRemoveAnchor(removedId);
        //    yield return anchorChangedValidator.ValidateAnchorRemove(removedId);

        //    Test.PrintTestMessage("Tests completed successfully.");
        //}

        IEnumerator TestAnchorCreationAndDestroy()
        {
            yield return new WaitForSeconds(6); // wait for some time for frame rendering to settle

            Test.PrintTestMessage("Waiting for head tracking ...");
            yield return new WaitUntil(() => { return Test.GetTrackingState(XRNode.Head); });

            Test.PrintTestMessage("Find the ARAnchorManager ...");
            var anchorManagers = FindObjectsOfType<ARAnchorManager>();
            Test.VerifyTrue(anchorManagers != null && anchorManagers.Length == 1 && anchorManagers[0] != null && anchorManagers[0].enabled,
                "The scene should contain exactly one valid and enabled ARAnchorManager.");
            var anchorManager = anchorManagers[0];

            if (anchorManager.subsystem == null)
            {
                Debug.LogWarning("Anchor subsystem not supported on this runtime. Tests not running.");
                yield break;
            }

            AnchorChangedValidator anchorChangedValidator = new AnchorChangedValidator(anchorManager);
            Pose anchorPose = new Pose(new Vector3(0, 0.1f, 0), new Quaternion(0.5f, 0.5f, 0.5f, 0.5f));

            Test.PrintTestMessage("Creating an new anchor ...");
#pragma warning disable 0618 // deprecated API
            ARAnchor addedAnchor = anchorManager.AddAnchor(anchorPose);
            Test.VerifyTrue(addedAnchor != null && addedAnchor.trackableId != TrackableId.invalidId, "Newly created anchor should be valid.");
#pragma warning restore 0618

            yield return anchorChangedValidator.ValidateAnchorCreation(addedAnchor.trackableId, anchorPose, wasCreatedImmediately: true);

            Test.PrintTestMessage("Converting anchor to OpenXR handle ...");
            object perceptionAnchor = AnchorConverter.ToPerceptionSpatialAnchor(addedAnchor.nativePtr);
            Test.VerifyTrue(perceptionAnchor != null, "Added anchor should contain a valid perceptionAnchor.");

            // Create an anchor from a perception spatial anchor
            TrackableId secondAnchor = AnchorConverter.CreateFromPerceptionSpatialAnchor(perceptionAnchor);
            yield return anchorChangedValidator.ValidateAnchorCreation(secondAnchor, anchorPose, wasCreatedImmediately: false);

            TrackableId removedId = addedAnchor.trackableId;
            yield return new WaitForSeconds(1);
#pragma warning disable 0618 // deprecated API
            anchorManager.RemoveAnchor(addedAnchor);
#pragma warning restore 0618
            yield return anchorChangedValidator.ValidateAnchorRemove(removedId);

            Test.PrintTestMessage("Tests completed successfully.");
        }
    }
}
