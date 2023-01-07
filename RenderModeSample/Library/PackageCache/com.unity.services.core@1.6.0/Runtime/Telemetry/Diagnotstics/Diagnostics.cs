using System;
using System.Collections.Generic;
using Unity.Services.Core.Internal;

namespace Unity.Services.Core.Telemetry.Internal
{
    class Diagnostics : IDiagnostics
    {
        internal const int MaxDiagnosticMessageLength = 10000;
        internal const string DiagnosticMessageTruncateSuffix = "[truncated]";
        internal DiagnosticsHandler Handler { get; }

        internal IDictionary<string, string> PackageTags { get; }

        public Diagnostics(DiagnosticsHandler handler, IDictionary<string, string> packageTags)
        {
            Handler = handler;
            PackageTags = packageTags;
        }

        public void SendDiagnostic(string name, string message, IDictionary<string, string> tags = null)
        {
            var diagnostic = new Diagnostic
            {
                Content = tags is null
                    ? new Dictionary<string, string>(PackageTags)
                    : new Dictionary<string, string>(tags)
                        .MergeAllowOverride(PackageTags),
            };

            diagnostic.Content.Add(TagKeys.DiagnosticName, name);
            if (message != null && message.Length > MaxDiagnosticMessageLength)
            {
                message = $"{message.Substring(0, MaxDiagnosticMessageLength)}{Environment.NewLine}{DiagnosticMessageTruncateSuffix}";
            }
            diagnostic.Content.Add(TagKeys.DiagnosticMessage, message);


            Handler.Register(diagnostic);
        }
    }
}
