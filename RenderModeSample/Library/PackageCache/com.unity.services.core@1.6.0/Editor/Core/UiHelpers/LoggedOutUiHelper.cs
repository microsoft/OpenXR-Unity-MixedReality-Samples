using System;
using UnityEditor;
using UnityEngine.UIElements;

namespace Unity.Services.Core.Editor
{
    static class LoggedOutUiHelper
    {
        const string k_UxmlPath = "Packages/com.unity.services.core/Editor/Core/UiHelpers/UXML/LoggedOut.uxml";

        internal static class UiClass
        {
            public const string Button = "submit-button";
        }

        public static void AddLoggedOutUI(VisualElement loggedOutContainer)
        {
            if (loggedOutContainer == null)
                return;

            var visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(k_UxmlPath);
            if (visualTreeAsset != null)
            {
                visualTreeAsset.CloneTree(loggedOutContainer);

                SetupButton(loggedOutContainer);
            }
        }

        static void SetupButton(VisualElement buttonContainer)
        {
            var button = buttonContainer.Q<Button>(className: UiClass.Button);
            if (button != null)
            {
#if ENABLE_EDITOR_GAME_SERVICES
                button.clickable.clicked += CloudProjectSettings.ShowLogin;
#else
                VisualElementHelper.SetDisplayStyle(button, DisplayStyle.None);
#endif
            }
        }
    }
}
