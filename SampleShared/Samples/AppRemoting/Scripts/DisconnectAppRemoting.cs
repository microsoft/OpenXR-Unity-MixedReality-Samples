using UnityEngine;
using Microsoft.MixedReality.OpenXR.Remoting;

namespace Microsoft.MixedReality.OpenXR.BasicSample
{
    // Helper script for hooking MRTK/Unity events to the AppRemoting Disconnect method.
    public class DisconnectAppRemoting : MonoBehaviour
    {
        public void Disconnect() => Remoting.AppRemoting.Disconnect();
    }
}
