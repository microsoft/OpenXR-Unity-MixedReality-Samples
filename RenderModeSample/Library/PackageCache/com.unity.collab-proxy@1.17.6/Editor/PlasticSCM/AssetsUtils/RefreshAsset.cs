using Unity.PlasticSCM.Editor.AssetUtils.Processor;

namespace Unity.PlasticSCM.Editor.AssetUtils
{
    internal static class RefreshAsset
    {
        internal static void BeforeLongAssetOperation()
        {
            UnityEditor.AssetDatabase.DisallowAutoRefresh();
        }

        internal static void AfterLongAssetOperation()
        {
            UnityEditor.AssetDatabase.AllowAutoRefresh();

            UnityAssetDatabase();
        }

        internal static void UnityAssetDatabase()
        {
            UnityEditor.AssetDatabase.Refresh(
                UnityEditor.ImportAssetOptions.Default);

            UnityEditor.VersionControl.Provider.ClearCache();

            AssetPostprocessor.SetIsRepaintInspectorNeededAfterAssetDatabaseRefresh();
        }

        internal static void VersionControlCache()
        {
            UnityEditor.VersionControl.Provider.ClearCache();

            RepaintInspector.All();
        }
    }
}