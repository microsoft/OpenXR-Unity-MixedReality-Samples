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
            StartCoroutine(TestAnchorCreationAndDestroyUsingSubsystem());
        }

        IEnumerator TestAnchorCreationAndDestroyUsingSubsystem()
        {
            yield return new WaitForSeconds(2); // wait for some time for frame rendering to settle

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
            Pose anchorPose = new Pose(new Vector3(0.1f, 0.2f, 0.3f), new Quaternion(0.5f, 0.5f, 0.5f, 0.5f));

            Test.PrintTestMessage("Creating an new anchor ...");
            XRAnchor addedAnchor = default;

            bool boolResult = anchorManager.subsystem?.TryAddAnchor(anchorPose, out addedAnchor) ?? false;
            Test.VerifyTrue(boolResult, "Creating anchor should succeed when head tracking is working.");
            Test.VerifyTrue(addedAnchor != default && addedAnchor.trackableId != TrackableId.invalidId, "Newly created anchor should be valid.");

            //// NOTE: change expectation to validate the newly created anchor pose because
            //// using XRAnchorSubsystem.TryAddAnchor() is adding parent GameObject's transform
            //// But when using ARAnchorManager.AddAnchor() the pose is in the world.
            //anchorPose = gameObject.transform.TransformPose(anchorPose);
            //Test.PrintTestMessage($"Adjusted expected anchor pose {anchorPose}.");

            yield return anchorChangedValidator.ValidateAnchorCreation(addedAnchor.trackableId, anchorPose, wasCreatedImmediately: false);

            boolResult = anchorManager.trackables.TryGetTrackable(addedAnchor.trackableId, out ARAnchor arAnchor);
            Test.VerifyTrue(boolResult, "New anchor should have been added to ARAnchorManager.trackables collection");
            Test.VerifyTrue(arAnchor != null && arAnchor.nativePtr == addedAnchor.nativePtr, "NativePtr should be the same cross ARAnchor and XRAnchor.");

            //Test.PrintTestMessage("Converting anchor to OpenXR handle ...");
            //ulong openxrAnchorHandle = AnchorConverter.ToOpenXRHandle(arAnchor != null ? arAnchor.nativePtr : IntPtr.Zero);
            //Test.VerifyTrue(openxrAnchorHandle != 0, "Added anchor should contain a valid OpenXR handle.");

            //TrackableId removedId = addedAnchor.trackableId;
            //yield return new WaitForSeconds(1);

            //anchorManager.subsystem.TryRemoveAnchor(removedId);
            //yield return anchorChangedValidator.ValidateAnchorRemove(removedId);

            Test.PrintTestMessage("Tests completed successfully.");
        }
    }
}
