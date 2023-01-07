using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime;

using UnityEngine;
using UnityEditor;


namespace UnityEditor.XR.Management
{
    internal class XRPackageInitializationSettings : ScriptableObject
    {
        private static XRPackageInitializationSettings s_PackageSettings = null;
        private static object s_Lock = new object();

        internal static string s_ProjectSettingsAssetName = "XRPackageSettings.asset";
        internal static string s_ProjectSettingsFolder = "../ProjectSettings";
        internal static string s_ProjectSettingsPath;
        internal static string s_PackageInitPath;


        [SerializeField]
        private List<string> m_Settings = new List<string>();

        private XRPackageInitializationSettings(){ }

        internal static XRPackageInitializationSettings Instance
        {
            get
            {
                if (s_PackageSettings == null)
                {
                    lock(s_Lock)
                    {
                        if (s_PackageSettings == null)
                        {
                            s_PackageSettings = ScriptableObject.CreateInstance<XRPackageInitializationSettings>();
                            s_PackageSettings.LoadSettings();
                        }
                    }
                }
                return s_PackageSettings;
            }
        }

        void InitPaths()
        {
            if (String.IsNullOrEmpty(s_ProjectSettingsPath)) s_ProjectSettingsPath = Path.Combine(Application.dataPath, s_ProjectSettingsFolder);
            if (String.IsNullOrEmpty(s_PackageInitPath)) s_PackageInitPath = Path.Combine(s_ProjectSettingsPath, s_ProjectSettingsAssetName);
        }

        void OnEnable()
        {
            InitPaths();
        }

        internal void LoadSettings()
        {
            InitPaths();
            if (File.Exists(s_PackageInitPath))
            {
                using (StreamReader sr = new StreamReader(s_PackageInitPath))
                {
                    string settings = sr.ReadToEnd();
                    JsonUtility.FromJsonOverwrite(settings, this);
                }
            }
        }


        internal void SaveSettings()
        {
            InitPaths();
            if (!Directory.Exists(s_ProjectSettingsPath))
                Directory.CreateDirectory(s_ProjectSettingsPath);

            using (StreamWriter sw = new StreamWriter(s_PackageInitPath))
            {
                string settings = JsonUtility.ToJson(this, true);
                sw.Write(settings);
            }
        }

        internal bool HasSettings(string key)
        {
            return m_Settings.Contains(key);
        }

        internal void AddSettings(string key)
        {
            if (!HasSettings(key))
                m_Settings.Add(key);
        }
    }
}
