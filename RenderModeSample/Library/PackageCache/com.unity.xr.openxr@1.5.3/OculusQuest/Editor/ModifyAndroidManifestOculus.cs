using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using UnityEditor.Build.Reporting;

using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features.OculusQuestSupport;

namespace UnityEditor.XR.OpenXR.Features.OculusQuestSupport
{
    internal class ModifyAndroidManifestOculus : OpenXRFeatureBuildHooks
    {
        public override int callbackOrder => 1;

        public override Type featureType => typeof(OculusQuestFeature);

        protected override void OnPreprocessBuildExt(BuildReport report)
        {
        }

        protected override void OnPostGenerateGradleAndroidProjectExt(string path)
        {
            var androidManifest = new AndroidManifest(GetManifestPath(path));
            androidManifest.AddOculusMetaData();
            androidManifest.Save();
        }

        protected override void OnPostprocessBuildExt(BuildReport report)
        {
        }

        private string _manifestFilePath;

        private string GetManifestPath(string basePath)
        {
            if (!string.IsNullOrEmpty(_manifestFilePath)) return _manifestFilePath;

            var pathBuilder = new StringBuilder(basePath);
            pathBuilder.Append(Path.DirectorySeparatorChar).Append("src");
            pathBuilder.Append(Path.DirectorySeparatorChar).Append("main");
            pathBuilder.Append(Path.DirectorySeparatorChar).Append("AndroidManifest.xml");
            _manifestFilePath = pathBuilder.ToString();

            return _manifestFilePath;
        }

        private class AndroidXmlDocument : XmlDocument
        {
            private string m_Path;
            protected XmlNamespaceManager nsMgr;
            public readonly string AndroidXmlNamespace = "http://schemas.android.com/apk/res/android";

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

        private class AndroidManifest : AndroidXmlDocument
        {
            private readonly XmlElement ApplicationElement;
            private readonly XmlElement ActivityIntentFilterElement;
            private readonly XmlElement ActivityElement;
            private readonly XmlElement ManifestElement;

            public AndroidManifest(string path) : base(path)
            {
                ApplicationElement = SelectSingleNode("/manifest/application") as XmlElement;
                ActivityIntentFilterElement = SelectSingleNode("/manifest/application/activity/intent-filter") as XmlElement;
                ActivityElement = SelectSingleNode("manifest/application/activity") as XmlElement;
                ManifestElement = SelectSingleNode("/manifest") as XmlElement;
            }

            private XmlAttribute CreateAndroidAttribute(string key, string value)
            {
                XmlAttribute attr = CreateAttribute("android", key, AndroidXmlNamespace);
                attr.Value = value;
                return attr;
            }

            private void UpdateOrCreateAttribute(XmlElement xmlParentElement, string tag, string name, params (string name, string value)[] attributes)
            {
                var xmlNodeList = xmlParentElement.SelectNodes(tag);
                XmlElement targetNode = null;

                // Check all XmlNodes to see if a node with matching name already exists.
                foreach (XmlNode node in xmlNodeList)
                {
                    XmlAttribute nameAttr = (XmlAttribute)node.Attributes.GetNamedItem("name", AndroidXmlNamespace);
                    if (nameAttr != null && nameAttr.Value.Equals(name))
                    {
                        targetNode = (XmlElement)node;
                        break;
                    }
                }

                // If node exists, update the attribute values if they are present or create new ones as requested. Else, create new XmlElement.
                if (targetNode != null)
                {
                    for (int i = 0; i < attributes.Length; i++)
                    {
                        XmlAttribute attr = (XmlAttribute)targetNode.Attributes.GetNamedItem(attributes[i].name, AndroidXmlNamespace);
                        if (attr != null)
                        {
                            attr.Value = attributes[i].value;
                        }
                        else
                        {
                            targetNode.SetAttribute(attributes[i].name, AndroidXmlNamespace, attributes[i].value);
                        }
                    }
                }
                else
                {
                    XmlElement newElement = CreateElement(tag);
                    newElement.SetAttribute("name", AndroidXmlNamespace, name);
                    for (int i = 0; i < attributes.Length; i++)
                        newElement.SetAttribute(attributes[i].name, AndroidXmlNamespace, attributes[i].value);
                    xmlParentElement.AppendChild(newElement);
                }
            }

            internal void AddOculusMetaData()
            {
                OpenXRSettings androidOpenXRSettings = OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android);
                var questFeature = androidOpenXRSettings.GetFeature<OculusQuestFeature>();

                string supportedDevices = "quest|quest2";
                if (questFeature != null)
                {
                    List<string> deviceList = new List<string>();
                    if (questFeature.targetQuest)
                        deviceList.Add("quest");
                    if (questFeature.targetQuest2)
                        deviceList.Add("quest2");

                    if (deviceList.Count > 0)
                    {
                        supportedDevices = String.Join("|", deviceList.ToArray());
                    }
                    else
                    {
                        supportedDevices = null;
                        UnityEngine.Debug.LogWarning("No target devices selected in Oculus Quest Support Feature. No devices will be listed as supported in the application Android manifest.");
                    }
                }

                UpdateOrCreateAttribute(ActivityIntentFilterElement,
                    "category", "com.oculus.intent.category.VR"
                    );

                UpdateOrCreateAttribute(ActivityElement,
                    "meta-data", "com.oculus.vr.focusaware",
                    new (string name, string value)[] {
                        ("value", "true")
                    });

                UpdateOrCreateAttribute(ApplicationElement,
                    "meta-data", "com.oculus.supportedDevices",
                    new (string name, string value)[] {
                        ("value", supportedDevices)
                    });

                UpdateOrCreateAttribute(ManifestElement,
                    "uses-feature", "android.hardware.vr.headtracking",
                    new (string name, string value)[] {
                        ("required", "true"),
                        ("version", "1")
                    });

            }
        }
    }
}