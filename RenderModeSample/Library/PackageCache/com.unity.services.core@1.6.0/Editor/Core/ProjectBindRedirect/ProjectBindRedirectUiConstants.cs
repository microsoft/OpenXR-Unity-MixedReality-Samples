namespace Unity.Services.Core.Editor.ProjectBindRedirect
{
    static class ProjectBindRedirectUiConstants
    {
        public static class UxmlPath
        {
            public const string Popup = "Packages/com.unity.services.core/Editor/Core/ProjectBindRedirect/UXML/ProjectBindRedirectPopup.uxml";
            public const string ProjectSettings = "Packages/com.unity.services.core/Editor/Core/ProjectBindRedirect/UXML/ProjectBindRedirectProjectSettings.uxml";
            public const string Content = "Packages/com.unity.services.core/Editor/Core/ProjectBindRedirect/UXML/ProjectBindRedirectContent.uxml";
        }

        public static class UxmlClassNames
        {
            public const string ContentContainer = "content-container";
            public const string CancelButton = "cancel-button";
            public const string ConfirmationButton = "confirmation-button";
            public const string ProjectRedirectButton = "project-redirect-button";
        }

        public static class UssPath
        {
            public const string Popup = "Packages/com.unity.services.core/Editor/Core/ProjectBindRedirect/USS/ProjectBindRedirectPopupCommon.uss";
            public const string PopupLight = "Packages/com.unity.services.core/Editor/Core/ProjectBindRedirect/USS/ProjectBindRedirectPopupLight.uss";
            public const string PopupDark = "Packages/com.unity.services.core/Editor/Core/ProjectBindRedirect/USS/ProjectBindRedirectPopupDark.uss";
            public const string ProjectSettings = "Packages/com.unity.services.core/Editor/Core/ProjectBindRedirect/USS/ProjectBindRedirectProjectSettings.uss";
        }
    }
}
