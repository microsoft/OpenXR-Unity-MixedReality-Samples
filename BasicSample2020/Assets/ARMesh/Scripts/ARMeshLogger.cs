// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.MixedReality.OpenXR.Sample;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace Microsoft.MixedReality.OpenXR.BasicSample
{
    public class ARMeshLogger : MonoBehaviour, ITextProvider
    {
        [SerializeField]
        private ARMeshManager meshManager;

        private int meshAddedCount;
        private int meshUpdatedCount;
        private int meshRemovedCount;

        private bool meshesChanged = true;
        private string meshInfoString = string.Empty;

        private void OnEnable()
        {
            if (meshManager != null)
            {
                meshManager.meshesChanged += MeshManager_meshesChanged;
            }
        }

        private void OnDisable()
        {
            if (meshManager != null)
            {
                meshManager.meshesChanged -= MeshManager_meshesChanged;
            }
        }

        private void MeshManager_meshesChanged(ARMeshesChangedEventArgs obj)
        {
            meshesChanged = true;
            meshAddedCount += obj.added.Count;
            meshUpdatedCount += obj.updated.Count;
            meshRemovedCount += obj.removed.Count;
        }

        string ITextProvider.UpdateText()
        {
            if (meshesChanged)
            {
                meshInfoString = $"Meshes added: {meshAddedCount}\nMeshes updated: {meshUpdatedCount}\nMeshes removed:{meshRemovedCount}";
                meshesChanged = false;
            }
            return meshInfoString;
        }
    }
}
