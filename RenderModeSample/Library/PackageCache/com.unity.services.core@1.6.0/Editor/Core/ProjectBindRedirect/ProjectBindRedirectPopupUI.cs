using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using NotNull = JetBrains.Annotations.NotNullAttribute;

namespace Unity.Services.Core.Editor.ProjectBindRedirect
{
    class ProjectBindRedirectPopupUI
    {
        const string k_ProjectSettingsPath = "Project/Services";
        const string k_SignUpClassName = "signup-link-button";
        const string k_SignupLink = "https://dashboard.unity3d.com";

        readonly Action m_OnCloseButtonFired;
        [NotNull]
        readonly List<string> m_InstalledPackages;
        readonly IEditorGameServiceAnalyticsSender m_EditorGameServiceAnalyticsSender;

        public ProjectBindRedirectPopupUI(
            VisualElement parentElement, Action closeAction, [NotNull] List<string> installedPackages,
            [NotNull] IEditorGameServiceAnalyticsSender editorGameServiceAnalyticsSender)
        {
            m_EditorGameServiceAnalyticsSender = editorGameServiceAnalyticsSender;

            SetupUxmlAndUss(parentElement);
            SetupButtons(parentElement);
            AddProjectBindRedirectContentUI(parentElement);
            SetSignupLink(parentElement);

            EditorGameServiceSettingsProvider.TranslateStringsInTree(parentElement);

            m_OnCloseButtonFired = closeAction;
            m_InstalledPackages = installedPackages;
        }

        static void SetupUxmlAndUss(VisualElement containerElement)
        {
            var visualAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(ProjectBindRedirectUiConstants.UxmlPath.Popup);
            if (visualAsset != null)
            {
                visualAsset.CloneTree(containerElement);
            }

            VisualElementHelper.AddStyleSheetFromPath(containerElement, ProjectBindRedirectUiConstants.UssPath.Popup);

            if (EditorGUIUtility.isProSkin)
            {
                VisualElementHelper.AddStyleSheetFromPath(containerElement, ProjectBindRedirectUiConstants.UssPath.PopupDark);
            }
            else
            {
                VisualElementHelper.AddStyleSheetFromPath(containerElement, ProjectBindRedirectUiConstants.UssPath.PopupLight);
            }
        }

        static void AddProjectBindRedirectContentUI(VisualElement parentElement)
        {
            var contentContainer = parentElement.Q(className: ProjectBindRedirectUiConstants.UxmlClassNames.ContentContainer) ?? parentElement;
            ProjectBindRedirectContentUI.Load(contentContainer);
        }

        void SetupButtons(VisualElement containerElement)
        {
            var cancelButton = containerElement.Q<Button>(className: ProjectBindRedirectUiConstants.UxmlClassNames.CancelButton);
            if (cancelButton != null)
            {
                cancelButton.clickable.clicked += CloseButtonAction;
            }

            var confirmButton = containerElement.Q<Button>(className: ProjectBindRedirectUiConstants.UxmlClassNames.ConfirmationButton);
            if (confirmButton != null)
            {
                confirmButton.clickable.clicked += ConfirmButtonAction;
            }
        }

        void SetSignupLink(VisualElement parentElement)
        {
            var contentContainer = parentElement.Q(className: ProjectBindRedirectUiConstants.UxmlClassNames.ContentContainer) ?? parentElement;
            var dashboardHyperlink = contentContainer.Q<TextElement>(className: k_SignUpClassName);
            if (dashboardHyperlink is null)
            {
                return;
            }

            dashboardHyperlink.text = k_SignupLink;
            var clickable = new Clickable(ClickSignUpLinkAction);
            dashboardHyperlink.AddManipulator(clickable);
        }

        internal void CloseButtonAction()
        {
            foreach (var package in m_InstalledPackages)
            {
                m_EditorGameServiceAnalyticsSender.SendProjectBindPopupCloseActionEvent(package);
            }

            m_OnCloseButtonFired?.Invoke();
        }

        internal void ConfirmButtonAction()
        {
            foreach (var package in m_InstalledPackages)
            {
                m_EditorGameServiceAnalyticsSender.SendProjectBindPopupOpenProjectSettingsEvent(package);
            }

            SettingsService.OpenProjectSettings(k_ProjectSettingsPath);
            m_OnCloseButtonFired?.Invoke();
        }

        internal void ClickSignUpLinkAction()
        {
            foreach (var package in m_InstalledPackages)
            {
                m_EditorGameServiceAnalyticsSender.SendClickedSignUpLinkActionEvent(package);
            }

            Application.OpenURL(k_SignupLink);
        }
    }
}
