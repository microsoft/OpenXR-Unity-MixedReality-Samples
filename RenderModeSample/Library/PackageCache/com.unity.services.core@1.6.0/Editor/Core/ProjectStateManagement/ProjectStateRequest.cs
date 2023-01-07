using System;
using UnityEditor;
using UnityEngine;

namespace Unity.Services.Core.Editor
{
    class ProjectStateRequest : IProjectStateRequest
    {
        const string k_UserNameAnonymous = "anonymous";

        public ProjectState GetProjectState()
        {
#if ENABLE_EDITOR_GAME_SERVICES
            return new ProjectState(
                CloudProjectSettings.userId, CloudProjectSettings.userName, CloudProjectSettings.accessToken,
                CloudProjectSettings.projectId, CloudProjectSettings.projectName, CloudProjectSettings.organizationId,
                CloudProjectSettings.organizationName, CloudProjectSettings.coppaCompliance,
                CloudProjectSettings.projectBound, IsInternetReachable(), IsLoggedIn());
#else
            return new ProjectState(
                CloudProjectSettings.userId, CloudProjectSettings.userName, CloudProjectSettings.accessToken,
                CloudProjectSettings.projectId, CloudProjectSettings.projectName, CloudProjectSettings.organizationId,
                CloudProjectSettings.organizationName, IsProjectBound(), IsInternetReachable(), IsLoggedIn());
#endif
        }

        static bool IsLoggedIn()
        {
            return !string.IsNullOrEmpty(CloudProjectSettings.userId) &&
                !CloudProjectSettings.userName.Equals(k_UserNameAnonymous, StringComparison.InvariantCultureIgnoreCase);
        }

#if !ENABLE_EDITOR_GAME_SERVICES
        static bool IsProjectBound()
        {
            return !(string.IsNullOrEmpty(CloudProjectSettings.organizationId) ||
                string.IsNullOrEmpty(CloudProjectSettings.organizationName) ||
                string.IsNullOrEmpty(CloudProjectSettings.projectId) ||
                string.IsNullOrEmpty(CloudProjectSettings.projectName));
        }
#endif

        static bool IsInternetReachable()
        {
            return Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork;
        }
    }
}
