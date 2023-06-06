// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using UnityEngine;

namespace Microsoft.MixedReality.OpenXR.Sample
{
    public class MarkerSampleInstructions : MonoBehaviour, ITextProvider
    {
        private string m_text;

        private void Start()
        {
            m_text = @"Sample: ARMarker

- Use the 'Change Transform Mode' button to alter the center of the marker between origin and geometric center.
- Use the 'Change Default Transform Mode' button to change the defaul transform mode of newly detected markers.";
        }

        string ITextProvider.UpdateText()
        {
            return m_text;
        }
    }
}
