using System.Threading.Tasks;
using UnityEngine.Analytics;

namespace Unity.Services.Core.Editor
{
    interface IEditorAnalyticsEventRegistrant
    {
        /// <summary>
        /// Meant to register EditorAnalytics event using this method:
        /// EditorAnalytics.RegisterEventWithLimit
        /// Expected to be handled asynchronously because it will fail if called before
        /// EditorAnalytics are enabled, which they should be at Editor Start but can be disabled in Unity Pro
        /// </summary>
        /// <returns>AnalyticsResult</returns>
        Task<AnalyticsResult> RegisterEditorAnalyticsEventAsync();
    }
}
