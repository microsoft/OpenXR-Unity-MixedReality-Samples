using UnityEditor;
using UnityEngine.UIElements;

namespace Unity.Services.Core.Editor.ProjectBindRedirect
{
    static class ProjectBindRedirectContentUI
    {
        public static void Load(VisualElement parent)
        {
            var visualAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                ProjectBindRedirectUiConstants.UxmlPath.Content);
            if (visualAsset != null)
            {
                visualAsset.CloneTree(parent);
            }
        }
    }
}
