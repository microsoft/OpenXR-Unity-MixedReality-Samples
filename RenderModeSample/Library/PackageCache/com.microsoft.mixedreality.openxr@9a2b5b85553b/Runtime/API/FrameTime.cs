// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.MixedReality.OpenXR
{
    /// <summary>
    /// Choose the predicted display time of a frame in pipelined rendering.
    /// </summary>
    public enum FrameTime
    {
        /// <summary>
        /// The time in update thread using previous frame's predicted time + duration.
        /// </summary>
        OnUpdate = 0,

        /// <summary>
        /// The time in render thread using current frame's predicted time.
        /// </summary>
        OnBeforeRender
    }
}
