// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using UnityEngine;

namespace Microsoft.MixedReality.OpenXR
{
    /// <summary>
    /// Handles articulation of the animatable parts of a controller model.
    /// </summary>
    public class ControllerModelArticulator : MonoBehaviour
    {
        private ControllerModel m_controllerModel = null;
        private ulong m_modelKey = 0;
        private Transform[] m_animationNodes = null;
        private Pose[] m_poses = null;

        private bool IsArticulating => m_controllerModel != null && m_modelKey != 0;

        /// <summary>
        /// Tries to start active articulation of this controller model.
        /// </summary>
        /// <param name="controllerModel">The reference to the controller model this component represents. See <see cref="ControllerModel.Left"/> and <see cref="ControllerModel.Right"/>.</param>
        /// <param name="modelKey">The model key corresponding to the loaded controller model. See <see cref="ControllerModel.TryGetControllerModelKey(out ulong)"/>.</param>
        /// <returns>True if the controller model supports part articulation and articulation was actively started.</returns>
        public bool TryStartArticulating(ControllerModel controllerModel, ulong modelKey)
        {
            if (controllerModel.TryGetControllerModelProperties(modelKey, transform, out m_animationNodes))
            {
                m_controllerModel = controllerModel;
                m_modelKey = modelKey;
                // For updating the node poses in Update. This needs to be the same length as the number of nodes.
                if (m_poses == null || m_poses.Length != m_animationNodes.Length)
                {
                    m_poses = new Pose[m_animationNodes.Length];
                }
            }
            else
            {
                // Disable the built-in, auto-playing glTF animations in the Quest model.
                Animation[] animations = GetComponentsInChildren<Animation>();
                foreach (Animation animation in animations)
                {
                    animation.enabled = false;
                }
            }

            return IsArticulating;
        }

        /// <summary>
        /// Stops any active articulation of this controller model.
        /// </summary>
        public void StopArticulating()
        {
            m_controllerModel = null;
            m_modelKey = 0;
        }

        /// <summary>
        /// The MonoBehaviour Update() callback.
        /// </summary>
        protected void Update()
        {
            if (IsArticulating
                && m_poses != null
                && m_animationNodes != null
                && m_controllerModel.TryGetControllerModelState(m_modelKey, m_poses))
            {
                for (int i = 0; i < m_poses.Length; i++)
                {
                    Transform node = m_animationNodes[i];
                    Pose pose = m_poses[i];

                    node.localPosition = pose.position;
                    node.localRotation = pose.rotation;
                }
            }
        }
    }
}
