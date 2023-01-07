using UnityEditor;
using UnityEngine.UIElements;

namespace Unity.Services.Core.Editor.ProjectBindRedirect
{
    class ProjectBindRedirectProjectSettingsUi
    {
#if UNITY_2020_1_OR_NEWER
        const string k_ProjectBindingRedirectPath = "Project/Services";
        const string k_ProjectRedirectButtonText = "Go to Services General Settings";
#else
        const string k_ProjectBindingRedirectPath = "Window/General/Services";
        const string k_ProjectRedirectButtonText = "Go to Services window";
#endif

        public static void Load(VisualElement parentElement)
        {
            SetupUxmlAndUss(parentElement);
            SetupRedirectButton(parentElement);
            AddProjectBindRedirectContentUI(parentElement);

            EditorGameServiceSettingsProvider.TranslateStringsInTree(parentElement);
        }

        static void SetupUxmlAndUss(VisualElement containerElement)
        {
            var visualAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(ProjectBindRedirectUiConstants.UxmlPath.ProjectSettings);
            if (visualAsset != null)
            {
                visualAsset.CloneTree(containerElement);
            }

            VisualElementHelper.AddStyleSheetFromPath(containerElement, ProjectBindRedirectUiConstants.UssPath.ProjectSettings);
        }

        static void AddProjectBindRedirectContentUI(VisualElement parentElement)
        {
            var contentContainer = parentElement.Q(className: ProjectBindRedirectUiConstants.UxmlClassNames.ContentContainer) ?? parentElement;
            ProjectBindRedirectContentUI.Load(contentContainer);
        }

        static void SetupRedirectButton(VisualElement containerElement)
        {
            var redirectButton = containerElement.Q<Button>(className: ProjectBindRedirectUiConstants.UxmlClassNames.ProjectRedirectButton);
            if (redirectButton != null)
            {
                redirectButton.clickable.clicked += RedirectToProjectBinding;
                redirectButton.text = k_ProjectRedirectButtonText;
            }
        }

        static void RedirectToProjectBinding()
        {
#if UNITY_2020_1_OR_NEWER
            SettingsService.OpenProjectSettings(k_ProjectBindingRedirectPath);
#else
            EditorApplication.ExecuteMenuItem(k_ProjectBindingRedirectPath);
#endif
        }
    }
}
