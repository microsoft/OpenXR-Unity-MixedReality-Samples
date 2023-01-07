namespace Unity.Services.Core.Editor
{
    interface IEditorGameServiceAnalyticsSender
    {
        void SendProjectSettingsGoToDashboardEvent(string package);

        void SendProjectBindPopupCloseActionEvent(string package);
        void SendClickedSignUpLinkActionEvent(string package);

        void SendProjectBindPopupOpenProjectSettingsEvent(string package);

        void SendProjectBindPopupDisplayedEvent(string package);
    }
}
