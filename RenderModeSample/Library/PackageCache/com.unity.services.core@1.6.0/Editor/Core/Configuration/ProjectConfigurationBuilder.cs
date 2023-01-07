using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Unity.Services.Core.Configuration.Editor
{
    class ProjectConfigurationBuilder
    {
        readonly IEnumerable<IConfigurationProvider> m_OrderedConfigProviders;

        public static ProjectConfigurationBuilder CreateBuilderWithAllProvidersInProject()
        {
            var allConfigProviderTypes = TypeCache.GetTypesDerivedFrom<IConfigurationProvider>();
            var orderedConfigProviders = CreateSortedConfigurationProviders(allConfigProviderTypes);
            return new ProjectConfigurationBuilder(orderedConfigProviders);
        }

        public ProjectConfigurationBuilder(IEnumerable<IConfigurationProvider> orderedConfigProviders)
        {
            m_OrderedConfigProviders = orderedConfigProviders;
        }

        internal static IEnumerable<IConfigurationProvider> CreateSortedConfigurationProviders(
            IEnumerable<Type> providerTypes)
        {
            return providerTypes.Where(
                    type => !type.IsAbstract && typeof(IConfigurationProvider).IsAssignableFrom(type))
                .Select(type => (IConfigurationProvider)Activator.CreateInstance(type))
                .OrderBy(prefs => prefs.callbackOrder)
                .ToArray();
        }

        [InitializeOnEnterPlayMode]
        internal static void SetUpPlayModeConfigOnEnteringPlayMode(EnterPlayModeOptions _)
        {
            var builderWithAllProviders = CreateBuilderWithAllProvidersInProject();
            ConfigurationUtils.ConfigurationLoader = new MemoryConfigurationLoader
            {
                Config = builderWithAllProviders.BuildConfiguration()
            };
        }

        public SerializableProjectConfiguration BuildConfiguration()
        {
            var builder = new ConfigurationBuilder();
            foreach (var provider in m_OrderedConfigProviders)
            {
                provider.OnBuildingConfiguration(builder);
            }

            return new SerializableProjectConfiguration(builder.Values);
        }
    }
}
