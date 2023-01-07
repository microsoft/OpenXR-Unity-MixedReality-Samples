using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.XR.CoreUtils
{
    /// <summary>
    /// Use this interface if you have a component that contains many instances of something you want discoverable
    /// by the cached component filter.  Make sure the THostType matches up with the TFilterType in the CachedComponent filter
    /// </summary>
    /// <typeparam name="THostType">The type of object the host component contains.</typeparam>
    public interface IComponentHost<THostType> where THostType : class
    {
        /// <summary>
        /// The list of hosted components
        /// </summary>
        THostType[] HostedComponents { get; }
    }

    /// <summary>
    /// Describes where the initial list of components should be built from
    /// </summary>
    [Flags]
    public enum CachedSearchType
    {
        /// <summary>
        /// Search in children
        /// </summary>
        Children = 1,

        /// <summary>
        /// Search on self
        /// </summary>
        Self = 2,

        /// <summary>
        /// Search in parents
        /// </summary>
        Parents = 4
    }

    /// <summary>
    /// Class that allows for cached retrieval/filtering of multiple types of components into lists
    /// Proper usage of this class is:
    /// <code>
    /// using (var componentFilter = new CachedComponentFilter&lt;typeToFind,componentTypeThatContains&gt;(instanceOfComponent))
    /// {
    ///
    /// }
    /// </code>
    /// </summary>
    /// <typeparam name="TFilterType">The type of component to find</typeparam>
    /// <typeparam name="TRootType">The type of component at the root of the hierarchy</typeparam>
    public class CachedComponentFilter<TFilterType, TRootType> : IDisposable where TRootType : Component where TFilterType : class
    {
        readonly List<TFilterType> m_MasterComponentStorage;

        // Local method use only -- created here to reduce garbage collection. Collections must be cleared before use
        static readonly List<TFilterType> k_TempComponentList = new List<TFilterType>();
        static readonly List<IComponentHost<TFilterType>> k_TempHostComponentList = new List<IComponentHost<TFilterType>>();

        bool m_DisposedValue; // To detect redundant calls

        /// <summary>
        /// Initialize a new cached component filter
        /// </summary>
        /// <param name="componentRoot">The component at the root of the hierarchy</param>
        /// <param name="cachedSearchType">What type of hierarchy traversal to perform</param>
        /// <param name="includeDisabled">Whether to include components on disabled objects</param>
        public CachedComponentFilter(TRootType componentRoot, CachedSearchType cachedSearchType = CachedSearchType.Self | CachedSearchType.Children, bool includeDisabled = true)
        {
            m_MasterComponentStorage = CollectionPool<List<TFilterType>, TFilterType>.GetCollection();

            k_TempComponentList.Clear();
            k_TempHostComponentList.Clear();

            // Components on the root get added first
            if ((cachedSearchType & CachedSearchType.Self) == CachedSearchType.Self)
            {
                componentRoot.GetComponents(k_TempComponentList);
                componentRoot.GetComponents(k_TempHostComponentList);
                FilteredCopyToMaster(includeDisabled);
            }

            // Then parents, until/unless we hit an end cap node
            if ((cachedSearchType & CachedSearchType.Parents) == CachedSearchType.Parents)
            {
                var searchRoot = componentRoot.transform.parent;
                while (searchRoot != null)
                {
                    if (searchRoot.GetComponent<TRootType>() != null)
                        break;

                    searchRoot.GetComponents(k_TempComponentList);
                    searchRoot.GetComponents(k_TempHostComponentList);
                    FilteredCopyToMaster(includeDisabled);

                    searchRoot = searchRoot.transform.parent;
                }
            }

            // Then children, until/unless we hit an end cap node
            if ((cachedSearchType & CachedSearchType.Children) == CachedSearchType.Children)
            {
                // It's not as graceful going down the hierarchy, so we just use the built-in functions and filter afterwards
                foreach (Transform child in componentRoot.transform)
                {
                    child.GetComponentsInChildren(k_TempComponentList);
                    child.GetComponentsInChildren(k_TempHostComponentList);
                    FilteredCopyToMaster(includeDisabled, componentRoot);
                }
            }
        }

        /// <summary>
        /// Initialize a new cached component filter
        /// </summary>
        /// <param name="componentList">The array of objects to use</param>
        /// <param name="includeDisabled">Whether to include components on disabled objects</param>
        public CachedComponentFilter(TFilterType[] componentList, bool includeDisabled = true)
        {
            if (componentList == null)
                return;

            m_MasterComponentStorage = CollectionPool<List<TFilterType>, TFilterType>.GetCollection();

            k_TempComponentList.Clear();
            k_TempComponentList.AddRange(componentList);
            FilteredCopyToMaster(includeDisabled);
        }

        /// <summary>
        /// Store components which match TChildType
        /// </summary>
        /// <param name="outputList">The list to which matching components will be added</param>
        /// <typeparam name="TChildType">The type for which to search. Must inherit from or be TFilterType</typeparam>
        public void StoreMatchingComponents<TChildType>(List<TChildType> outputList) where TChildType : class, TFilterType
        {
            foreach (var currentComponent in m_MasterComponentStorage)
            {
                if (currentComponent is TChildType asChildType)
                    outputList.Add(asChildType);
            }
        }

        /// <summary>
        /// Get an array of matching components
        /// </summary>
        /// <typeparam name="TChildType">The type for which to search. Must inherit from or be TFilterType</typeparam>
        /// <returns>The array of matching components</returns>
        public TChildType[] GetMatchingComponents<TChildType>() where TChildType : class, TFilterType
        {
            var componentCount = 0;
            foreach (var currentComponent in m_MasterComponentStorage)
            {
                if (currentComponent is TChildType)
                    componentCount++;
            }

            var outputArray = new TChildType[componentCount];
            componentCount = 0;
            foreach (var currentComponent in m_MasterComponentStorage)
            {
                var asChildType = currentComponent as TChildType;
                if (asChildType == null)
                    continue;

                outputArray[componentCount] = asChildType;
                componentCount++;
            }

            return outputArray;
        }

        void FilteredCopyToMaster(bool includeDisabled)
        {
            if (includeDisabled)
            {
                m_MasterComponentStorage.AddRange(k_TempComponentList);
                foreach (var currentEntry in k_TempHostComponentList)
                {
                    m_MasterComponentStorage.AddRange(currentEntry.HostedComponents);
                }
            }
            else
            {
                foreach (var currentEntry in k_TempComponentList)
                {
                    var currentBehaviour = currentEntry as Behaviour;
                    if (currentBehaviour != null && !currentBehaviour.enabled)
                        continue;

                    m_MasterComponentStorage.Add(currentEntry);
                }

                foreach (var currentEntry in k_TempHostComponentList)
                {
                    var currentBehaviour = currentEntry as Behaviour;
                    if (currentBehaviour != null && !currentBehaviour.enabled)
                        continue;

                    m_MasterComponentStorage.AddRange(currentEntry.HostedComponents);
                }
            }
        }

        void FilteredCopyToMaster(bool includeDisabled, TRootType requiredRoot)
        {
            // Here, we want every entry that isn't on the same GameObject as the required root
            // Additionally, any GameObjects that are between this object and the root (children of the root, parent of a component)
            // cannot have a component of the root type, or it is part of a different collection of objects and should be skipped
            if (includeDisabled)
            {
                foreach (var currentEntry in k_TempComponentList)
                {
                    var currentComponent = currentEntry as Component;

                    if (currentComponent.transform == requiredRoot)
                        continue;

                    if (currentComponent.GetComponentInParent<TRootType>() != requiredRoot)
                        continue;

                    m_MasterComponentStorage.Add(currentEntry);
                }

                foreach (var currentEntry in k_TempHostComponentList)
                {
                    var currentComponent = currentEntry as Component;

                    if (currentComponent.transform == requiredRoot)
                        continue;

                    if (currentComponent.GetComponentInParent<TRootType>() != requiredRoot)
                        continue;

                    m_MasterComponentStorage.AddRange(currentEntry.HostedComponents);
                }
            }
            else
            {
                foreach (var currentEntry in k_TempComponentList)
                {
                    var currentBehaviour = currentEntry as Behaviour;

                    if (!currentBehaviour.enabled)
                        continue;

                    if (currentBehaviour.transform == requiredRoot)
                        continue;

                    if (currentBehaviour.GetComponentInParent<TRootType>() != requiredRoot)
                        continue;

                    m_MasterComponentStorage.Add(currentEntry);
                }

                foreach (var currentEntry in k_TempHostComponentList)
                {
                    var currentBehaviour = currentEntry as Behaviour;

                    if (!currentBehaviour.enabled)
                        continue;

                    if (currentBehaviour.transform == requiredRoot)
                        continue;

                    if (currentBehaviour.GetComponentInParent<TRootType>() != requiredRoot)
                        continue;

                    m_MasterComponentStorage.AddRange(currentEntry.HostedComponents);
                }
            }
        }

        /// <summary>
        /// Dispose of the cached component filter
        /// </summary>
        /// <param name="disposing">Whether or not to dispose the contents of this object</param>
        protected virtual void Dispose(bool disposing)
        {
            if (m_DisposedValue)
                return;

            if (disposing && m_MasterComponentStorage != null)
                CollectionPool<List<TFilterType>, TFilterType>.RecycleCollection(m_MasterComponentStorage);

            m_DisposedValue = true;
        }

        /// <summary>
        /// Part of the IDisposable pattern
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
    }
}
