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

- Use the stereo separation slider to adjust the separation between the eyes.
  
  
  Warning, adjusting separation adjustment may cause user dizzy and hologram appears in different distance.
  The view may appear double images temporarily before user's eyes adjust to the differences.
  Please be careful when doing this and avoid tripping or physical damage.";
        }

        string ITextProvider.UpdateText()
        {
            return m_text;
        }
    }
}
