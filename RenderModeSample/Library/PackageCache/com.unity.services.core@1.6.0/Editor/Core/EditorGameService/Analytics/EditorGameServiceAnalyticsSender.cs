using System;
using UnityEditor;
using UnityEngine;

namespace Unity.Services.Core.Editor
{
    class EditorGameServiceAnalyticsSender : IEditorGameServiceAnalyticsSender
    {
        static class AnalyticsComponent
        {
            public const string ProjectSettings = "Project Settings";
            public const string ProjectBindPopup = "Project Bind Popup";
        }

        static class AnalyticsAction
        {
            public const string GoToDashboard = "Go to Dashboard";
            public const string OpenProjectSettings = "Open Project Settings";
            public const string CloseProjectBindPopup = "Close Project Bind Popup";
            public const string ProjectBindPopupDisplayed = "Project Bind Popup Displayed";
            public const string ClickedSignUpLink = "Clicked Signup Link";
        }

        const int k_Version = 1;
        const string k_EventName = "editorgameserviceeditor";

        public void SendProjectSettingsGoToDashboardEvent(string package)
        {
            SendEvent(AnalyticsComponent.ProjectSettings, AnalyticsAction.GoToDashboard, package);
        }

        public void SendProjectBindPopupCloseActionEvent(string package)
        {
            SendEvent(AnalyticsComponent.ProjectBindPopup, AnalyticsAction.CloseProjectBindPopup, package);
        }

        public void SendClickedSignUpLinkActionEvent(string package)
        {
            SendEvent(AnalyticsComponent.ProjectBindPopup, AnalyticsAction.ClickedSignUpLink, package);
        }

        public void SendProjectBindPopupOpenProjectSettingsEvent(string package)
        {
            SendEvent(AnalyticsComponent.ProjectBindPopup, AnalyticsAction.OpenProjectSettings, package);
        }

        public void SendProjectBindPopupDisplayedEvent(string package)
        {
            SendEvent(AnalyticsComponent.ProjectBindPopup, AnalyticsAction.ProjectBindPopupDisplayed, package);
        }

        static void SendEvent(string component, string action, string package)
        {
            EditorAnalytics.SendEventWithLimit(k_EventName, new EditorGameServiceEvent
            {
                action = action,
                component = component,
                package = package
            }, k_Version);
        }

        /// <remarks>Lowercase is used here for compatibility with analytics.</remarks>
        [Serializable]
        public struct EditorGameServiceEvent
        {
            public string action;
            public string component;
            public string package;
        }
    }
}
