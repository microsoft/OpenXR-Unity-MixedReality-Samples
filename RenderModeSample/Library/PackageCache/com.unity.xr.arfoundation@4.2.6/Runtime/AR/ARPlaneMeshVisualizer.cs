using System;
using System.Collections.Generic;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.ARFoundation
{
    /// <summary>
    /// Generates a mesh for an <see cref="ARPlane"/>.
    /// </summary>
    /// <remarks>
    /// If this component's [GameObject](xref:UnityEngine.GameObject) has a [MeshFilter](xref:UnityEngine.MeshFilter)
    /// and/or a [MeshCollider](xref:UnityEngine.MeshCollider), this component will generate a mesh from the
    /// [BoundedPlane](xref:UnityEngine.XR.ARSubsystems.BoundedPlane) associated with the <see cref="ARPlane"/>.
    ///
    /// It will also update a [LineRenderer](xref:UnityEngine.LineRenderer) with the boundary points, if present.
    ///
    /// [MeshRenderer](xref:UnityEngine.MeshRenderer) and [LineRenderer](xref:UnityEngine.LineRenderer) components will only be
    /// enabled if:
    /// - This component is enabled.
    /// - The plane's <see cref="ARTrackable{TSessionRelativeData,TTrackable}.trackingState"/> is greater than
    ///   or equal to <see cref="trackingStateVisibilityThreshold"/>.
    /// - The <see cref="ARSession"/>'s <see cref="ARSession.state"/> is greater than <see cref="ARSessionState.Ready"/>.
    /// - <see cref="hideSubsumed"/> is `false` OR <see cref="ARPlane.subsumedBy"/> is not `null`.
    /// </remarks>
    [RequireComponent(typeof(ARPlane))]
    [HelpURL(HelpUrls.ApiWithNamespace + nameof(ARPlaneMeshVisualizer) + ".html")]
    public sealed class ARPlaneMeshVisualizer : MonoBehaviour
    {
        /// <summary>
        /// Get the [Mesh](xref:UnityEngine.Mesh) that this visualizer creates and manages.
        /// </summary>
        public Mesh mesh { get; private set; }

        [SerializeField]
        [Tooltip("A plane whose tracking state is less than this threshold will have its render mesh and line renderer components disabled.")]
        TrackingState m_TrackingStateVisibilityThreshold = TrackingState.Limited;

        /// <summary>
        /// The threshold [TrackingState](xref:UnityEngine.XR.ARSubsystems.TrackingState) that affects the visibility of
        /// the [MeshRenderer](xref:UnityEngine.MeshRenderer) and [LineRenderer](xref:UnityEngine.LineRenderer) components.
        /// </summary>
        /// <remarks>
        /// [MeshRenderer](xref:UnityEngine.MeshRenderer) and [LineRenderer](xref:UnityEngine.LineRenderer) components will only be
        /// enabled if:
        /// - This component is enabled.
        /// - The plane's <see cref="ARTrackable{TSessionRelativeData,TTrackable}.trackingState"/> is greater than
        ///   or equal to <see cref="trackingStateVisibilityThreshold"/>.
        /// - The <see cref="ARSession"/>'s <see cref="ARSession.state"/> is greater than <see cref="ARSessionState.Ready"/>.
        /// - <see cref="hideSubsumed"/> is `false` OR <see cref="ARPlane.subsumedBy"/> is not `null`.
        /// </remarks>
        public TrackingState trackingStateVisibilityThreshold
        {
            get => m_TrackingStateVisibilityThreshold;
            set
            {
                m_TrackingStateVisibilityThreshold = value;
                Debug.Log($"Setting visibility threshold of {GetComponent<ARPlane>().trackableId} to {m_TrackingStateVisibilityThreshold}. Tracking state is {GetComponent<ARPlane>().trackingState}.");
                UpdateVisibility();
            }
        }

        [SerializeField]
        [Tooltip("When enabled, a plane that has been subsumed by (i.e., merged into) another plane will have its render mesh and line renderer disabled.")]
        bool m_HideSubsumed = true;

        /// <summary>
        /// Indicates whether subsumed planes should be rendered. (See <see cref="ARPlane.subsumedBy"/>.)
        /// </summary>
        /// <remarks>
        /// [MeshRenderer](xref:UnityEngine.MeshRenderer) and [LineRenderer](xref:UnityEngine.LineRenderer) components will only be
        /// enabled if:
        /// - This component is enabled.
        /// - The plane's <see cref="ARTrackable{TSessionRelativeData,TTrackable}.trackingState"/> is greater than
        ///   or equal to <see cref="trackingStateVisibilityThreshold"/>.
        /// - The <see cref="ARSession"/>'s <see cref="ARSession.state"/> is greater than <see cref="ARSessionState.Ready"/>.
        /// - <see cref="hideSubsumed"/> is `false` OR <see cref="ARPlane.subsumedBy"/> is not `null`.
        /// </remarks>
        public bool hideSubsumed
        {
            get => m_HideSubsumed;
            set
            {
                m_HideSubsumed = value;
                UpdateVisibility();
            }
        }

        void OnBoundaryChanged(ARPlaneBoundaryChangedEventArgs eventArgs)
        {
            var boundary = m_Plane.boundary;
            if (!ARPlaneMeshGenerators.GenerateMesh(mesh, new Pose(transform.localPosition, transform.localRotation), boundary))
                return;

            var lineRenderer = GetComponent<LineRenderer>();
            if (lineRenderer != null)
            {
                lineRenderer.positionCount = boundary.Length;
                for (int i = 0; i < boundary.Length; ++i)
                {
                    var point2 = boundary[i];
                    lineRenderer.SetPosition(i, new Vector3(point2.x, 0, point2.y));
                }
            }

            var meshFilter = GetComponent<MeshFilter>();
            if (meshFilter != null)
                meshFilter.sharedMesh = mesh;

            var meshCollider = GetComponent<MeshCollider>();
            if (meshCollider != null)
                meshCollider.sharedMesh = mesh;
        }

        void DisableComponents()
        {
            enabled = false;

            var meshCollider = GetComponent<MeshCollider>();
            if (meshCollider != null)
                meshCollider.enabled = false;

            UpdateVisibility();
        }

        void SetRendererEnabled<T>(bool visible) where T : Renderer
        {
            var component = GetComponent<T>();
            if (component)
            {
                component.enabled = visible;
            }
        }

        void SetVisible(bool visible)
        {
            SetRendererEnabled<MeshRenderer>(visible);
            SetRendererEnabled<LineRenderer>(visible);
        }

        void UpdateVisibility()
        {
            var visible = enabled &&
                m_Plane.trackingState >= m_TrackingStateVisibilityThreshold &&
                ARSession.state > ARSessionState.Ready &&
                (!m_HideSubsumed || m_Plane.subsumedBy == null);

            SetVisible(visible);
        }

        void Awake()
        {
            mesh = new Mesh();
            m_Plane = GetComponent<ARPlane>();
        }

        void OnEnable()
        {
            m_Plane.boundaryChanged += OnBoundaryChanged;
            UpdateVisibility();
            OnBoundaryChanged(default(ARPlaneBoundaryChangedEventArgs));
        }

        void OnDisable()
        {
            m_Plane.boundaryChanged -= OnBoundaryChanged;
            UpdateVisibility();
        }

        void Update()
        {
            if (transform.hasChanged)
            {
                var lineRenderer = GetComponent<LineRenderer>();
                if (lineRenderer != null)
                {
                    if (!m_InitialLineWidthMultiplier.HasValue)
                        m_InitialLineWidthMultiplier = lineRenderer.widthMultiplier;

                    lineRenderer.widthMultiplier = m_InitialLineWidthMultiplier.Value * transform.lossyScale.x;
                }
                else
                {
                    m_InitialLineWidthMultiplier = null;
                }

                transform.hasChanged = false;
            }

            if (m_Plane.subsumedBy != null)
            {
                DisableComponents();
            }
            else
            {
                UpdateVisibility();
            }
        }

        float? m_InitialLineWidthMultiplier;

        ARPlane m_Plane;
    }
}
