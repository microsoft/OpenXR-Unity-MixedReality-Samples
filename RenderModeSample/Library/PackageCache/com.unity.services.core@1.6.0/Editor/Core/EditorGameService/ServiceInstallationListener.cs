#if UNITY_2020_2_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Services.Core.Editor.ProjectBindRedirect;
using UnityEditor;
using UnityEditor.PackageManager;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace Unity.Services.Core.Editor
{
    static class ServiceInstallationListener
    {
        static IEditorGameServiceAnalyticsSender s_EditorGameServiceAnalyticsSender;

        static IEditorGameServiceAnalyticsSender EditorGameServiceAnalyticsSender
        {
            get
            {
                if (s_EditorGameServiceAnalyticsSender == null)
                {
                    s_EditorGameServiceAnalyticsSender = new EditorGameServiceAnalyticsSender();
                }

                return s_EditorGameServiceAnalyticsSender;
            }
        }

        [InitializeOnLoadMethod]
        static void RegisterToEvents()
        {
            Events.registeredPackages -= OnPackagesRegistered;
            Events.registeredPackages += OnPackagesRegistered;
        }

        static void OnPackagesRegistered(PackageRegistrationEventArgs args)
        {
            if (args.added.Any())
            {
                OnPackagesAdded(args.added);
            }
        }

        static void OnPackagesAdded(IEnumerable<PackageInfo> packageInfos)
        {
            var newServices = GetNewServices(packageInfos);
            var gameServices = newServices.ToList();
            if (!gameServices.Any())
            {
                return;
            }

            var request = new ProjectStateRequest();
            var projectState = request.GetProjectState();
            if (!ShouldShowRedirect(projectState))
            {
                return;
            }

            var installedPackages = gameServices.Select(service => service.Name).ToList();
            ProjectBindRedirectPopupWindow.CreateAndShowPopup(installedPackages, EditorGameServiceAnalyticsSender);
        }

        internal static bool ShouldShowRedirect(ProjectState projectState)
        {
            return !projectState.ProjectBound || !projectState.IsLoggedIn;
        }

        static IEnumerable<IEditorGameService> GetNewServices(IEnumerable<PackageInfo> packageInfos)
        {
            var serviceTypes = TypeCache.GetTypesDerivedFrom<IEditorGameService>();
            var packages = packageInfos.ToList();
            var editorGameServices = EditorGameServiceRegistry.Instance.Services.Values;
            var newServices = serviceTypes.Where(serviceType => IsTypeDefinedInPackages(serviceType, packages))
                .Select(serviceType => editorGameServices.Where(editorGameService => editorGameService.GetType() == serviceType))
                .SelectMany(services => services);

            return new HashSet<IEditorGameService>(newServices);
        }

        static bool IsTypeDefinedInPackages(Type type, IEnumerable<PackageInfo> packageInfos)
        {
            return packageInfos.Any(packageInfo => IsTypeDefinedInPackage(type, packageInfo));
        }

        static bool IsTypeDefinedInPackage(Type type, PackageInfo packageInfo)
        {
            var packageInfoFromAssembly = PackageInfo.FindForAssembly(type.Assembly);
            return ArePackageInfosEqual(packageInfoFromAssembly, packageInfo);
        }

        static bool ArePackageInfosEqual(PackageInfo x, PackageInfo y)
        {
            return x != null && y != null && x.name == y.name;
        }
    }
}
#endif
