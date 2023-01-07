// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using UnityEngine;

namespace Microsoft.MixedReality.OpenXR
{
    [AttributeUsage(AttributeTargets.Field)]
    internal class DocURLAttribute : PropertyAttribute
    {
        public string Url { get; }

        public DocURLAttribute(string url)
        {
            Url = url;
        }
    }
}
