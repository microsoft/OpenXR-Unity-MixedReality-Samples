using NotNull = JetBrains.Annotations.NotNullAttribute;
using CanBeNull = JetBrains.Annotations.CanBeNullAttribute;

namespace Unity.Services.Core.Internal
{
    interface IPackageRegistry
    {
        [CanBeNull]
        DependencyTree Tree { get; set; }

        CoreRegistration RegisterPackage<TPackage>(
            [NotNull] TPackage package)
            where TPackage : IInitializablePackage;

        void RegisterDependency<TComponent>(int packageTypeHash)
            where TComponent : IServiceComponent;

        void RegisterOptionalDependency<TComponent>(int packageTypeHash)
            where TComponent : IServiceComponent;

        void RegisterProvision<TComponent>(int packageTypeHash)
            where TComponent : IServiceComponent;
    }
}
