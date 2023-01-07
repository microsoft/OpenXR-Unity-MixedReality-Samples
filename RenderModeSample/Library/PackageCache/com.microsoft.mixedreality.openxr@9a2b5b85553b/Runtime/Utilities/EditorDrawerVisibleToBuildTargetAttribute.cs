// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;

namespace Microsoft.MixedReality.OpenXR
{
    [AttributeUsage(AttributeTargets.Field)]
    internal class EditorDrawerVisibleToBuildTargetAttribute : PropertyAttribute
    {
        public BuildTargetGroup[] BuildTargetGroups { get; }

        public EditorDrawerVisibleToBuildTargetAttribute(params BuildTargetGroup[] buildTargetGroups)
        {
            BuildTargetGroups = buildTargetGroups;
        }
    }
}

#endif
