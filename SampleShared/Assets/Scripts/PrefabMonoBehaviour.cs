// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using UnityEditor;
using UnityEngine;

namespace Microsoft.MixedReality.OpenXR.Sample
{
    public abstract class PrefabMonoBehaviour : MonoBehaviour
    {
        private bool m_childrenDirty = false;

        #region MonoBehaviour
        protected virtual void Start()
        {
            InitializeComponents();
            UpdateChildren();
        }

        protected virtual void Update()
        {
            UpdateChildrenWhenDirty();
        }
        #endregion

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if (!PrefabUtility.IsPartOfImmutablePrefab(this))
            {
                InitializeComponents();
                UpdateChildren();
            }
        }
#endif

        protected abstract void InitializeComponents();
        protected abstract void UpdateChildren();

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

        protected void UpdateChildrenWhenDirty()
        {
            if (m_childrenDirty)
            {
                m_childrenDirty = false;
                UpdateChildren();
            }
        }
    }
}