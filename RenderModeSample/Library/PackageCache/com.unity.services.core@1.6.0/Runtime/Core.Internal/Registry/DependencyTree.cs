using System.Collections.Generic;

namespace Unity.Services.Core.Internal
{
    /// <summary>
    /// Contain dependency relations between <see cref="IInitializablePackage"/>
    /// types and <see cref="IServiceComponent"/> types using their hash code.
    /// </summary>
    class DependencyTree
    {
        /// <summary>
        /// Key: Hash code of a <see cref="IInitializablePackage"/> type.
        /// Value: Package instance.
        /// </summary>
        public readonly Dictionary<int, IInitializablePackage> PackageTypeHashToInstance;

        /// <summary>
        /// Key: Hash code of a <see cref="IServiceComponent"/> type.
        /// Value: Hash code of the <see cref="IInitializablePackage"/> type providing the component type.
        /// </summary>
        public readonly Dictionary<int, int> ComponentTypeHashToPackageTypeHash;

        /// <summary>
        /// Key: Hash code of the <see cref="IInitializablePackage"/> type.
        /// Value: Container of all hash code of <see cref="IServiceComponent"/>
        /// types required to initialize the package.
        /// </summary>
        public readonly Dictionary<int, List<int>> PackageTypeHashToComponentTypeHashDependencies;

        /// <summary>
        /// Key: Hash code of a <see cref="IServiceComponent"/> type.
        /// Value: Component instance.
        /// </summary>
        public readonly Dictionary<int, IServiceComponent> ComponentTypeHashToInstance;

        internal DependencyTree()
            : this(
                new Dictionary<int, IInitializablePackage>(),
                new Dictionary<int, int>(),
                new Dictionary<int, List<int>>(),
                new Dictionary<int, IServiceComponent>()) {}

        internal DependencyTree(
            Dictionary<int, IInitializablePackage> packageToInstance,
            Dictionary<int, int> componentToPackage,
            Dictionary<int, List<int>> packageToComponentDependencies,
            Dictionary<int, IServiceComponent> componentToInstance)
        {
            PackageTypeHashToInstance = packageToInstance;
            ComponentTypeHashToPackageTypeHash = componentToPackage;
            PackageTypeHashToComponentTypeHashDependencies = packageToComponentDependencies;
            ComponentTypeHashToInstance = componentToInstance;
        }
    }
}
