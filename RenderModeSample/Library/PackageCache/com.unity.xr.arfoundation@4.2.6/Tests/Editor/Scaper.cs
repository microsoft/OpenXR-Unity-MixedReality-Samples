#if UNITY_CODING_ENABLED
using Unity.Coding.Editor.ApiScraping;

namespace UnityEditor.XR.ARFoundation
{
    static class MenuOptions
    {
        [MenuItem("AR Foundation/Scrape APIs")]
        static void ScrapeApis() => ApiScraping.Scrape();
    }
}
#endif
