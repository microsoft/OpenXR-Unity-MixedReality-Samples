using Unity.PlasticSCM.Editor.AssetsOverlays.Cache;

namespace Unity.PlasticSCM.Editor.AssetUtils.Processor
{
    internal static class AssetsProcessors
    {
        internal static void Enable(
            PlasticAssetsProcessor plasticAssetsProcessor,
            IAssetStatusCache assetStatusCache)
        {
            AssetPostprocessor.Enable(plasticAssetsProcessor);
            AssetModificationProcessor.Enable(assetStatusCache);
        }

        internal static void Disable()
        {
            AssetPostprocessor.Disable();
            AssetModificationProcessor.Disable();
        }
    }
}
