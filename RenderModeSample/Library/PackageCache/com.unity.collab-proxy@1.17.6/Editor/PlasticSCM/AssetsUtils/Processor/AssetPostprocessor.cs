using Codice.Client.Common.FsNodeReaders.Watcher;

namespace Unity.PlasticSCM.Editor.AssetUtils.Processor
{
    class AssetPostprocessor : UnityEditor.AssetPostprocessor
    {
        internal static void Enable(
            PlasticAssetsProcessor plasticAssetsProcessor)
        {
            mPlasticAssetsProcessor = plasticAssetsProcessor;
            mIsEnabled = true;
        }

        internal static void Disable()
        {
            mIsEnabled = false;
            mPlasticAssetsProcessor = null;
        }

        internal static void SetIsRepaintInspectorNeededAfterAssetDatabaseRefresh()
        {
            mIsRepaintInspectorNeededAfterAssetDatabaseRefresh = true;
        }

        static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            if (!mIsEnabled)
                return;

            if (mIsRepaintInspectorNeededAfterAssetDatabaseRefresh)
            {
                mIsRepaintInspectorNeededAfterAssetDatabaseRefresh = false;
                RepaintInspector.All();
            }

            // We need to ensure that the FSWatcher is enabled before processing Plastic operations
            // It fixes the following scenario: 
            // 1. Close PlasticSCM window
            // 2. Create an asset, it appears with the added overlay
            // 3. Open PlasticSCM window, the asset should appear as added instead of deleted locally
            MonoFileSystemWatcher.IsEnabled = true;

            for (int i = 0; i < movedAssets.Length; i++)
            {
                mPlasticAssetsProcessor.MoveOnSourceControl(
                    movedFromAssetPaths[i],
                    movedAssets[i]);
            }

            foreach (string deletedAsset in deletedAssets)
            {
                mPlasticAssetsProcessor.DeleteFromSourceControl(
                    deletedAsset);
            }

            mPlasticAssetsProcessor.AddToSourceControl(importedAssets);

            if (AssetModificationProcessor.ModifiedAssets == null)
                return;

            mPlasticAssetsProcessor.CheckoutOnSourceControl(
                AssetModificationProcessor.ModifiedAssets);

            AssetModificationProcessor.ModifiedAssets = null;
        }

        static bool mIsEnabled;
        static bool mIsRepaintInspectorNeededAfterAssetDatabaseRefresh;
        static PlasticAssetsProcessor mPlasticAssetsProcessor;
    }
}
