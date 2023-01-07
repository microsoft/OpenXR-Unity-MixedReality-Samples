using Unity.Services.Core.Internal;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using Unity.Services.Core.Registration;

namespace Unity.Services.Core.Editor
{
    class ProjectUnlinkBuildWarning : IPreprocessBuildWithReport
    {
        public int callbackOrder { get; }

        readonly IProjectStateRequest k_ProjectStateRequest;

        /// <remarks>
        /// Necessary for <see cref="IPostprocessBuildWithReport"/> compatibility.
        /// </remarks>
        public ProjectUnlinkBuildWarning()
            : this(new ProjectStateRequest()) {}

        public ProjectUnlinkBuildWarning(IProjectStateRequest projectStateRequest)
        {
            k_ProjectStateRequest = projectStateRequest;
        }

        public void OnPreprocessBuild(BuildReport report)
        {
            if (!IsProjectLinked(k_ProjectStateRequest.GetProjectState()))
            {
                CoreLogger.LogWarning(CorePackageInitializer.ProjectUnlinkMessage);
            }

            bool IsProjectLinked(ProjectState projectState)
            {
                return (projectState.ProjectBound && projectState.IsLoggedIn);
            }
        }
    }
}
