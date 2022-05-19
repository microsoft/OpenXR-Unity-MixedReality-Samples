// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using UnityEngine;

namespace Microsoft.MixedReality.OpenXR.Samples
{
    public abstract class ConfigOption : MonoBehaviour
    {
        [SerializeField]
        private TextMesh tappableText;

        [SerializeField]
        private Tappable tappable;

        protected string Value { get; set; }
        protected abstract string Label { get; }

        private void Start()
        {
            Initialize();
            UpdateVisuals();
        }

        protected abstract void Initialize();
        protected abstract void Select();

        private void OnEnable()
        {
            if (tappableText == null)
            {
                tappableText = GetComponentInChildren<TextMesh>();
            }

            if (tappable == null)
            {
                tappable = GetComponentInChildren<Tappable>();
            }
            tappable.OnAirTapped.AddListener(Select);
        }

        protected void UpdateVisuals() => tappableText.text = $"{Label}:\n{Value}\n\nTap to change";
    }
}
