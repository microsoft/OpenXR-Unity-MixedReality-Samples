// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace Microsoft.MixedReality.OpenXR.BasicSample
{
    public class PrintSubsystemStatuses : MonoBehaviour
    {
        [SerializeField]
        private ARSession m_arSession;

        [SerializeField]
        private ARAnchorManager m_arAnchorManager;

        [SerializeField]
        private ARMeshManager m_arMeshManager;

        [SerializeField]
        private ARPlaneManager m_arPlaneManager;

        [SerializeField]
        private ARRaycastManager m_arRaycastManager;

        public void PrintStatuses()
        {
            Debug.Log($"Session: has subsystem? {m_arSession.subsystem != null}");
            Debug.Log($"Anchor: has subsystem? {m_arAnchorManager.subsystem != null}");
            Debug.Log($"Mesh: has subsystem? {m_arMeshManager.subsystem != null}");
            Debug.Log($"Plane: has subsystem? {m_arPlaneManager.subsystem != null}");
            Debug.Log($"Raycast: has subsystem? {m_arRaycastManager.subsystem != null}");
        }
    }
}