using System;
using NotNull = JetBrains.Annotations.NotNullAttribute;

namespace Unity.Services.Core.Internal
{
    class LockedPackageRegistry : IPackageRegistry
    {
        const string k_ErrorMessage = "Package registration has been locked. " +
            "Make sure to register service packages in" +
            "[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)].";

        [NotNull]
        internal IPackageRegistry Registry { get; }

        public LockedPackageRegistry(
            [NotNull] IPackageRegistry registryToLock)
        {
            Registry = registryToLock;
        }

        public DependencyTree Tree
        {
            get => Registry.Tree;
            set => Registry.Tree = value;
        }

        public CoreRegistration RegisterPackage<TPackage>(TPackage package)
            where TPackage : IInitializablePackage
        {
            throw new InvalidOperationException(k_ErrorMessage);
        }

        public void RegisterDependency<TComponent>(int packageTypeHash)
            where TComponent : IServiceComponent
        {
            throw new InvalidOperationException(k_ErrorMessage);
        }

        public void RegisterOptionalDependency<TComponent>(int packageTypeHash)
            where TComponent : IServiceComponent
        {
            throw new InvalidOperationException(k_ErrorMessage);
        }

        public void RegisterProvision<TComponent>(int packageTypeHash)
            where TComponent : IServiceComponent
        {
            throw new InvalidOperationException(k_ErrorMessage);
        }
    }
}
