using NUnit.Framework;

namespace UnityEngine.XR.ARFoundation
{
    [TestFixture]
    class AnchorSamples
    {
        // Disable "field never assigned to"
        #pragma warning disable CS0649
        class ExistingContent
        {
            #region anchor_existing_content

            void AnchorContent(Vector3 position, GameObject content)
            {
                // Add an anchor to your content
                content.AddComponent<ARAnchor>();
            }
            #endregion
        }

        class Prefab : MonoBehaviour
        {
            #region anchor_prefab_content

            void AnchorContent(Vector3 position, GameObject prefab)
            {
                // Create an instance of the prefab
                var instance = Instantiate(prefab, position, Quaternion.identity);

                // Add an ARAnchor component if it doesn't have one already.
                if (instance.GetComponent<ARAnchor>() == null)
                {
                    instance.AddComponent<ARAnchor>();
                }
            }
            #endregion
        }
        #pragma warning restore CS0649
    }
}
