using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Unity.Services.Core.Configuration.Internal;
using Unity.Services.Core.Environments.Internal;
using Unity.Services.Core.Scheduler.Internal;
using UnityEngine;

namespace Unity.Services.Core.Telemetry.Internal
{
    /// <summary>
    /// Handles common logic between all <see cref="Diagnostics"/> instances.
    /// </summary>
    class DiagnosticsHandler : TelemetryHandler<DiagnosticsPayload, Diagnostic>
    {
        class SendState
        {
            public DiagnosticsHandler Self;

            public CachedPayload<DiagnosticsPayload> Payload;
        }

        public DiagnosticsHandler(
            TelemetryConfig config, CachedPayload<DiagnosticsPayload> cache, IActionScheduler scheduler,
            ICachePersister<DiagnosticsPayload> cachePersister, TelemetrySender sender)
            : base(config, cache, scheduler, cachePersister, sender) {}

        internal override void SendPersistedCache(CachedPayload<DiagnosticsPayload> persistedCache)
        {
            var sendAsync = m_Sender.SendAsync(persistedCache.Payload);

            m_CachePersister.Delete();

            var localState = new SendState
            {
                Self = this,
                Payload = new CachedPayload<DiagnosticsPayload>
                {
                    TimeOfOccurenceTicks = persistedCache.TimeOfOccurenceTicks,
                    Payload = persistedCache.Payload,
                },
            };
            sendAsync.ContinueWith(OnSendAsyncCompleted, localState, TaskContinuationOptions.ExecuteSynchronously);
        }

        static void OnSendAsyncCompleted(Task sendOperation, object state)
        {
            if (!(state is SendState castState))
            {
                throw new ArgumentException("The given state is invalid.");
            }

            switch (sendOperation.Status)
            {
                case TaskStatus.Canceled:
                case TaskStatus.Faulted:
                {
                    castState.Self.Cache.AddRangeFrom(castState.Payload);
                    break;
                }
                case TaskStatus.RanToCompletion:
                {
                    break;
                }

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(sendOperation.Status), "Can't continue without the send operation being completed.");
            }
        }

        internal override void FetchSpecificCommonTags(ICloudProjectId cloudProjectId, IEnvironments environments)
        {
            var commonTags = Cache.Payload.DiagnosticsCommonTags;
            commonTags.Clear();

            commonTags[TagKeys.ApplicationVersion] = Application.version;
            commonTags[TagKeys.ProductName] = Application.productName;
            commonTags[TagKeys.CloudProjectId] = cloudProjectId.GetCloudProjectId();
            commonTags[TagKeys.EnvironmentName] = environments.Current;
            commonTags[TagKeys.ApplicationGenuine] = Application.genuineCheckAvailable
                ? Application.genuine.ToString(CultureInfo.InvariantCulture)
                : "unavailable";
            commonTags[TagKeys.InternetReachability] = Application.internetReachability.ToString();
        }

        internal override void SendCachedPayload()
        {
            if (Cache.IsEmpty())
                return;

            var sendAsync = m_Sender.SendAsync(Cache.Payload);

            var localState = new SendState
            {
                Self = this,
                Payload = new CachedPayload<DiagnosticsPayload>
                {
                    TimeOfOccurenceTicks = Cache.TimeOfOccurenceTicks,
                    Payload = new DiagnosticsPayload
                    {
                        Diagnostics = new List<Diagnostic>(Cache.Payload.Diagnostics),
                        CommonTags = new Dictionary<string, string>(Cache.Payload.CommonTags),
                        DiagnosticsCommonTags = new Dictionary<string, string>(Cache.Payload.DiagnosticsCommonTags),
                    },
                },
            };
            Cache.TimeOfOccurenceTicks = 0;
            Cache.Payload.Diagnostics.Clear();

            if (m_CachePersister.CanPersist)
            {
                m_CachePersister.Delete();
            }

            sendAsync.ContinueWith(OnSendAsyncCompleted, localState, TaskContinuationOptions.ExecuteSynchronously);
        }
    }
}
