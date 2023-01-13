// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Microsoft.MixedReality.OpenXR.Sample
{
    public interface ITextProvider
    {
        // Update and return the latest text
        string UpdateText();
    }

    public class TextPanel : PrefabMonoBehaviour
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
        private const float m_padding = 10;

        #region MonoBehaviour

        protected override void Update()
        {
            base.Update();

            if (m_textProviders != null && m_textProviders.Count > 0)
            {
                bool shouldAddLineBreak = false;
                StringBuilder stringBuilder = new StringBuilder();
                foreach (var textProvider in m_textProviders)
                {
                    if (shouldAddLineBreak)
                    {
                        stringBuilder.AppendLine();
                    }

                    stringBuilder.Append(textProvider.UpdateText());
                    shouldAddLineBreak = true;
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

        #endregion MonoBehaviour

        protected override void InitializeComponents()
        {
            if (m_textMesh == null)
            {
                m_textMesh = gameObject.transform.GetChild(0).GetComponent<TextMesh>();
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

        protected override void UpdateChildren()
        {
            UpdateTextLayout();
            UpdateColors();
        }

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

            const float scaleX = 0.1f;
            const float scaleY = 3.7f;
            // Adjust background panel size;
            {
                var scale = m_background.transform.localScale;
                scale.x = m_textMesh.characterSize * (scaleX * maxLength + m_padding);
                scale.y = m_textMesh.characterSize * (scaleY * m_lines.Length * m_textMesh.lineSpacing + m_padding);
                scale.z = 0.001f;
                m_background.transform.localScale = scale;

                var position = m_background.transform.localPosition;

                // Set horizontal position
                if (m_textMesh.anchor == TextAnchor.UpperLeft ||
                    m_textMesh.anchor == TextAnchor.MiddleLeft ||
                    m_textMesh.anchor == TextAnchor.LowerLeft)
                {
                    position.x = m_textMesh.characterSize * (scaleX * maxLength) / 2;
                }
                else if (m_textMesh.anchor == TextAnchor.UpperRight ||
                         m_textMesh.anchor == TextAnchor.MiddleRight ||
                         m_textMesh.anchor == TextAnchor.LowerRight)
                {
                    position.x = -m_textMesh.characterSize * (scaleX * maxLength) / 2;
                }
                else
                    position.x = 0;

                // Set vertical position
                if (m_textMesh.anchor == TextAnchor.UpperLeft ||
                    m_textMesh.anchor == TextAnchor.UpperCenter ||
                    m_textMesh.anchor == TextAnchor.UpperRight)
                {
                    position.y = -m_textMesh.characterSize * (scaleY * m_lines.Length * m_textMesh.lineSpacing) / 2;
                }
                else if (m_textMesh.anchor == TextAnchor.LowerLeft ||
                         m_textMesh.anchor == TextAnchor.LowerCenter ||
                         m_textMesh.anchor == TextAnchor.LowerRight)
                {
                    position.y = m_textMesh.characterSize * (scaleY * m_lines.Length * m_textMesh.lineSpacing) / 2;
                }
                else
                    position.y = 0;

                m_background.transform.localPosition = position;
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
    }
}
