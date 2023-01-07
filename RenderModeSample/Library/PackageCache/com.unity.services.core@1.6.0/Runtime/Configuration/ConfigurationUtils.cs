namespace Unity.Services.Core.Configuration
{
    static class ConfigurationUtils
    {
        public const string ConfigFileName = "UnityServicesProjectConfiguration.json";

        public static IConfigurationLoader ConfigurationLoader { get; internal set; }
            = new StreamingAssetsConfigurationLoader();
    }
}
