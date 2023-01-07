using System;
using System.Collections.Generic;
using NotNull = JetBrains.Annotations.NotNullAttribute;

namespace Unity.Services.Core.Internal
{
    class LockedComponentRegistry : IComponentRegistry
    {
        const string k_ErrorMessage = "Component registration has been locked. " +
            "Make sure to register service components before all packages have finished initializing.";

        [NotNull]
        internal IComponentRegistry Registry { get; }

        public LockedComponentRegistry(
            [NotNull] IComponentRegistry registryToLock)
        {
            Registry = registryToLock;
        }

        public void RegisterServiceComponent<TComponent>(TComponent component)
            where TComponent : IServiceComponent
        {
            throw new InvalidOperationException(k_ErrorMessage);
        }

        public TComponent GetServiceComponent<TComponent>()
            where TComponent : IServiceComponent
        {
            return Registry.GetServiceComponent<TComponent>();
        }

        public void ResetProvidedComponents(IDictionary<int, IServiceComponent> componentTypeHashToInstance) => throw new InvalidOperationException(k_ErrorMessage);
    }
}
