// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR.OpenXR;

namespace Microsoft.MixedReality.OpenXR
{
    /// <summary>
    /// Provides access to a byte stream representing a glTF model of the current controller.
    /// </summary>
    public class ControllerModel
    {
        /// <summary>
        /// The user's left controller.
        /// </summary>
        public static ControllerModel Left { get; } = new ControllerModel(Handedness.Left);

        /// <summary>
        /// The user's right controller.
        /// </summary>
        public static ControllerModel Right { get; } = new ControllerModel(Handedness.Right);

        private readonly Handedness m_handedness;

        internal ControllerModel(Handedness trackerHandedness)
        {
            m_handedness = trackerHandedness;
        }

        /// <summary>
        /// Returns true if the necessary OpenXR feature extensions are enabled on the current runtime.
        /// </summary>
        /// <remarks>This value should not be assumed immutable and should be queried on a new XR session.</remarks>
        public static bool IsSupported => m_feature.NativeLibToken != NativeLibToken.Invalid && m_feature.enabled && NativeLib.IsControllerModelSupported(m_feature.NativeLibToken);

        /// <summary>
        /// Provides access to a model-specific key to either load a new model or use to cache loaded models.
        /// </summary>
        /// <param name="modelKey">The unique key representing this controller's model, if one exists.</param>
        /// <returns>True if a valid key could be retrieved. False otherwise.</returns>
        public bool TryGetControllerModelKey(out ulong modelKey)
        {
            if (!IsSupported)
            {
                modelKey = 0;
                return false; // Controller feature is not enabled.
            }

            return NativeLib.TryGetControllerModelKey(m_feature.NativeLibToken, m_handedness, out modelKey);
        }

        /// <summary>
        /// Provides a byte stream representing the glTF model of the controller, if available.
        /// </summary>
        /// <remarks>
        /// Needs to be passed into a glTF parser/loader to convert into a Unity GameObject.
        /// This method allocates a byte buffer on every successful call. It's recommended to either cache it or the resulting GameObject locally instead of calling this multiple times.
        /// </remarks>
        /// <param name="modelKey">The unique key representing the desired controller's model. Can be queried using <see cref="TryGetControllerModelKey"/>.</param>
        /// <returns>Task that triggers once the controller model stream is loaded, yielding the stream or null if there is no model available.</returns>
        public Task<byte[]> TryGetControllerModel(ulong modelKey)
        {
            if (!IsSupported)
            {
                return Task.FromResult<byte[]>(null); // Controller feature is not enabled.
            }

            Task<byte[]> newTask = Task.Run(() =>
            {
                if (NativeLib.TryGetControllerModel(m_feature.NativeLibToken, modelKey, 0, out uint bufferCapacity))
                {
                    byte[] modelBuffer = new byte[bufferCapacity];
                    if (NativeLib.TryGetControllerModel(m_feature.NativeLibToken, modelKey, bufferCapacity, out _, modelBuffer))
                    {
                        return modelBuffer;
                    }
                }
                return null;
            });

            return newTask;
        }

        private uint m_lastNodeStateCount = 0;

        /// <summary>
        /// Represents a set of animatable nodes in the controller model. Use <see cref="TryGetControllerModelState"/> to obtain the current animation values.
        /// </summary>
        /// <param name="modelKey">The unique key representing the desired controller's model. Can be queried using <see cref="ControllerModel.TryGetControllerModelKey"/>.</param>
        /// <param name="modelRoot">The Transform representing the loaded controller model from <see cref="ControllerModel.TryGetControllerModel"/>.</param>
        /// <param name="nodes">A method-allocated array containing the animatable nodes in the current controller model, with the same indices as the Pose array data from <see cref="TryGetControllerModelState"/>.</param>
        /// <remarks>
        /// This method allocates a Transform array on every successful call.
        /// It's recommended to cache it locally instead of calling this multiple times, as this won't change unless the model key changes.
        /// </remarks>
        internal bool TryGetControllerModelProperties(ulong modelKey, Transform modelRoot, out Transform[] nodes)
        {
            if (IsSupported && NativeLib.TryGetControllerModelProperties(m_feature.NativeLibToken, modelKey, 0, out uint nodeCountOutput))
            {
                m_lastNodeStateCount = nodeCountOutput;
                ControllerModelNodeProperties[] properties = new ControllerModelNodeProperties[nodeCountOutput];
                if (NativeLib.TryGetControllerModelProperties(m_feature.NativeLibToken, modelKey, nodeCountOutput, out _, properties))
                {
                    nodes = new Transform[nodeCountOutput];
                    Transform[] children = modelRoot.GetComponentsInChildren<Transform>();
                    int nodesFound = 0;
                    // Iterates through all children of the model root in order to find the
                    // animatable nodes by name plus parent name (if provided).
                    foreach (Transform potentialNode in children)
                    {
                        // If we've found all named nodes, we can return early.
                        if (nodesFound == nodeCountOutput)
                        {
                            break;
                        }

                        for (int i = 0; i < nodeCountOutput; i++)
                        {
                            // Because we iterate through all node names for each node, it's possible that this node has already been found.
                            if (nodes[i] != null)
                            {
                                continue;
                            }

                            ControllerModelNodeProperties property = properties[i];
                            if (potentialNode.name.Equals(property.NodeName, StringComparison.OrdinalIgnoreCase)
                                && (string.IsNullOrWhiteSpace(property.ParentNodeName)
                                || (potentialNode.parent != null && potentialNode.parent.name.Equals(property.ParentNodeName, StringComparison.OrdinalIgnoreCase))))
                            {
                                nodes[i] = potentialNode;
                                nodesFound++;
                                break;
                            }
                        }
                    }

                    // If we didn't find all nodes, log which ones are missing.
                    if (nodesFound != nodeCountOutput)
                    {
                        for (int i = 0; i < nodeCountOutput; i++)
                        {
                            if (nodes[i] == null)
                            {
                                Debug.LogError($"No corresponding node found for node name {properties[i].NodeName} and parent name {properties[i].ParentNodeName}.");
                            }
                        }
                    }
                    return nodesFound == nodeCountOutput;
                }
            }

            nodes = Array.Empty<Transform>();
            return false;
        }

        /// <summary>
        /// Represents the current state of the controller model representing user's interaction to the controller, such as pressing a button or pulling a trigger.
        /// </summary>
        /// <param name="modelKey">The unique key representing the desired controller's model. Can be queried using <see cref="ControllerModel.TryGetControllerModelKey"/>.</param>
        /// <param name="poses"></param>
        /// <exception cref="ArgumentException">The pose array must match the properties array size of the most recent call to <see cref="TryGetControllerModelProperties"/>.</exception>
        internal bool TryGetControllerModelState(ulong modelKey, Pose[] poses)
        {
            if (!IsSupported)
            {
                return false;
            }

            if (poses.Length != m_lastNodeStateCount)
            {
                throw new ArgumentException("The poses array doesn't match the most recent array size from TryGetControllerModelProperties.");
            }

            return NativeLib.TryGetControllerModelState(m_feature.NativeLibToken, modelKey, m_lastNodeStateCount, out _, poses);
        }

        private static readonly MotionControllerFeaturePlugin m_feature = OpenXRSettings.Instance.GetFeature<MotionControllerFeaturePlugin>();

        /// <summary>
        /// Describes properties of animatable nodes, including the node name and parent node name to locate a glTF node in the controller model that can be animated based on user's interactions on the controller.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 8, CharSet = CharSet.Ansi)]
        internal readonly struct ControllerModelNodeProperties
        {
            // Represents the maximum name size defined by the OpenXR spec. Used for string marshaling.
            private const int ControllerModelNodeNameSize = 64;

            /// <summary>
            /// The name of the parent node in the provided glTF file.
            /// </summary>
            /// <remarks>The parent name may be empty if it should not be used to locate this node.</remarks>
            [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = ControllerModelNodeNameSize)]
            public string ParentNodeName { get; }

            /// <summary>
            /// The name of this node in the provided glTF file.
            /// </summary>
            [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = ControllerModelNodeNameSize)]
            public string NodeName { get; }
        }
    }
}
