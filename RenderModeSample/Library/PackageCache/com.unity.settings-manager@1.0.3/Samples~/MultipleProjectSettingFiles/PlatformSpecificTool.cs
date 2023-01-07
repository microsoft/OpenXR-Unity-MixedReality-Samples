using UnityEditor.EditorTools;
using UnityEngine;

namespace UnityEditor.SettingsManagement.Examples
{
    /// <summary>
    /// This example shows how to access multiple project setting repositories without making use of
    /// <see cref="UserSetting{T}"/>.
    /// </summary>
    [EditorTool("Editor Tool Settings Example")]
    class PerPlatformSettingsTool : EditorTool
    {
#if !UNITY_2019_2_OR_NEWER
        public override GUIContent toolbarIcon
        {
            get { return new GUIContent("Settings Example Tool", "Settings Manager Example Tool"); }
        }
#endif

        // This example creates two project settings repositories, A and B.
        static readonly string[] k_ProjectRepositories = new[]
        {
            "Settings A",
            "Settings B"
        };

        // The settings manager.
        static Settings s_Settings;

        // This is the key that is used to store the color setting.
        const string k_ToolColorSetting = "ToolColor";

        // Current tool color
        Color m_ToolColor;

        // The repository that color is read from and written to.
        int m_Repository;

        Vector3 m_HandlePosition;

        // Get the color value from a repository, setting a default value if the key does not already exist. This is
        // handled for you if using UserSetting{T}.
        Color GetToolColor(string repository, Color defaultColor)
        {
            if (!s_Settings.ContainsKey<Color>(k_ToolColorSetting, repository))
                s_Settings.Set<Color>(k_ToolColorSetting, defaultColor, repository);

            return s_Settings.Get<Color>(k_ToolColorSetting, k_ProjectRepositories[m_Repository]);
        }

        void OnEnable()
        {
            s_Settings = new Settings(new ISettingsRepository[]
            {
                new UserSettingsRepository(),
                new PackageSettingsRepository("com.unity.settings-manager-examples", k_ProjectRepositories[0]),
                new PackageSettingsRepository("com.unity.settings-manager-examples", k_ProjectRepositories[1])
            });

            m_Repository = s_Settings.Get<int>("ToolColorRepositoryName", SettingsScope.User);

            m_ToolColor = GetToolColor(k_ProjectRepositories[m_Repository], Color.blue);
        }

        public override void OnToolGUI(EditorWindow window)
        {
            Handles.BeginGUI();

            GUILayout.BeginVertical(GUILayout.MaxWidth(300));

            EditorGUI.BeginChangeCheck();
            m_Repository = EditorGUILayout.IntPopup(m_Repository, k_ProjectRepositories, new int[] { 0, 1 });
            if (EditorGUI.EndChangeCheck())
                m_ToolColor = GetToolColor(k_ProjectRepositories[m_Repository], Color.blue);

            EditorGUI.BeginChangeCheck();
            m_ToolColor = EditorGUILayout.ColorField(m_ToolColor);
            if (EditorGUI.EndChangeCheck())
            {
                s_Settings.Set<Color>(k_ToolColorSetting, m_ToolColor, k_ProjectRepositories[m_Repository]);
                s_Settings.Save();
            }

            GUILayout.EndVertical();

            Handles.EndGUI();

            using (new Handles.DrawingScope(m_ToolColor))
            {
                m_HandlePosition = Handles.Slider(m_HandlePosition, Vector3.right);
            }
        }
    }
}
