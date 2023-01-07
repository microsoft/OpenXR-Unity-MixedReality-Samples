---
uid: arsubsystems-anchor-subsystem
---
# XR anchor subsystem

The anchor subsystem manages "anchors". Anchors are specific [poses](xref:UnityEngine.Pose) in the physical environment that you want to track. The underlying AR framework attempts to track that specific pose as long as the anchor exists.

Once you create an anchor, it cannot be moved; instead, its pose is updated automatically by the underlying AR framework.

The anchor subsystem is a type of [tracking subsystem](xref:arsubsystems-manual#tracking-subsystems) and follows the same [GetChanges](xref:UnityEngine.XR.ARSubsystems.XRAnchorSubsystem.Provider.GetChanges(UnityEngine.XR.ARSubsystems.XRAnchor,Unity.Collections.Allocator)) pattern to inform the user about changes to the state of anchors. Its trackable is [XRAnchor](xref:UnityEngine.XR.ARSubsystems.XRAnchor).

## Anchor lifecycle

Typically, anchors are created and destroyed programmatically through explicit calls to [TryAddAnchor](xref:UnityEngine.XR.ARSubsystems.XRAnchorSubsystem.Provider.TryAddAnchor(UnityEngine.Pose,UnityEngine.XR.ARSubsystems.XRAnchor@)) and [TryRemoveAnchor](xref:UnityEngine.XR.ARSubsystems.XRAnchorSubsystem.Provider.TryRemoveAnchor(UnityEngine.XR.ARSubsystems.TrackableId)).

On some platforms, anchors can be created automatically in response to loading AR data that contains a previously saved anchor. In this case, the anchor might be created and removed in response to external events not direclty under your control.
