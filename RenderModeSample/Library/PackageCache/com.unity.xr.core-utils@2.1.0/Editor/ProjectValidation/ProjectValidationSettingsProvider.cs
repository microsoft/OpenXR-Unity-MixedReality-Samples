using UnityEditor;
using UnityEngine.UIElements;

namespace Unity.XR.CoreUtils.Editor
{
    internal class ProjectValidationSettingsProvider : SettingsProvider
    {
        const string ProjectValidationSettingsPath = "Project/XR Plug-in Management/Project Validation";

        ProjectValidationDrawer m_ValidationDrawer;
        
        [SettingsProvider]
        public static SettingsProvider CreateProjectValidationSettingsProvider()
        {
            return new ProjectValidationSettingsProvider();
        }

        ProjectValidationSettingsProvider(string path = ProjectValidationSettingsPath,
                                          SettingsScope scopes = SettingsScope.Project)
            : base(path, scopes) {}

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            base.OnActivate(searchContext, rootElement);
            m_ValidationDrawer = new ProjectValidationDrawer(BuildTargetGroup.Unknown);
        }

        public override void OnGUI(string searchContext)
        {
            base.OnGUI(searchContext);
            if(m_ValidationDrawer == null)
                m_ValidationDrawer = new ProjectValidationDrawer(BuildTargetGroup.Unknown);
            
            m_ValidationDrawer.OnGUI();
        }

        public override void OnInspectorUpdate()
        {
            base.OnInspectorUpdate();
            if (m_ValidationDrawer.UpdateIssues(true, false))
                Repaint();
        }
    }
}
