using System;
using UnityEngine;

namespace Unity.XR.CoreUtils
{
    /// <inheritdoc />
    /// <summary>
    /// Based off of Unity's Internal ScriptableSingleton with UnityEditorInternal bits removed
    /// </summary>
    /// <typeparam name="T">The class being created</typeparam>
    public abstract class ScriptableSettings<T> : ScriptableSettingsBase<T> where T : ScriptableObject
    {
        const string k_CustomSavePathFormat = "{0}Resources/{1}.asset";
        const string k_SavePathFormat = "{0}Resources/ScriptableSettings/{1}.asset";
        const string k_LoadPathFormat = "ScriptableSettings/{0}";

        /// <summary>
        /// Retrieves a reference to the given settings class. Will load and initialize once, and cache for all future access.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (BaseInstance == null)
                    CreateAndLoad();

                return BaseInstance;
            }
        }

        internal static T CreateAndLoad()
        {
            System.Diagnostics.Debug.Assert(BaseInstance == null);

            // Try to load the singleton
            var path = HasCustomPath ? GetFilePath() : string.Format(k_LoadPathFormat, GetFilePath());
            BaseInstance = Resources.Load(path) as T;

            // Create it if it doesn't exist
            if (BaseInstance == null)
            {
                BaseInstance = CreateInstance<T>();

                // And save it back out if appropriate
                Save(HasCustomPath ? k_CustomSavePathFormat : k_SavePathFormat);
            }

            System.Diagnostics.Debug.Assert(BaseInstance != null);

            return BaseInstance;
        }
    }
}
