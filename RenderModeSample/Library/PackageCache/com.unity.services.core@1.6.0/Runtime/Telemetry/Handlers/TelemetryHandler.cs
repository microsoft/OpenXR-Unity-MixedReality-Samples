#if UNITY_ANDROID && !UNITY_EDITOR
using System;
#endif
using Newtonsoft.Json;
using Unity.Services.Core.Configuration.Internal;
using Unity.Services.Core.Environments.Internal;
using Unity.Services.Core.Internal;
using Unity.Services.Core.Scheduler.Internal;
using UnityEngine;

namespace Unity.Services.Core.Telemetry.Internal
{
    /// <remarks>
    /// A non-generic version of the class to hold non-generic static code in order to
    /// avoid unnecessary duplication that happens with static members in generic classes.
    /// </remarks>
    abstract class TelemetryHandler
    {
        internal static string FormatOperatingSystemInfo(string rawOsInfo)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            //Android's os data is formatted as follow:
            //"<Device system name> <Device system version> / API-<API level> (<ID>/<Version incremental>)"
            //eg. "Android OS 10 / API-29 (HONORHRY-LX1T/10.0.0.200C636)"
            var trimmedOsInfoSize = rawOsInfo.LastIndexOf(" (", StringComparison.Ordinal);
            if (trimmedOsInfoSize < 0)
                return rawOsInfo;

            var osTag = rawOsInfo.Substring(0, trimmedOsInfoSize);
            return osTag;
#else
            return rawOsInfo;
#endif
        }
    }

    abstract class TelemetryHandler<TPayload, TEvent> : TelemetryHandler
        where TPayload : ITelemetryPayload
        where TEvent : ITelemetryEvent
    {
        readonly IActionScheduler m_Scheduler;

        protected readonly ICachePersister<TPayload> m_CachePersister;

        protected readonly TelemetrySender m_Sender;

        internal long SendingLoopScheduleId;

        internal long PersistenceLoopScheduleId;

        public TelemetryConfig Config { get; }

        public CachedPayload<TPayload> Cache { get; }

        protected TelemetryHandler(
            TelemetryConfig config, CachedPayload<TPayload> cache, IActionScheduler scheduler,
            ICachePersister<TPayload> cachePersister, TelemetrySender sender)
        {
            Config = config;
            Cache = cache;
            m_Scheduler = scheduler;
            m_CachePersister = cachePersister;
            m_Sender = sender;
        }

        public void Initialize(ICloudProjectId cloudProjectId, IEnvironments environments)
        {
            HandlePersistedCache();
            FetchAllCommonTags(cloudProjectId, environments);
            ScheduleSendingLoop();

            if (m_CachePersister.CanPersist)
            {
                SchedulePersistenceLoop();
            }
        }

        internal void HandlePersistedCache()
        {
            if (!m_CachePersister.CanPersist
                || !m_CachePersister.TryFetch(out var persistedCache))
                return;

            if (persistedCache.IsEmpty())
            {
                m_CachePersister.Delete();
                return;
            }

            SendPersistedCache(persistedCache);
        }

        internal abstract void SendPersistedCache(CachedPayload<TPayload> persistedCache);

        void FetchAllCommonTags(ICloudProjectId cloudProjectId, IEnvironments environments)
        {
            FetchTelemetryCommonTags();
            FetchSpecificCommonTags(cloudProjectId, environments);
        }

        internal abstract void FetchSpecificCommonTags(ICloudProjectId cloudProjectId, IEnvironments environments);

        internal void FetchTelemetryCommonTags()
        {
            var commonTags = Cache.Payload.CommonTags;
            commonTags.Clear();
            commonTags[TagKeys.ApplicationInstallMode] = Application.installMode.ToString();
            commonTags[TagKeys.OperatingSystem] = FormatOperatingSystemInfo(SystemInfo.operatingSystem);
            commonTags[TagKeys.Platform] = Application.platform.ToString();
            commonTags[TagKeys.Engine] = "Unity";
            commonTags[TagKeys.UnityVersion] = Application.unityVersion;
        }

        internal void ScheduleSendingLoop()
        {
            SendingLoopScheduleId = m_Scheduler.ScheduleAction(SendingLoop, Config.PayloadSendingMaxIntervalSeconds);

            void SendingLoop()
            {
                ScheduleSendingLoop();
                SendCachedPayload();
            }
        }

        internal abstract void SendCachedPayload();

        internal void SchedulePersistenceLoop()
        {
            PersistenceLoopScheduleId = m_Scheduler.ScheduleAction(
                PersistenceLoop, Config.SafetyPersistenceIntervalSeconds);

            void PersistenceLoop()
            {
                SchedulePersistenceLoop();
                PersistCache();
            }
        }

        internal void PersistCache()
        {
            if (!m_CachePersister.CanPersist
                || Cache.TimeOfOccurenceTicks <= 0
                || Cache.Payload.Count <= 0)
                return;

            m_CachePersister.Persist(Cache);
        }

        public void Register(TEvent telemetryEvent)
        {
            CoreLogger.LogTelemetry(
                $"Cached the {typeof(TEvent).Name} event: {JsonConvert.SerializeObject(telemetryEvent)}");
            Cache.Add(telemetryEvent);

            if (!IsCacheFull())
                return;

            SendCachedPayload();
            m_Scheduler.CancelAction(SendingLoopScheduleId);
            ScheduleSendingLoop();

            bool IsCacheFull()
            {
                return Cache.Payload.Count >= Config.MaxMetricCountPerPayload;
            }
        }
    }
}
