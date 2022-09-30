// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using UnityEngine;

namespace Microsoft.MixedReality.OpenXR.Sample
{
    public class SpeechSampleInstructions : MonoBehaviour, ITextProvider
    {
        private string m_text;

        private void Start()
        {
            m_text = @"Sample: Speech Keyword Recognition

Say 'red', 'green', 'blue', 'orange', 'yellow', or 'purple' to change the color of the objects in this scene.";
        }

        string ITextProvider.UpdateText()
        {
            return m_text;
        }
    }
}
