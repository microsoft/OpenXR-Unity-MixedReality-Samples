using Unity.Services.Core.Internal;

namespace Unity.Services.Core.Configuration.Internal
{
    /// <summary>
    /// Component for project configuration.
    /// </summary>
    /// <remarks>
    /// For WebGL platform, the configuration is only accessable if the application is hosted behind https. Behind http you will get an error: `Insecure connection not allowed`.
    /// </remarks>
    public interface IProjectConfiguration : IServiceComponent
    {
        /// <summary>
        /// Get the boolean value for the project config <paramref name="key"/>.
        /// </summary>
        /// <param name="key">
        /// The identifier of the project config to find.
        /// </param>
        /// <param name="defaultValue">
        /// The value returned if there is no match for the given <paramref name="key"/>.
        /// </param>
        /// <returns>
        /// Return the boolean value for the project config for the given <paramref name="key"/> if any;
        /// return <paramref name="defaultValue"/> otherwise.
        /// </returns>
        bool GetBool(string key, bool defaultValue = default);

        /// <summary>
        /// Get the integer value for the project config with the given <paramref name="key"/>.
        /// </summary>
        /// <param name="key">
        /// The identifier of the project config to find.
        /// </param>
        /// <param name="defaultValue">
        /// The value returned if there is no match for the given <paramref name="key"/>.
        /// </param>
        /// <returns>
        /// Return the integer value for the project config for the given <paramref name="key"/> if any;
        /// return <paramref name="defaultValue"/> otherwise.
        /// </returns>
        int GetInt(string key, int defaultValue = default);

        /// <summary>
        /// Get the float value for the project config with the given <paramref name="key"/>.
        /// </summary>
        /// <param name="key">
        /// The identifier of the project config to find.
        /// </param>
        /// <param name="defaultValue">
        /// The value returned if there is no match for the given <paramref name="key"/>.
        /// </param>
        /// <returns>
        /// Return the float value for the project config for the given <paramref name="key"/> if any;
        /// return <paramref name="defaultValue"/> otherwise.
        /// </returns>
        float GetFloat(string key, float defaultValue = default);

        /// <summary>
        /// Get the string value for the project config with the given <paramref name="key"/>.
        /// </summary>
        /// <param name="key">
        /// The identifier of the project config to find.
        /// </param>
        /// <param name="defaultValue">
        /// The value returned if there is no match for the given <paramref name="key"/>.
        /// </param>
        /// <returns>
        /// Return the string value for the project config for the given <paramref name="key"/> if any;
        /// return <paramref name="defaultValue"/> otherwise.
        /// </returns>
        string GetString(string key, string defaultValue = default);
    }
}
