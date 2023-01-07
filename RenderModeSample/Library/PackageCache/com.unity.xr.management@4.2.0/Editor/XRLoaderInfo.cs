using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEngine.XR.Management;

namespace UnityEditor.XR.Management
{
    internal class XRLoaderInfo : IEquatable<XRLoaderInfo>
    {
        public Type loaderType;
        public string assetName;
        public XRLoader instance;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is XRLoaderInfo && Equals((XRLoaderInfo)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (loaderType != null ? loaderType.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (instance != null ? instance.GetHashCode() : 0);
                return hashCode;
            }
        }

        public bool Equals(XRLoaderInfo other)
        {
            return other != null && Equals(loaderType, other.loaderType) && Equals(instance, other.instance);
        }

        static string[] s_LoaderBlackList = { "DummyLoader", "SampleLoader", "XRLoaderHelper" };

        internal static void GetAllKnownLoaderInfos(List<XRLoaderInfo> newInfos)
        {
            var loaderTypes = TypeLoaderExtensions.GetAllTypesWithInterface<XRLoader>();
            foreach (Type loaderType in loaderTypes)
            {
                if (loaderType.IsAbstract)
                    continue;

                if (s_LoaderBlackList.Contains(loaderType.Name))
                    continue;

                var assets = AssetDatabase.FindAssets(String.Format("t:{0}", loaderType));
                if (!assets.Any())
                {
                    XRLoaderInfo info = new XRLoaderInfo();
                    info.loaderType = loaderType;
                    newInfos.Add(info);
                }
                else
                {
                    foreach (var asset in assets)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(asset);

                        XRLoaderInfo info = new XRLoaderInfo();
                        info.loaderType = loaderType;
                        info.instance = AssetDatabase.LoadAssetAtPath(path, loaderType) as XRLoader;
                        info.assetName = Path.GetFileNameWithoutExtension(path);
                        newInfos.Add(info);
                    }
                }
            }
        }
    }
}
