// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using UnityEngine.XR.OpenXR;

namespace Microsoft.MixedReality.OpenXR
{
    /// <summary>
    /// Represents the xr session state in its lifecycle.
    /// Reference https://www.khronos.org/registry/OpenXR/specs/1.0/html/xrspec.html#session-lifecycle for more details on session state machine in OpenXR.
    /// </summary>
    internal enum XrSessionState : int
    {
        /// <summary>
        /// Indicates an unknown state of session, typically means the session is not created yet.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Indicates that the runtime considers the session is idle.
        /// Applications in this state should minimize resource consumption.
        /// </summary>
        Idle = 1,

        /// <summary>
        /// Indicates that the runtime desires the application to prepare rendering resources, begin its session and synchronize its frame loop with the runtime.
        /// Unity engine will handle the necessary preparation and begin a session.
        /// </summary>
        Ready = 2,

        /// <summary>
        /// Indicates that the application has synchronized its frame loop with the runtime, but its frames are not visible to the user.
        /// </summary>
        Synchronized = 3,

        /// <summary>
        /// Indicates that the application has synchronized its frame loop with the runtime, and the session's frames will be visible to the user,
        /// but the session is not eligible to receive XR input.
        /// </summary>
        Visible = 4,

        /// <summary>
        ///  indicates that the application has synchronized its frame loop with the runtime, the session's frames will be visible to the user,
        ///  and the session is eligible to receive XR input.
        /// </summary>
        Focused = 5,

        /// <summary>
        /// Indicates that the runtime has determined that the application should halt its rendering loop.
        /// Unity engine will handle the stopping of a running session.
        /// </summary>
        Stopping = 6,

        /// <summary>
        /// Indicates the runtime is no longer able to operate with the current session, for example due to the loss of a display hardware connection.
        /// </summary>
        LossPending = 7,

        /// <summary>
        /// Indicates the runtime wishes the application to terminate its XR experience, typically due to a user request via a runtime user interface.
        /// </summary>
        Exiting = 8,
    };
}