// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using UnityEngine;

namespace Microsoft.MixedReality.OpenXR.Sample
{
    public class GrabAndThrowSampleInstructions : MonoBehaviour, ITextProvider
    {
        private string m_text;

        private void Start()
        {
            m_text = @"Sample: Grab and Throw

- Air tap / press the primary button while touching
an object to grab it, then release to throw it.";
        }

        string ITextProvider.UpdateText()
        {
            return m_text;
        }
    }
}
