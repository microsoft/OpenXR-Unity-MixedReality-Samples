namespace Unity.Services.Core.Editor
{
    class ProjectStateHelper : IProjectStateHelper
    {
        public bool IsProjectOnlyPartiallyBound(ProjectState projectState)
        {
            return projectState.ProjectBound && string.IsNullOrWhiteSpace(projectState.OrganizationId);
        }

        public bool IsProjectBeingBound(ProjectState? previousProjectState, ProjectState currentProjectState)
        {
            return (!previousProjectState.HasValue
                    || !previousProjectState.Value.ProjectBound)
                && currentProjectState.ProjectBound;
        }

        public bool IsProjectBeingUnbound(ProjectState? cachedProjectState, ProjectState currentProjectState)
        {
            return (!cachedProjectState.HasValue
                    || cachedProjectState.Value.ProjectBound)
                && !currentProjectState.ProjectBound;
        }
    }
}
