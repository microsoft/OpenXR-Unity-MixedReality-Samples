---
uid: arsubsystems-participant-subsystem
---
# XR participant subsystem

This subsystem provides information about other users in a multi-user collaborative session. It is a type of [tracking subsystem](index.md#tracking-subsystems) and follows the same `GetChanges` pattern to inform the user about changes to the state of tracked participants. Its trackable is [`XRParticipant`](xref:UnityEngine.XR.ARSubsystems.XRParticipant).

Participants are detected automatically, like planes or images. You can't create or display participants.
