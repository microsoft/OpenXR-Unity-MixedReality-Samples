using System;
using System.Linq;
using Unity.Services.Core.Configuration.Editor;
using Unity.Services.Core.Internal;
using UnityEditor.Build;

namespace Unity.Services.Core.Editor
{
    class CloudEnvironmentConfigProvider : IConfigurationProvider
    {
        const string k_CloudEnvironmentArg = "-cloudEnvironment";
        const string k_CloudEnvironmentKey = "com.unity.services.core.cloud-environment";
        const string k_StagingEnv = "staging";

        int IOrderedCallback.callbackOrder { get; }

        void IConfigurationProvider.OnBuildingConfiguration(ConfigurationBuilder builder)
        {
            SetCloudEnvironment(builder, GetCloudEnvironment());
        }

        internal string GetCloudEnvironment()
        {
            return GetCloudEnvironment(Environment.GetCommandLineArgs());
        }

        internal bool IsStaging()
        {
            return GetCloudEnvironment() == k_StagingEnv;
        }

        internal string GetCloudEnvironment(string[] commandLineArgs)
        {
            try
            {
                var cloudEnvironmentField = commandLineArgs.FirstOrDefault(x => x.StartsWith(k_CloudEnvironmentArg));

                if (cloudEnvironmentField != null)
                {
                    var cloudEnvironmentIndex = Array.IndexOf(commandLineArgs, cloudEnvironmentField);

                    if (cloudEnvironmentField == k_CloudEnvironmentArg)
                    {
                        if (cloudEnvironmentIndex <= commandLineArgs.Length - 2)
                        {
                            return commandLineArgs[cloudEnvironmentIndex + 1];
                        }
                    }
                    else if (cloudEnvironmentField.Contains('='))
                    {
                        var value = cloudEnvironmentField.Substring(cloudEnvironmentField.IndexOf('=') + 1);
                        return !string.IsNullOrEmpty(value) ? value : null;
                    }
                }
            }
            catch (Exception e)
            {
                CoreLogger.LogVerbose(e);
            }

            return null;
        }

        internal void SetCloudEnvironment(ConfigurationBuilder builder, string cloudEnvironment)
        {
            if (cloudEnvironment != null)
            {
                builder?.SetString(k_CloudEnvironmentKey, cloudEnvironment, false);
            }
        }
    }
}
