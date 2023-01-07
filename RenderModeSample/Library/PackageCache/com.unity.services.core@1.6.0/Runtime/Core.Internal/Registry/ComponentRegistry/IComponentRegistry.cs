using System.Collections.Generic;
using NotNull = JetBrains.Annotations.NotNullAttribute;

namespace Unity.Services.Core.Internal
{
    interface IComponentRegistry
    {
        void RegisterServiceComponent<TComponent>(
            [NotNull] TComponent component)
            where TComponent : IServiceComponent;

        TComponent GetServiceComponent<TComponent>()
            where TComponent : IServiceComponent;

        void ResetProvidedComponents(IDictionary<int, IServiceComponent> componentTypeHashToInstance);
    }
}
