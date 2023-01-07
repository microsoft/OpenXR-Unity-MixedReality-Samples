using System.Collections.Generic;
using CanBeNull = JetBrains.Annotations.CanBeNullAttribute;

namespace Unity.Services.Core.Internal
{
    class PackageRegistry : IPackageRegistry
    {
        public DependencyTree Tree { get; set; }

        public PackageRegistry(
            [CanBeNull] DependencyTree tree)
        {
            Tree = tree;
        }

        public CoreRegistration RegisterPackage<TPackage>(TPackage package)
            where TPackage : IInitializablePackage
        {
            var packageTypeHash = typeof(TPackage).GetHashCode();
            Tree.PackageTypeHashToInstance[packageTypeHash] = package;
            Tree.PackageTypeHashToComponentTypeHashDependencies[packageTypeHash] = new List<int>();

            return new CoreRegistration(this, packageTypeHash);
        }

        public void RegisterDependency<TComponent>(int packageTypeHash)
            where TComponent : IServiceComponent
        {
            var intendedType = typeof(TComponent);
            var componentTypeHash = intendedType.GetHashCode();
            Tree.ComponentTypeHashToInstance[componentTypeHash] = new MissingComponent(intendedType);

            AddComponentDependencyToPackage(componentTypeHash, packageTypeHash);
        }

        public void RegisterOptionalDependency<TComponent>(int packageTypeHash)
            where TComponent : IServiceComponent
        {
            var componentTypeHash = typeof(TComponent).GetHashCode();
            if (!Tree.ComponentTypeHashToInstance.ContainsKey(componentTypeHash))
            {
                Tree.ComponentTypeHashToInstance[componentTypeHash] = null;
            }

            AddComponentDependencyToPackage(componentTypeHash, packageTypeHash);
        }

        public void RegisterProvision<TComponent>(int packageTypeHash)
            where TComponent : IServiceComponent
        {
            var componentTypeHash = typeof(TComponent).GetHashCode();
            Tree.ComponentTypeHashToPackageTypeHash[componentTypeHash] = packageTypeHash;
        }

        void AddComponentDependencyToPackage(int componentTypeHash, int packageTypeHash)
        {
            var dependencyTypeHashs = Tree.PackageTypeHashToComponentTypeHashDependencies[packageTypeHash];
            if (!dependencyTypeHashs.Contains(componentTypeHash))
            {
                dependencyTypeHashs.Add(componentTypeHash);
            }
        }
    }
}
