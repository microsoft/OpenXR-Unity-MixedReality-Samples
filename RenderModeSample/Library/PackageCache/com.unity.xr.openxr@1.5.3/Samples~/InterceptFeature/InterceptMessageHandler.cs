
namespace UnityEngine.XR.OpenXR.Samples.InterceptFeature
{
    /// <summary>
    /// Ensures that the intercept feature is enabled and updates the message on screen
    /// with the message received from the intercepted xrCreateSession call.
    /// </summary>
    public class InterceptMessageHandler : MonoBehaviour
    {
        public TextMesh textMesh;

        private void Start()
        {
            var feature = OpenXRSettings.Instance.GetFeature<InterceptCreateSessionFeature>();
            if (feature == null || !feature.enabled)
                textMesh.text = "InterceptCreateSession feature not enabled";
            else
            {
                textMesh.text = feature.receivedMessage;
                textMesh.color = Color.white;
            }

        }
    }
}