using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SubsystemsImplementation;
using UnityEngine.XR;

namespace UnityEngine.XR.OpenXR.Samples.MeshingFeature
{
    public class MeshingBehaviour : MonoBehaviour
    {
        public GameObject emptyMeshPrefab;
        public TextMesh textMesh;
        public Transform target;

        private XRMeshSubsystem s_MeshSubsystem;
        private List<MeshInfo> s_MeshInfos = new List<MeshInfo>();

        private Dictionary<MeshId, GameObject> m_MeshIdToGo = new Dictionary<MeshId, GameObject>();

        void Start()
        {
            var feature = OpenXRSettings.Instance.GetFeature<MeshingTeapotFeature>();
            if (null == feature || feature.enabled == false)
            {
                enabled = false;
                return;
            }

            var meshSubsystems = new List<XRMeshSubsystem>();
            SubsystemManager.GetInstances(meshSubsystems);
            if (meshSubsystems.Count == 1)
            {
                s_MeshSubsystem = meshSubsystems[0];
                textMesh.gameObject.SetActive(false);
            }
            else
            {
#if UNITY_EDITOR
                textMesh.text = "Failed to initialize MeshSubsystem.\nTry reloading the Unity Editor";
#else
                textMesh.text = "Failed to initialize MeshSubsystem.";
#endif
                enabled = false;
            }
        }

        void Update()
        {
            if (s_MeshSubsystem.running && s_MeshSubsystem.TryGetMeshInfos(s_MeshInfos))
            {
                foreach (var meshInfo in s_MeshInfos)
                {
                    switch (meshInfo.ChangeState)
                    {
                        case MeshChangeState.Added:
                        case MeshChangeState.Updated:
                            if (!m_MeshIdToGo.TryGetValue(meshInfo.MeshId, out var go))
                            {
                                go = Instantiate(emptyMeshPrefab, target, false);
                                m_MeshIdToGo[meshInfo.MeshId] = go;
                            }

                            var mesh = go.GetComponent<MeshFilter>().mesh;
                            var col = go.GetComponent<MeshCollider>();

                            s_MeshSubsystem.GenerateMeshAsync(meshInfo.MeshId, mesh, col, MeshVertexAttributes.Normals | MeshVertexAttributes.UVs,
                                result =>
                                {
                                    Debug.Log("Mesh generated: " + result.Status);
                                });
                            break;
                        case MeshChangeState.Removed:
                            if (m_MeshIdToGo.TryGetValue(meshInfo.MeshId, out var meshGo))
                            {
                                Destroy(meshGo);
                                m_MeshIdToGo.Remove(meshInfo.MeshId);
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
}
