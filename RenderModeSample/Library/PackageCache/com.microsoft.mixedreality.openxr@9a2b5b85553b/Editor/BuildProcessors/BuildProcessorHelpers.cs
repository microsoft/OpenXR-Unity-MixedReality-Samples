// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using UnityEditor;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;

namespace Microsoft.MixedReality.OpenXR.Editor
{
    internal static class BuildProcessorHelpers
    {
        internal static bool IsFeatureEnabled<T>() where T : OpenXRFeature
        {
            T feature = GetOpenXRFeature<T>();
            return feature != null && feature.enabled;
        }

        /// <summary>
        /// Get the feature from the OpenXRSettings for the current build target group.
        /// </summary>
        /// <typeparam name="T">The specific feature to check.</typeparam>
        /// <remarks>
        /// This helper does not filter input T using <![CDATA[OpenXRFeaturePlugin<T>]]> to support
        /// scenarios when T derived from another plug-in instead of OpenXRFeaturePlugin.
        /// For example, <![CDATA[class BaseFeature : OpenXRFeaturePlugin<BaseFeature> {...}]]>.
        /// This function also works with <![CDATA[class DerivedFeature : BaseFeature {...}]]>.
        /// </remarks>
        internal static T GetOpenXRFeature<T>() where T : OpenXRFeature
        {
            return GetOpenXRFeature<T>(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget));
        }

        /// <summary>
        /// Get the feature from the OpenXRSettings for the provided build target group.
        /// </summary>
        /// <typeparam name="T">The specific feature to check.</typeparam>
        /// <remarks>
        /// This helper does not filter input T using <![CDATA[OpenXRFeaturePlugin<T>]]> to support
        /// scenarios when T derived from another plug-in instead of OpenXRFeaturePlugin.
        /// For example, <![CDATA[class BaseFeature : OpenXRFeaturePlugin<BaseFeature> {...}]]>.
        /// This function also works with <![CDATA[class DerivedFeature : BaseFeature {...}]]>.
        /// </remarks>
        internal static T GetOpenXRFeature<T>(BuildTargetGroup buildTargetGroup, bool returnNullWhenLoaderDisabled = true) where T : OpenXRFeature
        {
            if (returnNullWhenLoaderDisabled && !IsLoaderEnabledForTarget(buildTargetGroup))
            {
                return null;
            }

            EditorBuildSettings.TryGetConfigObject(Constants.k_SettingsKey, out UnityEngine.Object obj);
            OpenXRSettings openXRSettings = null;
            if (obj is IPackageSettings packageSettings)
            {
                openXRSettings = packageSettings.GetSettingsForBuildTargetGroup(buildTargetGroup);
            }

            if (openXRSettings != null)
            {
                foreach (OpenXRFeature feature in openXRSettings.GetFeatures())
                {
                    if (feature is T)
                    {
                        return feature as T;
                    }
                }
            }

            return null;
        }

        internal static bool IsLoaderEnabledForTarget(BuildTargetGroup buildTargetGroup)
        {
            XRManagerSettings settings = XRSettingsHelpers.GetOrCreateXRManagerSettings(buildTargetGroup);
            if (settings == null)
            {
                return false;
            }

            IReadOnlyList<XRLoader> loaders = settings.activeLoaders;
            for (int i = 0; i < loaders.Count; i++)
            {
                if (loaders[i] is OpenXRLoaderBase)
                {
                    return true;
                }
            }

            return false;
        }

        internal class AndroidXmlDocument : XmlDocument
        {
            private readonly string m_Path;
            protected XmlNamespaceManager nsMgr;
            public const string AndroidXmlNamespace = "http://schemas.android.com/apk/res/android";

            public AndroidXmlDocument(string path)
            {
                m_Path = path;
                using (var reader = new XmlTextReader(m_Path))
                {
                    reader.Read();
                    Load(reader);
                }

                nsMgr = new XmlNamespaceManager(NameTable);
                nsMgr.AddNamespace("android", AndroidXmlNamespace);
            }

            public string Save()
            {
                return SaveAs(m_Path);
            }

            public string SaveAs(string path)
            {
                using (var writer = new XmlTextWriter(path, new UTF8Encoding(false)))
                {
                    writer.Formatting = Formatting.Indented;
                    Save(writer);
                }

                return path;
            }
        }

        internal class AndroidManifest : AndroidXmlDocument
        {
            internal readonly XmlElement RootElement;
            internal readonly XmlElement ApplicationElement;
            internal readonly XmlElement IntentFilterElement;

            internal AndroidManifest(string path) : base(path)
            {
                RootElement = SelectSingleNode("/manifest") as XmlElement;
                ApplicationElement = SelectSingleNode("/manifest/application") as XmlElement;
                IntentFilterElement = SelectSingleNode("/manifest/application/activity/intent-filter") as XmlElement;
            }

            private static string _manifestFilePath;

            internal static string GetManifestPath(string basePath)
            {
                if (!string.IsNullOrEmpty(_manifestFilePath)) return _manifestFilePath;

                var pathBuilder = new StringBuilder(basePath);
                pathBuilder.Append(Path.DirectorySeparatorChar).Append("src");
                pathBuilder.Append(Path.DirectorySeparatorChar).Append("main");
                pathBuilder.Append(Path.DirectorySeparatorChar).Append("AndroidManifest.xml");
                _manifestFilePath = pathBuilder.ToString();

                return _manifestFilePath;
            }

            internal XmlNode GetOrCreateChild(XmlNode node, string name)
            {
                foreach (XmlNode child in node.ChildNodes)
                {
                    if (child.Name == name)
                    {
                        return child;
                    }
                }
                return node.AppendChild(CreateElement(name));
            }

            internal XmlAttribute CreateAndroidAttribute(string key, string value)
            {
                XmlAttribute attr = CreateAttribute("android", key, AndroidXmlNamespace);
                attr.Value = value;
                return attr;
            }

            internal static bool HasAttribute(XmlNode node, string name, string value)
            {
                foreach (XmlAttribute attribute in node.Attributes)
                {
                    if (attribute.Name == name && attribute.Value == value)
                    {
                        return true;
                    }
                }
                return false;
            }

            // return false if attribute is not found.
            internal static bool SetAttribute(XmlNode node, string name, string value)
            {
                foreach (XmlAttribute attribute in node.Attributes)
                {
                    if (attribute.Name == name)
                    {
                        attribute.Value = value;
                        return true;
                    }
                }
                return false;
            }
        }
    }
}
