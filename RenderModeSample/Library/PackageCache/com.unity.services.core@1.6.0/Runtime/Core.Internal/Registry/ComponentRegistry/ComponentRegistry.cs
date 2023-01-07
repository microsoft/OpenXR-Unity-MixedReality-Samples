using System;
using System.Collections.Generic;
using NotNull = JetBrains.Annotations.NotNullAttribute;

namespace Unity.Services.Core.Internal
{
    class ComponentRegistry : IComponentRegistry
    {
        /// <summary>
        /// Key: Hash code of a <see cref="IServiceComponent"/> type.
        /// Value: Component instance.
        /// </summary>
        [NotNull]
        internal Dictionary<int, IServiceComponent> ComponentTypeHashToInstance { get; }

        public ComponentRegistry(
            [NotNull] Dictionary<int, IServiceComponent> componentTypeHashToInstance)
        {
            ComponentTypeHashToInstance = componentTypeHashToInstance;
        }

        public void RegisterServiceComponent<TComponent>(TComponent component)
            where TComponent : IServiceComponent
        {
            var componentType = typeof(TComponent);

            // This check is to avoid passing the component without specifying the interface type as a generic argument.
            if (component.GetType() == componentType)
            {
                throw new ArgumentException("Interface type of component not specified.");
            }

            var componentTypeHash = componentType.GetHashCode();
            if (IsComponentTypeRegistered(componentTypeHash))
            {
                throw new InvalidOperationException(
                    $"A component with the type {componentType.FullName} has already been registered.");
            }

            ComponentTypeHashToInstance[componentTypeHash] = component;
        }

        public TComponent GetServiceComponent<TComponent>()
            where TComponent : IServiceComponent
        {
            var componentType = typeof(TComponent);
            if (!ComponentTypeHashToInstance.TryGetValue(componentType.GetHashCode(), out var component)
                || component is MissingComponent)

            {
                throw new KeyNotFoundException($"There is no component `{componentType.Name}` registered. " +
                    "Are you missing a package?");
            }

            return (TComponent)component;
        }

        bool IsComponentTypeRegistered(int componentTypeHash)
        {
            return ComponentTypeHashToInstance.TryGetValue(componentTypeHash, out var storedComponent)
                && !(storedComponent is null)
                && !(storedComponent is MissingComponent);
        }

        public void ResetProvidedComponents(IDictionary<int, IServiceComponent> componentTypeHashToInstance)
        {
            ComponentTypeHashToInstance.Clear();
            ComponentTypeHashToInstance.MergeAllowOverride(componentTypeHashToInstance);
        }
    }
}
