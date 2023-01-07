using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using NotNull = JetBrains.Annotations.NotNullAttribute;

namespace Unity.Services.Core.Internal
{
    /// <summary>
    /// Utility to initialize all Unity services from a single endpoint.
    /// </summary>
    class UnityServicesInternal : IUnityServices
    {
        /// <summary>
        /// Initialization state.
        /// </summary>
        public ServicesInitializationState State { get; private set; }

        public InitializationOptions Options { get; internal set; }

        internal bool CanInitialize;

        TaskCompletionSource<object> m_Initialization;

        [NotNull]
        CoreRegistry Registry { get; }

        [NotNull]
        CoreMetrics Metrics { get; }

        [NotNull]
        CoreDiagnostics Diagnostics { get; }

        public UnityServicesInternal([NotNull] CoreRegistry registry, [NotNull] CoreMetrics metrics, [NotNull] CoreDiagnostics diagnostics)
        {
            Registry = registry;
            Metrics = metrics;
            Diagnostics = diagnostics;
        }

        /// <summary>
        /// Single entry point to initialize all used services.
        /// </summary>
        /// <param name="options">
        /// The options to customize services initialization.
        /// </param>
        /// <returns>
        /// Return a handle to the initialization operation.
        /// </returns>
        public async Task InitializeAsync(InitializationOptions options)
        {
            if (!HasRequestedInitialization()
                || HasInitializationFailed())
            {
                Options = options;
                m_Initialization = new TaskCompletionSource<object>();
            }

            if (!CanInitialize
                || State != ServicesInitializationState.Uninitialized)
            {
                await m_Initialization.Task;
            }
            else
            {
                await InitializeServicesAsync();
            }

            bool HasInitializationFailed()
            {
                return m_Initialization.Task.IsCompleted
                    && m_Initialization.Task.Status != TaskStatus.RanToCompletion;
            }
        }

        bool HasRequestedInitialization()
        {
            return !(m_Initialization is null);
        }

        async Task InitializeServicesAsync()
        {
            State = ServicesInitializationState.Initializing;
            var initStopwatch = new Stopwatch();
            initStopwatch.Start();

            var dependencyTree = Registry.PackageRegistry.Tree;
            if (dependencyTree is null)
            {
                var reason = new NullReferenceException("Services require a valid dependency tree to be initialized.");
                FailServicesInitialization(reason);
                throw reason;
            }

            var sortedPackageTypeHashes = new List<int>(dependencyTree.PackageTypeHashToInstance.Count);

            List<PackageInitializationInfo> packageInitInfos;
            try
            {
                SortPackages();
                packageInitInfos = await InitializePackagesAsync();
            }
            catch (Exception reason)
            {
                FailServicesInitialization(reason);
                throw;
            }

            SendInitializationMetrics(packageInitInfos);

            SucceedServicesInitialization();

            void SortPackages()
            {
                var sorter = new DependencyTreeInitializeOrderSorter(dependencyTree, sortedPackageTypeHashes);
                sorter.SortRegisteredPackagesIntoTarget();
            }

            async Task<List<PackageInitializationInfo>> InitializePackagesAsync()
            {
                var initializer = new CoreRegistryInitializer(Registry, sortedPackageTypeHashes);
                return await initializer.InitializeRegistryAsync();
            }

            void FailServicesInitialization(Exception reason)
            {
                State = ServicesInitializationState.Uninitialized;
                initStopwatch.Stop();
                m_Initialization.TrySetException(reason);

                if (reason is CircularDependencyException)
                {
                    Diagnostics.SendCircularDependencyDiagnostics(reason);
                }
                else
                {
                    Diagnostics.SendOperateServicesInitDiagnostics(reason);
                }
            }

            void SucceedServicesInitialization()
            {
                State = ServicesInitializationState.Initialized;
                Registry.PackageRegistry.Tree = null;
                Registry.LockComponentRegistration();
                initStopwatch.Stop();
                m_Initialization.TrySetResult(null);

                Metrics.SendAllPackagesInitSuccessMetric();
                Metrics.SendAllPackagesInitTimeMetric(initStopwatch.Elapsed.TotalSeconds);
            }
        }

        internal void SendInitializationMetrics(List<PackageInitializationInfo> packageInitInfos)
        {
            foreach (var initInfo in packageInitInfos)
            {
                Metrics.SendInitTimeMetricForPackage(initInfo.PackageType, initInfo.InitializationTimeInSeconds);
            }
        }

        internal async Task EnableInitializationAsync()
        {
            CanInitialize = true;

            Registry.LockPackageRegistration();

            if (!HasRequestedInitialization())
                return;

            await InitializeServicesAsync();
        }
    }
}
