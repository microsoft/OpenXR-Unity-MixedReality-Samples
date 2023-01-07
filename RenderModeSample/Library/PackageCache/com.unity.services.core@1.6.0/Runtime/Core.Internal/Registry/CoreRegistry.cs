using System.Collections.Generic;
using NotNull = JetBrains.Annotations.NotNullAttribute;
using SuppressMessage = System.Diagnostics.CodeAnalysis.SuppressMessageAttribute;

namespace Unity.Services.Core.Internal
{
    /// <summary>
    /// A container to store all available <see cref="IInitializablePackage"/>
    /// and <see cref="IServiceComponent"/> in the project.
    /// </summary>
    public sealed class CoreRegistry
    {
        /// <summary>
        /// Get the only registry of this project.
        /// </summary>
        public static CoreRegistry Instance { get; internal set; }

        [NotNull]
        internal IPackageRegistry PackageRegistry { get; private set; }

        [NotNull]
        internal IComponentRegistry ComponentRegistry { get; private set; }

        internal CoreRegistry()
        {
            var dependencyTree = new DependencyTree();
            PackageRegistry = new PackageRegistry(dependencyTree);
            var componentTypeHashToInstance = new Dictionary<int, IServiceComponent>(
                dependencyTree.ComponentTypeHashToInstance.Count);
            ComponentRegistry = new ComponentRegistry(componentTypeHashToInstance);
        }

        internal CoreRegistry(
            [NotNull] IPackageRegistry packageRegistry,
            [NotNull] IComponentRegistry componentRegistry)
        {
            PackageRegistry = packageRegistry;
            ComponentRegistry = componentRegistry;
        }

        /// <summary>
        /// Store the given <paramref name="package"/> in this registry.
        /// </summary>
        /// <param name="package">
        /// The service package instance to register.
        /// </param>
        /// <typeparam name="TPackage">
        /// The type of <see cref="IInitializablePackage"/> to register.
        /// </typeparam>
        /// <returns>
        /// Return a handle to the registered <paramref name="package"/>
        /// to define its dependencies and provided components.
        /// </returns>
        public CoreRegistration RegisterPackage<TPackage>(
            [NotNull] TPackage package)
            where TPackage : IInitializablePackage
        {
            return PackageRegistry.RegisterPackage(package);
        }

        /// <summary>
        /// Store the given <paramref name="component"/> in this registry.
        /// </summary>
        /// <param name="component">
        /// The component instance to register.
        /// </param>
        /// <typeparam name="TComponent">
        /// The type of <see cref="IServiceComponent"/> to register.
        /// </typeparam>
        [SuppressMessage("ReSharper", "RedundantTypeArgumentsOfMethod")]
        public void RegisterServiceComponent<TComponent>(
            [NotNull] TComponent component)
            where TComponent : IServiceComponent
        {
            ComponentRegistry.RegisterServiceComponent<TComponent>(component);
        }

        /// <summary>
        /// Get the instance of the given <see cref="IServiceComponent"/> type.
        /// </summary>
        /// <typeparam name="TComponent">
        /// The type of <see cref="IServiceComponent"/> to get.
        /// </typeparam>
        /// <returns>
        /// Return the instance of the given <see cref="IServiceComponent"/> type if it has been registered;
        /// throws an exception otherwise.
        /// </returns>
        /// <exception cref="KeyNotFoundException">
        /// Thrown if the requested type of <typeparamref name="TComponent"/> hasn't been registered yet.
        /// </exception>
        public TComponent GetServiceComponent<TComponent>()
            where TComponent : IServiceComponent
        {
            return ComponentRegistry.GetServiceComponent<TComponent>();
        }

        internal void LockPackageRegistration()
        {
            if (PackageRegistry is LockedPackageRegistry)
            {
                return;
            }

            PackageRegistry = new LockedPackageRegistry(PackageRegistry);
        }

        internal void LockComponentRegistration()
        {
            if (ComponentRegistry is LockedComponentRegistry)
            {
                return;
            }

            ComponentRegistry = new LockedComponentRegistry(ComponentRegistry);
        }
    }
}
