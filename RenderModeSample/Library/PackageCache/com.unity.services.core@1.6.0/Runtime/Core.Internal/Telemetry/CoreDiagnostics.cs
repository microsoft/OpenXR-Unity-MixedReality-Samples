using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Core.Telemetry.Internal;

namespace Unity.Services.Core.Internal
{
    class CoreDiagnostics
    {
        internal const string CorePackageName = "com.unity.services.core";

        internal const string CircularDependencyDiagnosticName = "circular_dependency";

        internal const string CorePackageInitDiagnosticName = "core_package_init";

        internal const string OperateServicesInitDiagnosticName = "operate_services_init";

        internal const string ProjectConfigTagName = "project_config";

        public static CoreDiagnostics Instance { get; internal set; }

        public IDictionary<string, string> CoreTags { get; }
            = new Dictionary<string, string>();

        internal IDiagnosticsComponentProvider DiagnosticsComponentProvider { get; set; }

        internal IDiagnostics Diagnostics { get; set; }

        public void SetProjectConfiguration(string serializedProjectConfig)
        {
            CoreTags[ProjectConfigTagName] = serializedProjectConfig;
        }

        public void SendCircularDependencyDiagnostics(Exception exception)
        {
            var sendTask = SendCoreDiagnosticsAsync(CircularDependencyDiagnosticName, exception);
            sendTask.ContinueWith(OnSendFailed, TaskContinuationOptions.OnlyOnFaulted);
        }

        public void SendCorePackageInitDiagnostics(Exception exception)
        {
            var sendTask = SendCoreDiagnosticsAsync(CorePackageInitDiagnosticName, exception);
            sendTask.ContinueWith(OnSendFailed, TaskContinuationOptions.OnlyOnFaulted);
        }

        public void SendOperateServicesInitDiagnostics(Exception exception)
        {
            var sendTask = SendCoreDiagnosticsAsync(OperateServicesInitDiagnosticName, exception);
            sendTask.ContinueWith(OnSendFailed, TaskContinuationOptions.OnlyOnFaulted);
        }

        internal async Task SendCoreDiagnosticsAsync(string diagnosticName, Exception exception)
        {
            var diagnostics = await GetOrCreateDiagnosticsAsync();
            diagnostics?.SendDiagnostic(diagnosticName, exception?.ToString(), CoreTags);
        }

        static void OnSendFailed(Task failedSendTask)
        {
            CoreLogger.LogException(failedSendTask.Exception);
        }

        internal async Task<IDiagnostics> GetOrCreateDiagnosticsAsync()
        {
            if (!(Diagnostics is null))
            {
                return Diagnostics;
            }

            if (DiagnosticsComponentProvider is null)
            {
                CoreLogger.LogVerbose(
                    $"There is no {nameof(DiagnosticsComponentProvider)} set for {nameof(CoreDiagnostics)}.");
                return null;
            }

            var diagnosticFactory = await DiagnosticsComponentProvider.CreateDiagnosticsComponents();
            Diagnostics = diagnosticFactory.Create(CorePackageName);
            SetProjectConfiguration(await DiagnosticsComponentProvider.GetSerializedProjectConfigurationAsync());

            return Diagnostics;
        }
    }
}
