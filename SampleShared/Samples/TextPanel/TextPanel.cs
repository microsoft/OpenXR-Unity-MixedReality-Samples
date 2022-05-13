using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace Microsoft.MixedReality.OpenXR.Sample
{
    public interface ITextProvider
    {
        // Update and return the latest text
        string UpdateText();
    }

    public class TextPanel : MonoBehaviour
    {
        [SerializeField]
        private float m_fontScale = 1;

        [SerializeField]
        private Color m_foregroundColor = Color.white;

        [SerializeField]
        private Color m_backgroundColor = Color.gray;

        [SerializeField]
        private int m_minimumWidth = 400;

        private TextMesh m_textMesh;
        private GameObject m_background;
        private Renderer m_foregroundRenderer;
        private Renderer m_backgroundRenderer;
        private IList<ITextProvider> m_textProviders;
        private string[] m_lines;
        private const float padding = 10;

        void Start()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            if (m_textMesh == null)
            {
                m_textMesh = gameObject.transform.GetChild(0).gameObject.GetComponent<TextMesh>();
                Debug.Assert(m_textMesh != null);
                m_foregroundRenderer = m_textMesh.GetComponent<Renderer>();
                Debug.Assert(m_foregroundRenderer != null);
                m_background = gameObject.transform.GetChild(1).gameObject;
                Debug.Assert(m_background != null);
                m_backgroundRenderer = m_background.GetComponent<Renderer>();
                Debug.Assert(m_backgroundRenderer != null);
                m_textProviders = gameObject.GetComponents<ITextProvider>();
                Debug.Assert(m_textProviders != null);
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            InitializeComponents();

            UpdateTextLayout();
            UpdateColors();
        }
#endif

        private void UpdateTextLayout()
        {
            // Determine the line width
            m_lines = m_textMesh.text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None); // "\r" will not show as a return in the Unity editor or TextMesh.

            int maxLength = 0;
            if (m_lines != null && m_lines.Length > 0)
            {
                Font font = m_textMesh.font;
                foreach (var line in m_lines)
                {
                    int lineLength = 0;
                    foreach (var chr in line)
                    {
                        CharacterInfo charInfo;
                        font.GetCharacterInfo(chr, out charInfo, m_textMesh.fontSize);
                        lineLength += charInfo.advance;
                    }

                    maxLength = Math.Max(maxLength, lineLength);
                }
            }

            // Ensure the size is at least the minimumWidth
            maxLength = Math.Max(m_minimumWidth, maxLength);

            m_textMesh.characterSize = 0.01f * m_fontScale;

            // Adjust background panel size;
            {
                var scale = m_background.transform.localScale;
                scale.x = m_textMesh.characterSize * (0.1f * maxLength + m_padding);
                scale.y = m_textMesh.characterSize * (3.7f * m_lines.Length * m_textMesh.lineSpacing + m_padding);
                scale.z = 0.001f;
                m_background.transform.localScale = scale;
            }
        }

        private void UpdateColors()
        {
            if (m_textMesh != null && m_textMesh.color != m_foregroundColor)
            {
                m_textMesh.color = m_foregroundColor;
            }

            if (m_backgroundRenderer.sharedMaterial != null &&
                m_backgroundRenderer.sharedMaterial.color != m_backgroundColor)
            {
                m_backgroundRenderer.sharedMaterial.color = m_backgroundColor;
            }
        }

        void Update()
        {
            if (m_textProviders != null && m_textProviders.Count > 0)
            {
                StringBuilder stringBuilder = new StringBuilder();
                foreach (var textProvider in m_textProviders)
                {
                    stringBuilder.Append(textProvider.UpdateText());
                }

                var text = stringBuilder.ToString();
                if (m_textMesh.text != text)
                {
                    m_textMesh.text = text;
                    UpdateTextLayout();
                }
            }

            UpdateColors();
        }
    }
}
