using UnityEngine;
using Microsoft.MixedReality.OpenXR.Sample;

namespace Microsoft.MixedReality.OpenXR.ASASample
{
    /// <summary>
    /// This component is used in the PersistableSpatialAnchor prefab variant to convert visuals
    /// on the PersistableAnchor prefab for better use with Azure Spatial Anchors.
    /// </summary>
    [RequireComponent(typeof(PersistableAnchorVisuals))]
    public class AnchorVisualsAdapter : MonoBehaviour
    {
        void Start()
        {
            GetComponent<PersistableAnchorVisuals>().ManageOwnTrackingState = true;
            GetComponent<PersistableAnchorVisuals>().AnchorTextFormatter = SpatialAnchorTextFormatter;
        }

        string SpatialAnchorTextFormatter(PersistableAnchorVisuals visuals)
        {
            if (visuals.Persisted)
            {
                return $"Identifier: {visuals.Name}\nTracking State: {visuals.TrackingState}";
            }
            else
            {
                return "Air tap this anchor to save it to the cloud";
            }
        }
    }
}
