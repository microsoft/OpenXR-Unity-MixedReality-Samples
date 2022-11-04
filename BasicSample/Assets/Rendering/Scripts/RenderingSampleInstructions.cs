// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using UnityEngine;

namespace Microsoft.MixedReality.OpenXR.Sample
{
    public class RenderingSampleInstructions : MonoBehaviour, ITextProvider
    {
        private string m_text;

        private void Start()
        {
            m_text = @"Sample: Rendering

- Use the stereo separation slider to adjust the separation between the eyes.";
        }

        string ITextProvider.UpdateText()
        {
            return m_text;
        }
    }
}
