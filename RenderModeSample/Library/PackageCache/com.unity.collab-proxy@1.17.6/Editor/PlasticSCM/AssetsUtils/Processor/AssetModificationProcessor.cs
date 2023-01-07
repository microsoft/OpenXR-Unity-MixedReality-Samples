using System.IO;

using UnityEditor;

using Unity.PlasticSCM.Editor.AssetsOverlays.Cache;
using Unity.PlasticSCM.Editor.UI;
using AssetOverlays = Unity.PlasticSCM.Editor.AssetsOverlays;

namespace Unity.PlasticSCM.Editor.AssetUtils.Processor
{
    class AssetModificationProcessor : UnityEditor.AssetModificationProcessor
    {
        internal static bool ForceCheckout { get; private set; }

        /*We need to do a checkout, verifying that the content/date or size has changed. In order
          to do this checkout we need the changes to have reached the disk. That's why we save the
          changed files in this array, and when they are reloaded in 
        AssetPostprocessor.OnPostprocessAllAssets we process them. */
        internal static string[] ModifiedAssets { get; set; }

        static AssetModificationProcessor()
        {
            ForceCheckout = EditorPrefs.GetBool(
                UnityConstants.FORCE_CHECKOUT_KEY_NAME);
        }

        internal static void Enable(
            IAssetStatusCache assetStatusCache)
        {
            mAssetStatusCache = assetStatusCache;
            sIsEnabled = true;
        }

        internal static void Disable()
        {
            sIsEnabled = false;
            mAssetStatusCache = null;
        }

        internal static void SetForceCheckoutOption(bool isEnabled)
        {
            ForceCheckout = isEnabled;
            EditorPrefs.SetBool(
                UnityConstants.FORCE_CHECKOUT_KEY_NAME,
                isEnabled);
        }

        static string[] OnWillSaveAssets(string[] paths)
        {
            if (!sIsEnabled)
                return paths;

            ModifiedAssets = (string[])paths.Clone();

            return paths;
        }

        static bool IsOpenForEdit(string assetPath, out string message)
        {
            message = string.Empty;

            if (!sIsEnabled)
                return true;

            if (assetPath.StartsWith("ProjectSettings/"))
                return true;

            if (!ForceCheckout)
                return true;

            if (MetaPath.IsMetaPath(assetPath))
                assetPath = MetaPath.GetPathFromMetaPath(assetPath);

            AssetOverlays.AssetStatus status = mAssetStatusCache.GetStatusForPath(
                Path.GetFullPath(assetPath));

            if (AssetOverlays.ClassifyAssetStatus.IsAdded(status) ||
                AssetOverlays.ClassifyAssetStatus.IsCheckedOut(status))
                return true;

            return !AssetOverlays.ClassifyAssetStatus.IsControlled(status);
        }

        static bool sIsEnabled;
        static IAssetStatusCache mAssetStatusCache;
    }
}
