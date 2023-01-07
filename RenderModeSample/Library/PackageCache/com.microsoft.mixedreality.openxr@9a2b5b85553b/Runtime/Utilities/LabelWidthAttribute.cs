// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using UnityEngine;

namespace Microsoft.MixedReality.OpenXR
{
    [AttributeUsage(AttributeTargets.Field)]
    internal class LabelWidthAttribute : PropertyAttribute
    {
        public float Width { get; }

        public LabelWidthAttribute(float width)
        {
            Width = width;
        }
    }
}
