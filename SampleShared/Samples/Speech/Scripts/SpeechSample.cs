// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Windows.Speech;

namespace Microsoft.MixedReality.OpenXR.BasicSample
{
    /// <summary>
    /// A sample which uses the KeywordRecognizer to detect spoken colors and re-color a material accordingly.
    /// </summary>
    public class SpeechSample : MonoBehaviour
    {
        /// <summary>
        /// The colors which can be recognized by the KeywordRecognizer and applied to the material of scene objects.
        /// </summary>
        private Dictionary<string, Color> m_colors = new Dictionary<string, Color>()
        {
            { "red", Color.red } ,
            { "orange", new Color(1, 0.65f, 0) },
            { "yellow", Color.yellow },
            { "green", Color.green },
            { "blue", Color.blue },
            { "purple", Color.magenta }
        };

        /// <summary>
        /// The minimum confidence level to be used by the KeywordRecognizer.
        /// </summary>
        [SerializeField, Tooltip("The minimum confidence level to be used by the KeywordRecognizer.")]
        private ConfidenceLevel m_confidenceLevel = ConfidenceLevel.Medium;

        /// <summary>
        /// The material whose color should be changed when a new color is spoken.
        /// </summary>
        [SerializeField, Tooltip("The material whose color should be changed when a new color is spoken.")]
        private Material m_material;

        private KeywordRecognizer m_recognizer = null;
        private Color m_originalColor;

        private void Start()
        {
            m_originalColor = m_material.color;
            m_recognizer = new KeywordRecognizer(m_colors.Keys.ToArray(), m_confidenceLevel);
            m_recognizer.OnPhraseRecognized += PhraseRecognized;
            m_recognizer.Start();
        }

        private void PhraseRecognized(PhraseRecognizedEventArgs args)
        {
            m_material.color = m_colors[args.text];
        }

        private void OnDestroy()
        {
            if (m_recognizer != null)
            {
                m_recognizer.Stop();
                m_recognizer.OnPhraseRecognized -= PhraseRecognized;
                m_recognizer.Dispose();
                m_recognizer = null;
            }
            m_material.color = m_originalColor;
        }
    }
}
