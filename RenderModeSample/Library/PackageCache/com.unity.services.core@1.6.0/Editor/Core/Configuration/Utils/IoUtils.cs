using System;
using System.IO;
using System.Linq;
using UnityEngine;
using Unity.Services.Core.Internal;

namespace Unity.Services.Core.Configuration.Editor
{
    static class IoUtils
    {
        const string k_MetaExtension = ".meta";
        public const string packageDefaultPath = "Packages/com.unity.services.core/Editor/Core";

        public static bool TryDeleteAssetFile(string path)
        {
            return TryDeleteFile(path) && TryDeleteFile(path + k_MetaExtension);
        }

        static bool TryDeleteFile(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                    return true;
                }
            }
            catch (Exception e)
            {
                CoreLogger.LogException(e);
            }

            return false;
        }

        public static void TryDeleteAssetFolder(string path)
        {
            if (TryDeleteFolder(path))
            {
                TryDeleteFile(path + k_MetaExtension);
            }
        }

        static bool TryDeleteFolder(string path)
        {
            try
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                    return true;
                }
            }
            catch (Exception e)
            {
                CoreLogger.LogException(e);
            }

            return false;
        }

        public static void TryDeleteStreamAssetsFolder()
        {
            var streamingAssetsPath = Application.streamingAssetsPath;

            if(Directory.Exists(streamingAssetsPath) &&
                IsDirectoryEmpty(streamingAssetsPath))
            {
                TryDeleteAssetFolder(streamingAssetsPath);
            }
        }

        static bool IsDirectoryEmpty(string path)
        {
            return !Directory.EnumerateFileSystemEntries(path).Any();
        }
    }
}
