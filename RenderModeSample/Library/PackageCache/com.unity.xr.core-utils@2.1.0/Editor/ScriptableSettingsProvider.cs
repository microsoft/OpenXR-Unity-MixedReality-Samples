using UnityEditor;
using UnityEngine.UIElements;

namespace Unity.XR.CoreUtils.Editor
{
    /// <summary>
    /// Expose a ScriptableSettings of type T as a settings provider
    /// </summary>
    /// <typeparam name="T">The ScriptableSettings type which will be exposed</typeparam>
    public abstract class ScriptableSettingsProvider<T> : SettingsProvider where T : ScriptableSettingsBase<T>
    {
        T m_Target;
        SerializedObject m_SerializedObject;

        /// <summary>
        /// The ScriptableSettings being provided
        /// </summary>
        protected T Target
        {
            get
            {
                if (m_Target == null || m_SerializedObject == null)
                    GetSerializedSettings();

                return m_Target;
            }
        }

        /// <summary>
        /// A SerializedObject representing the ScriptableSettings being provided
        /// </summary>
        protected SerializedObject SerializedObject
        {
            get
            {
                if (m_SerializedObject == null)
                    m_SerializedObject = GetSerializedSettings();

                return m_SerializedObject;
            }
        }

        /// <summary>
        /// Initialize a new ScriptableSettingsProvider
        /// </summary>
        /// <param name="path">The path to this settings view within the Preferences or Project Settings window</param>
        /// <param name="scope">The scope of these settings</param>
        protected ScriptableSettingsProvider(string path, SettingsScope scope = SettingsScope.User)
            : base(path, scope) {}

        /// <summary>
        /// Use this function to implement a handler for when the user clicks on the Settings in the Settings window.
        /// You can fetch a settings Asset or set up UIElements UI from this function.
        /// </summary>
        /// <param name="searchContext">Search context in the search box on the Settings window.</param>
        /// <param name="rootElement">Root of the UIElements tree. If you add to this root, the SettingsProvider uses
        /// UIElements instead of calling SettingsProvider.OnGUI to build the UI. If you do not add to this
        /// VisualElement, then you must use the IMGUI to build the UI.</param>
        public abstract override void OnActivate(string searchContext, VisualElement rootElement);

        /// <summary>
        /// Use this function to draw the UI based on IMGUI. This assumes you haven't added any children to the
        /// rootElement passed to the OnActivate function.
        /// </summary>
        /// <param name="searchContext">Search context for the Settings window. Used to show or hide relevant properties.</param>
        public abstract override void OnGUI(string searchContext);

        /// <summary>
        /// Initialize this settings object and return a SerializedObject wrapping it
        /// </summary>
        /// <returns>The SerializedObject wrapper</returns>
        SerializedObject GetSerializedSettings()
        {
            if (typeof(EditorScriptableSettings<T>).IsAssignableFrom(typeof(T)))
            {
                m_Target = EditorScriptableSettings<T>.Instance;
                return new SerializedObject(m_Target);
            }

            m_Target = ScriptableSettings<T>.Instance;
            return new SerializedObject(m_Target);
        }
    }
}
