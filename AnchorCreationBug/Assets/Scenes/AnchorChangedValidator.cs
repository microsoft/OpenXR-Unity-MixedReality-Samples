// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace Microsoft.MixedReality.OpenXR.Tests
{
    class AnchorChangedValidator
    {
        private ARAnchorManager anchorManager;
        private List<TrackableId> addedAnchors = new List<TrackableId>();
        private List<TrackableId> updatedAnchors = new List<TrackableId>();
        private List<TrackableId> removedAnchors = new List<TrackableId>();

        public AnchorChangedValidator(ARAnchorManager anchorManager)
        {
            this.anchorManager = anchorManager;
            anchorManager.anchorsChanged += (args) =>
            {
                args.added.ForEach(a => addedAnchors.Add(a.trackableId));
                args.updated.ForEach(a => updatedAnchors.Add(a.trackableId));
                args.removed.ForEach(a => removedAnchors.Add(a.trackableId));
            };
        }

        public void Clear()
        {
            addedAnchors.Clear();
            updatedAnchors.Clear();
            removedAnchors.Clear();
        }

        public IEnumerator ValidateAnchorCreation(
            TrackableId trackableId,
            Pose anchorPose,
            bool wasCreatedImmediately,
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string caller = null)
        {
            Test.PrintTestMessage($"AnchorChangedValidator-ValidateAnchorCreation: {trackableId}", lineNumber, caller);

            // Frame 0:
            // For immediately-created trackables (e.g. AddAnchor) the anchor should exist without tracking, but at the correct location
            // For delayed-creation trackables (e.g. loading persisted anchors) the anchor will exist after the first anchorsChanged event

            bool found = anchorManager.trackables.TryGetTrackable(trackableId, out ARAnchor anchor);
            if (wasCreatedImmediately)
            {
                // PrintTestMessage($"Frame 0: Tracking: {anchor.trackingState}, Pose: {anchor.gameObject.transform.position}, Pending: {anchor.pending}", lineNumber, caller);
                Test.VerifyTrue(found, "The new anchor id should be inside anchor manager collection");
                Test.VerifyTrue(anchor != null && anchor.trackableId == trackableId, "The new ARAnchor component should have been created and well formed.");
                Test.VerifyTrue(anchor != null && anchor.pending, "The new anchor should still be pending before it's reported in the anchorsChanged event");
                Test.VerifyTrue(anchor != null && anchor.gameObject.transform.position == anchorPose.position, 
                    $"The anchor should have the correct position. Expecting {anchorPose.position:#.###}, actually {anchor.gameObject.transform.position:#.###}");
                Test.VerifyTrue(anchor != null && anchor.gameObject.transform.rotation == anchorPose.rotation, 
                    $"The anchor should have the correct rotation. Expecting {anchorPose.rotation:#.###}, actually {anchor.gameObject.transform.rotation:#.###}");
            }

            Test.VerifyTrue(!addedAnchors.Contains(trackableId), "The anchorsChanged event won't happen in the same frame.");
            Clear();   // clear the list to prepare for new events.
            yield return 0;

            // Frame 1:
            // The anchor should be reported as added. It should be tracked with the correct pose.
            if (!wasCreatedImmediately)
            {
                anchorManager.trackables.TryGetTrackable(trackableId, out ARAnchor foundAnchor);
                anchor = foundAnchor;
            }

            // PrintTestMessage($"Frame 1: Tracking: {anchor.trackingState},  Pose: {anchor.gameObject.transform.position}, Pending: {anchor.pending}", lineNumber, caller);
            Test.VerifyTrue(addedAnchors.Contains(trackableId), "The new anchor should trigger anchorsChanged event in the next frame");
            Test.VerifyTrue(anchor != null && anchor.trackingState == TrackingState.Tracking, "The new ARAnchor should be tracking fine at the 1st frame.");
            Test.VerifyTrue(anchor != null && !anchor.pending, "The new anchor should no longer be pending at the next frame");
            Test.VerifyTrue(anchor != null && anchor.gameObject.transform.position == anchorPose.position,
                $"The anchor should have the correct position. Expecting {anchorPose.position:#.###}, actually {anchor.gameObject.transform.position:#.###}");
            Test.VerifyTrue(anchor != null && anchor.gameObject.transform.rotation == anchorPose.rotation,
                $"The anchor should have the correct rotation. Expecting {anchorPose.rotation:#.###}, actually {anchor.gameObject.transform.rotation:#.###}");
            Clear();   // clear the list to prepare for new events.
            yield return 0;

            // Frame 2:
            // The anchor should be reported as updated. It should be tracked with the correct pose.
            // PrintTestMessage($"Frame 2: Tracking: {anchor.trackingState},  Pose: {anchor.gameObject.transform.position}, Pending: {anchor.pending}", lineNumber, caller);
            Test.VerifyTrue(updatedAnchors.Contains(trackableId), "The new anchor should always receive one update in anchorsChanged");
            Test.VerifyTrue(anchor != null && anchor.trackingState == TrackingState.Tracking, "The new ARAnchor should be tracking fine at the 2nd frame.");
            Test.VerifyTrue(anchor != null && !anchor.pending, "The new anchor should no longer be pending at the next frame");
            Test.VerifyTrue(anchor != null && anchor.gameObject.transform.position == anchorPose.position,
                $"The anchor should have the correct position. Expecting {anchorPose.position:#.###}, actually {anchor.gameObject.transform.position:#.###}");
            Test.VerifyTrue(anchor != null && anchor.gameObject.transform.rotation == anchorPose.rotation,
                $"The anchor should have the correct rotation. Expecting {anchorPose.rotation:#.###}, actually {anchor.gameObject.transform.rotation:#.###}");
        }

        public IEnumerator ValidateAnchorRemove(
            TrackableId trackableId,
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string caller = null)
        {
            Test.PrintTestMessage($"AnchorChangedValidator-ValidateAnchorRemove: {trackableId}", lineNumber, caller);

            // Frame 0:
            // The anchor has been removed from the subsystem, but the changes have not yet been reflected in the Unity scene.

            bool found = anchorManager.trackables.TryGetTrackable(trackableId, out ARAnchor anchor);
            Test.VerifyTrue(found, "The anchor is just removed, it should be inside anchor manager collection at this moment");
            Test.VerifyTrue(anchor != null && !anchor.pending, "The anchor is just removed, it shouldn't be in pending state in current frame.");
            Test.VerifyTrue(anchor != null && anchor.trackingState == TrackingState.Tracking, "The anchor is just removed, it shouldn't be lost tracking in current frame.");

            Clear();   // clear the list to prepare for new events.
            yield return 0;

            // Frame 1:
            // The anchor should be reported as removed. Due to constraints in ARFoundation, neither the pose nor the tracking
            // state can be updated for an anchor which was just deleted. The state of the anchor is as it was in the previous update.

            Test.VerifyTrue(removedAnchors.Contains(trackableId), "The remove of the anchor should trigger anchorsChanged event in the next frame");

            found = anchorManager.trackables.TryGetTrackable(trackableId, out _);
            Test.VerifyTrue(!found, "The anchor is removed in last frame, it should not be inside anchor manager collection anymore");
        }
    }

}
