using System.Collections.Generic;

namespace Unity.Services.Core.Telemetry.Internal
{
    class DisabledDiagnosticsFactory : IDiagnosticsFactory
    {
        IReadOnlyDictionary<string, string> IDiagnosticsFactory.CommonTags { get; }
            = new Dictionary<string, string>();

        IDiagnostics IDiagnosticsFactory.Create(string packageName) => new DisabledDiagnostics();
    }
}
