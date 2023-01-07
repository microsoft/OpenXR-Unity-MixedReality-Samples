using System;

namespace Unity.XR.CoreUtils
{
    /// <summary>
    /// Allows a class inheriting from <see cref="ScriptableSettings{T}"/> to specify that its instance Asset
    /// should be saved under "Assets/[<see cref="Path"/>]/Resources/ScriptableSettings/".
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ScriptableSettingsPathAttribute : Attribute
    {
        readonly string m_Path;

        /// <summary>
        /// The path where this ScriptableSettings should be stored
        /// </summary>
        public string Path => m_Path;

        /// <summary>
        /// Initialize a new ScriptableSettingsPathAttribute
        /// </summary>
        /// <param name="path">The path where the ScriptableSettings should be stored</param>
        public ScriptableSettingsPathAttribute(string path = "")
        {
            m_Path = path;
        }
    }
}
