using UnityEditor;

namespace Unity.Services.Core.Editor
{
    /// <summary>
    /// Data container of the state of the project when events are forwarded to the <see cref="IEditorGameService"/>
    /// </summary>
    struct ProjectState
    {
        /// <inheritdoc cref="CloudProjectSettings.userId"/>
        public string UserId;
        /// <inheritdoc cref="CloudProjectSettings.userName"/>
        public string UserName;
        /// <inheritdoc cref="CloudProjectSettings.accessToken"/>
        public string AccessToken;
        /// <inheritdoc cref="CloudProjectSettings.projectId"/>
        public string ProjectId;
        /// <inheritdoc cref="CloudProjectSettings.projectName"/>
        public string ProjectName;
        /// <inheritdoc cref="CloudProjectSettings.organizationId"/>
        public string OrganizationId;
        /// <inheritdoc cref="CloudProjectSettings.organizationName"/>
        public string OrganizationName;
#if ENABLE_EDITOR_GAME_SERVICES
        /// <inheritdoc cref="CloudProjectSettings.coppaCompliance"/>
        public CoppaCompliance CoppaCompliance;
#endif
        /// <inheritdoc cref="CloudProjectSettings.projectBound"/>
        public bool ProjectBound;

        /// <summary>
        /// Is the user connected to the internet.
        /// </summary>
        public bool IsOnline;

        /// <summary>
        /// Is the user logged in.
        /// </summary>
        public bool IsLoggedIn;

#if ENABLE_EDITOR_GAME_SERVICES
        public ProjectState(
            string userId, string userName, string accessToken, string projectId, string projectName,
            string organizationId, string organizationName, CoppaCompliance coppaCompliance, bool projectBound,
            bool isOnline, bool isLoggedIn)
        {
            UserId = userId;
            UserName = userName;
            AccessToken = accessToken;
            ProjectId = projectId;
            ProjectName = projectName;
            OrganizationId = organizationId;
            OrganizationName = organizationName;
            ProjectBound = projectBound;
            CoppaCompliance = coppaCompliance;
            IsOnline = isOnline;
            IsLoggedIn = isLoggedIn;
        }

#else
        public ProjectState(
            string userId, string userName, string accessToken, string projectId, string projectName,
            string organizationId, string organizationName, bool projectBound, bool isOnline, bool isLoggedIn)
        {
            UserId = userId;
            UserName = userName;
            AccessToken = accessToken;
            ProjectId = projectId;
            ProjectName = projectName;
            OrganizationId = organizationId;
            OrganizationName = organizationName;
            ProjectBound = projectBound;
            IsOnline = isOnline;
            IsLoggedIn = isLoggedIn;
        }

#endif

        public bool HasDiff(ProjectState projectStateObj)
        {
            return !(UserId.Equals(projectStateObj.UserId) &&
                UserName.Equals(projectStateObj.UserName) &&
                AccessToken.Equals(projectStateObj.AccessToken) &&
                ProjectId.Equals(projectStateObj.ProjectId) &&
                ProjectName.Equals(projectStateObj.ProjectName) &&
                OrganizationId.Equals(projectStateObj.OrganizationId) &&
                OrganizationName.Equals(projectStateObj.OrganizationName) &&
                ProjectBound.Equals(projectStateObj.ProjectBound) &&
#if ENABLE_EDITOR_GAME_SERVICES
                CoppaCompliance.Equals(projectStateObj.CoppaCompliance) &&
#endif
                IsOnline.Equals(projectStateObj.IsOnline) &&
                IsLoggedIn.Equals(projectStateObj.IsLoggedIn));
        }
    }
}
