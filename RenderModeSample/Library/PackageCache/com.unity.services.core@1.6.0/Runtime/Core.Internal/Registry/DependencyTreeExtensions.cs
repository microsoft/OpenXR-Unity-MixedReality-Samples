using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Unity.Services.Core.Internal
{
    internal static class DependencyTreeExtensions
    {
        internal static string ToJson(this DependencyTree tree, ICollection<int> order = null)
        {
            var orderArray = new JArray();
            var orderProperty = new JProperty("ordered", orderArray);
            if (order != null)
            {
                foreach (var packageHash in order)
                {
                    var packageJObject = GetPackageJObject(tree, packageHash);
                    orderArray.Add(new JObject(packageJObject));
                }
            }
            var packageTree = new JArray();
            var packagesProperty = new JProperty("packages", packageTree);
            foreach (var packageHash in tree.PackageTypeHashToInstance.Keys)
            {
                var package = GetPackageJObject(tree, packageHash);
                packageTree.Add(package);
            }
            var components = new JArray();
            var componentsProperty = new JProperty("components", components);
            foreach (var componentHash in tree.ComponentTypeHashToInstance.Keys)
            {
                var componentJObject = GetComponentJObject(tree, componentHash);
                components.Add(componentJObject);
            }
            var json = new JObject(orderProperty, packagesProperty, componentsProperty);
            return json.ToString();
        }

        internal static bool IsOptional(this DependencyTree tree, int componentTypeHash)
        {
            return tree.ComponentTypeHashToInstance.TryGetValue(componentTypeHash, out var component)
                && component is null;
        }

        internal static bool IsProvided(this DependencyTree tree, int componentTypeHash)
        {
            return tree.ComponentTypeHashToPackageTypeHash.ContainsKey(componentTypeHash);
        }

        private static JObject GetPackageJObject(DependencyTree tree, int packageHash)
        {
            var packageHashProperty = new JProperty("packageHash", packageHash);
            tree.PackageTypeHashToInstance.TryGetValue(packageHash, out var packageProvider);
            var packageProviderProperty = new JProperty("packageProvider", packageProvider != null ? packageProvider.GetType().Name : "null");
            var packageDependencies = new JArray();
            var packageDependenciesProperty = new JProperty("packageDependencies", packageDependencies);
            if (tree.PackageTypeHashToComponentTypeHashDependencies.TryGetValue(packageHash, out var componentDependencies))
            {
                foreach (var componentDependency in componentDependencies)
                {
                    var dependencyHash = new JProperty("dependencyHash", componentDependency);
                    tree.ComponentTypeHashToInstance.TryGetValue(componentDependency, out var componentInstance);
                    var dependencyComponent = new JProperty("dependencyComponent", GetComponentIdentifier(componentInstance));
                    var dependencyProvided = new JProperty("dependencyProvided", tree.IsProvided(componentDependency) ? "true" : "false");
                    var dependencyOptional = new JProperty("dependencyOptional", tree.IsOptional(componentDependency) ? "true" : "false");
                    var dependencyJObject = new JObject(dependencyHash, dependencyComponent, dependencyProvided, dependencyOptional);
                    packageDependencies.Add(dependencyJObject);
                }
            }
            return new JObject(packageHashProperty, packageProviderProperty, packageDependenciesProperty);
        }

        private static JObject GetComponentJObject(DependencyTree tree, int componentHash)
        {
            var componentHashProperty = new JProperty("componentHash", componentHash);
            tree.ComponentTypeHashToInstance.TryGetValue(componentHash, out var component);
            var componentProperty = new JProperty("component", GetComponentIdentifier(component));
            tree.ComponentTypeHashToPackageTypeHash.TryGetValue(componentHash, out var packageHash);
            var componentPackageHash = new JProperty("componentPackageHash", packageHash);
            var hasPackage = tree.PackageTypeHashToInstance.TryGetValue(packageHash, out var packageInstance);
            var componentPackage = new JProperty("componentPackage", hasPackage ? packageInstance.GetType().Name : "null");
            return new JObject(componentHashProperty, componentProperty, componentPackageHash, componentPackage);
        }

        private static string GetComponentIdentifier(IServiceComponent component)
        {
            if (component == null)
            {
                return "null";
            }
            if (component is MissingComponent missingComponent)
            {
                return missingComponent.IntendedType.Name;
            }
            return component.GetType().Name;
        }
    }
}
