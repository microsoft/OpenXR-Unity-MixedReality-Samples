using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEngine.XR.Management;

namespace UnityEditor.XR.Management
{
    internal class XRLoaderInfoManager : IXRLoaderOrderManager
    {
        // Simple class to give us updates when the asset database changes.
        internal class AssetCallbacks : AssetPostprocessor
        {
            static bool s_EditorUpdatable = false;
            internal static System.Action Callback { get; set; }

            static AssetCallbacks()
            {
                if (!s_EditorUpdatable)
                {
                    EditorApplication.update += EditorUpdatable;
                }
                EditorApplication.projectChanged += EditorApplicationOnProjectChanged;
            }

            static void EditorApplicationOnProjectChanged()
            {
                if (Callback != null)
                    Callback.Invoke();
            }

            static void EditorUpdatable()
            {
                s_EditorUpdatable = true;
                EditorApplication.update -= EditorUpdatable;
                if (Callback != null)
                    Callback.Invoke();
            }
        }

        private SerializedObject m_SerializedObject;
        SerializedProperty m_RequiresSettingsUpdate = null;
        SerializedProperty m_LoaderList = null;

        public SerializedObject SerializedObjectData
        {
            get { return m_SerializedObject; }
            set
            {
                if (m_SerializedObject != value)
                {
                    m_SerializedObject = value;
                    PopulateProperty("m_RequiresSettingsUpdate", ref m_RequiresSettingsUpdate);
                    PopulateProperty("m_Loaders", ref m_LoaderList);
                    ShouldReload = true;
                }
            }
        }

        List<XRLoaderInfo> m_AllLoaderInfos = new List<XRLoaderInfo>();
        List<XRLoaderInfo> m_AllLoaderInfosForBuildTarget = new List<XRLoaderInfo>();
        List<XRLoaderInfo> m_AssignedLoaderInfos = new List<XRLoaderInfo>();
        List<XRLoaderInfo> m_UnassignedLoaderInfos = new List<XRLoaderInfo>();

        private BuildTargetGroup m_BuildTargetGroup = BuildTargetGroup.Unknown;
        internal BuildTargetGroup BuildTarget
        {
            get { return m_BuildTargetGroup; }
            set
            {
                if (m_BuildTargetGroup != value)
                {
                    m_BuildTargetGroup = value;
                    ShouldReload = true;
                }
            }
        }

        void AssetProcessorCallback()
        {
            ShouldReload = true;
        }

        public void OnEnable()
        {
            AssetCallbacks.Callback += AssetProcessorCallback;
            ShouldReload = true;
        }

        public bool ShouldReload
        {
            get
            {
                if (m_RequiresSettingsUpdate != null)
                {
                    SerializedObjectData.Update();

                    return m_RequiresSettingsUpdate.boolValue;
                }
                return false;
            }
            set
            {
                if (m_RequiresSettingsUpdate != null && m_RequiresSettingsUpdate.boolValue != value)
                {
                    m_RequiresSettingsUpdate.boolValue = value;
                    SerializedObjectData.ApplyModifiedProperties();
                }
            }
        }

        public void OnDisable()
        {
            AssetCallbacks.Callback -= null;
        }

        public void ReloadData()
        {
            if (m_LoaderList == null)
                return;

            PopulateAllLoaderInfos();
            PopulateLoadersForBuildTarget();
            PopulateAssignedLoaderInfos();
            PopulateUnassignedLoaderInfos();

            ShouldReload = false;
        }

        void PopulateAllLoaderInfos()
        {
            m_AllLoaderInfos.Clear();
            XRLoaderInfo.GetAllKnownLoaderInfos(m_AllLoaderInfos);
        }

        void CleanupLostAssignedLoaders()
        {
            var missingLoaders = from info in m_AssignedLoaderInfos
                                 where info.instance == null
                                 select info;

            if (missingLoaders.Any())
            {
                m_AssignedLoaderInfos = m_AssignedLoaderInfos.Except(missingLoaders).ToList();
            }
        }

        void PopulateAssignedLoaderInfos()
        {
            m_AssignedLoaderInfos.Clear();
            for (int i = 0; i < m_LoaderList.arraySize; i++)
            {
                var prop = m_LoaderList.GetArrayElementAtIndex(i);

                XRLoaderInfo info = new XRLoaderInfo();
                info.loaderType = (prop.objectReferenceValue == null) ? null : prop.objectReferenceValue.GetType();
                info.assetName = AssetNameFromInstance(prop.objectReferenceValue);
                info.instance = prop.objectReferenceValue as XRLoader;

                m_AssignedLoaderInfos.Add(info);
            }
            CleanupLostAssignedLoaders();
        }

        string AssetNameFromInstance(UnityEngine.Object asset)
        {
            if (asset == null)
                return "";

            string assetPath = AssetDatabase.GetAssetPath(asset);
            return Path.GetFileNameWithoutExtension(assetPath);
        }

        void PopulateLoadersForBuildTarget()
        {
            m_AllLoaderInfosForBuildTarget = FilteredLoaderInfos(m_AllLoaderInfos);
        }

        void PopulateUnassignedLoaderInfos()
        {
            m_UnassignedLoaderInfos.Clear();
            foreach (var info in m_AllLoaderInfosForBuildTarget)
            {
                var assigned = from ai in m_AssignedLoaderInfos where ai.loaderType == info.loaderType select ai;
                if (!assigned.Any()) m_UnassignedLoaderInfos.Add(info);
            }
        }

        void PopulateProperty(string propertyPath, ref SerializedProperty prop)
        {
            if (SerializedObjectData != null && prop == null) prop = SerializedObjectData.FindProperty(propertyPath);
        }

        private List<XRLoaderInfo> FilteredLoaderInfos(List<XRLoaderInfo> loaderInfos)
        {
            List<XRLoaderInfo> ret = new List<XRLoaderInfo>();

            foreach (var info in loaderInfos)
            {
                if (info.loaderType == null)
                    continue;

                object[] attrs;

                try
                {
                    attrs = info.loaderType.GetCustomAttributes(typeof(XRSupportedBuildTargetAttribute), true);
                }
                catch (Exception)
                {
                    attrs = default;
                }

                if (attrs.Length == 0)
                {
                    // If unmarked we assume it will be applied to all build targets.
                    ret.Add(info);
                }
                else
                {
                    foreach (XRSupportedBuildTargetAttribute attr in attrs)
                    {
                        if (attr.buildTargetGroup == m_BuildTargetGroup)
                        {
                            ret.Add(info);
                            break;
                        }
                    }
                }
            }

            return ret;
        }

        void UpdateSerializedProperty()
        {
            if (m_LoaderList != null && m_LoaderList.isArray)
            {
                m_LoaderList.ClearArray();

                int index = 0;
                foreach (XRLoaderInfo info in m_AssignedLoaderInfos)
                {
                    m_LoaderList.InsertArrayElementAtIndex(index);
                    var prop = m_LoaderList.GetArrayElementAtIndex(index);
                    prop.objectReferenceValue = info.instance;
                    index++;
                }
            }

            SerializedObjectData.ApplyModifiedProperties();
        }

        #region IXRLoaderOrderManager
        List<XRLoaderInfo> IXRLoaderOrderManager.AssignedLoaders { get { return m_AssignedLoaderInfos; } }
        List<XRLoaderInfo> IXRLoaderOrderManager.UnassignedLoaders { get { return m_UnassignedLoaderInfos; } }

        void IXRLoaderOrderManager.AssignLoader(XRLoaderInfo assignedInfo)
        {
            m_AssignedLoaderInfos.Add(assignedInfo);
            m_UnassignedLoaderInfos.Remove(assignedInfo);
            UpdateSerializedProperty();
            ShouldReload = true;
        }

        void IXRLoaderOrderManager.UnassignLoader(XRLoaderInfo unassignedInfo)
        {
            m_AssignedLoaderInfos.Remove(unassignedInfo);
            m_UnassignedLoaderInfos.Add(unassignedInfo);
            UpdateSerializedProperty();
            ShouldReload = true;
        }

        void IXRLoaderOrderManager.Update()
        {
            UpdateSerializedProperty();
            ShouldReload = true;
        }

        #endregion
    }

}
