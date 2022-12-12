// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using UnityEngine;

namespace Microsoft.MixedReality.OpenXR.Sample
{
    public abstract class PrefabMonoBehaviour : MonoBehaviour
    {
        private bool m_childrenDirty = false;

        protected void SetPropertyValue<T>(ref T member, T value) where T : IEquatable<T>
        {
            if (member.Equals(value))
            {
                member = value;
                m_childrenDirty = true;
            }
        }

        protected void SetPropertyObject<T>(ref T member, T value) where T : class
        {
            if (member == value)
            {
                member = value;
                m_childrenDirty = true;
            }
        }

        protected virtual void InitializeContext() { }
        protected virtual void UpdateChildren() { }

        protected void UpdateChidrenWhenDirty()
        {
            if (m_childrenDirty)
            {
                m_childrenDirty = false;
                UpdateChildren();
            }
        }

        void Start()
        {
            InitializeContext();
            UpdateChildren();
        }

        void Update()
        {
            UpdateChidrenWhenDirty();
        }

#if UNITY_EDITOR
        protected void OnValidate()
        {
            InitializeContext();
            UpdateChildren();
        }
#endif
    }
}