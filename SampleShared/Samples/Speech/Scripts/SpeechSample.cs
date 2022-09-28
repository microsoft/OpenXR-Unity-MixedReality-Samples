using System.Collections;
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
        public Dictionary<string, Color> m_colors = new Dictionary<string, Color>(){
            { "red", new Color(1, 0, 0) } ,
            { "orange", new Color(1, 0.65f, 0) },
            { "yellow", new Color(1, 1, 0) },
            { "green", new Color(0, 1, 0) },
            { "blue", new Color(0, 0, 1) },
            { "purple", new Color(1, 0, 1) }
        };
           
        /// <summary>
        /// The minimum confidence level to be used by the KeywordRecognizer.
        /// </summary>
        public ConfidenceLevel m_confidenceLevel = ConfidenceLevel.Medium;

        /// <summary>
        /// The material whose color should be changed when a new color is spoken.
        /// </summary>
        public Material m_material;

        private KeywordRecognizer m_recognizer = null;

        private void Start()
        {
            m_recognizer = new KeywordRecognizer(m_colors.Keys.ToArray(), m_confidenceLevel);
            m_recognizer.OnPhraseRecognized += PhraseRecognized;
            m_recognizer.Start();
        }

        private void PhraseRecognized(PhraseRecognizedEventArgs args)
        {
            m_material.color = m_colors[args.text];
        }

        void OnDestroy()
        {
            if (m_recognizer != null)
            {
                m_recognizer.Stop();
                m_recognizer.Dispose();
                m_recognizer = null;
            }
        }
    }
}
