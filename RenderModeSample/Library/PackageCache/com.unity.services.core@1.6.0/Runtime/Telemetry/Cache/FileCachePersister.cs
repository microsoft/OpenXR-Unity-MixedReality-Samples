using System;
using System.IO;
using Newtonsoft.Json;
using Unity.Services.Core.Internal;
using UnityEngine;
using NotNull = JetBrains.Annotations.NotNullAttribute;

namespace Unity.Services.Core.Telemetry.Internal
{
    abstract class FileCachePersister
    {
        internal static bool IsAvailableFor(RuntimePlatform platform)
        {
            return !string.IsNullOrEmpty(GetPersistentDataPathFor(platform));
        }

        internal static string GetPersistentDataPathFor(RuntimePlatform platform)
        {
            // Application.persistentDataPath has side effects on Switch so it shouldn't be called.
            if (platform == RuntimePlatform.Switch)
                return string.Empty;

            return Application.persistentDataPath;
        }
    }

    class FileCachePersister<TPayload> : FileCachePersister, ICachePersister<TPayload>
        where TPayload : ITelemetryPayload
    {
        public FileCachePersister(string fileName)
        {
            FilePath = Path.Combine(GetPersistentDataPathFor(Application.platform), fileName);
        }

        public string FilePath { get; }

        public bool CanPersist { get; } = IsAvailableFor(Application.platform);

        readonly string k_MultipleInstanceDiagnosticsName = "telemetry_cache_file_multiple_instances_exception";
        readonly string k_CacheFileException = "telemetry_cache_file_exception";
        readonly string k_MultipleInstanceError =
            "This exception is most likely caused by a multiple instance file sharing violation.";

        public void Persist(CachedPayload<TPayload> cache)
        {
            if (cache.IsEmpty())
            {
                return;
            }

            var serializedEvents = JsonConvert.SerializeObject(cache);

            try
            {
                File.WriteAllText(FilePath, serializedEvents);
            }
            catch (IOException e)
            {
                var exception = new IOException(k_MultipleInstanceError, e);
                CoreLogger.LogTelemetry(exception);
                CoreDiagnostics.Instance.SendCoreDiagnosticsAsync(k_MultipleInstanceDiagnosticsName, exception);
            }
            catch (Exception e)
            {
                CoreLogger.LogTelemetry(e);
                CoreDiagnostics.Instance.SendCoreDiagnosticsAsync(k_CacheFileException, e);
            }
        }

        public bool TryFetch(out CachedPayload<TPayload> persistedCache)
        {
            persistedCache = default;

            if (!File.Exists(FilePath))
            {
                return false;
            }

            try
            {
                var rawPersistedCache = File.ReadAllText(FilePath);
                persistedCache = JsonConvert.DeserializeObject<CachedPayload<TPayload>>(rawPersistedCache);
                return persistedCache != null;
            }
            catch (IOException e)
            {
                var exception = new IOException(k_MultipleInstanceError, e);
                CoreLogger.LogTelemetry(exception);
                CoreDiagnostics.Instance.SendCoreDiagnosticsAsync(k_MultipleInstanceDiagnosticsName, exception);
                return false;
            }
            catch (Exception e)
            {
                CoreLogger.LogTelemetry(e);
                CoreDiagnostics.Instance.SendCoreDiagnosticsAsync(k_CacheFileException, e);
                return false;
            }
        }

        public void Delete()
        {
            if (File.Exists(FilePath))
            {
                try
                {
                    File.Delete(FilePath);
                }
                catch (IOException e)
                {
                    var exception = new IOException(k_MultipleInstanceError, e);
                    CoreLogger.LogTelemetry(exception);
                    CoreDiagnostics.Instance.SendCoreDiagnosticsAsync(k_MultipleInstanceDiagnosticsName, exception);
                }
                catch (Exception e)
                {
                    CoreLogger.LogTelemetry(e);
                    CoreDiagnostics.Instance.SendCoreDiagnosticsAsync(k_CacheFileException, e);
                }
            }
        }
    }
}
