using System;

namespace Unity.Services.Core.Telemetry.Internal
{
    class DisabledCachePersister<TPayload> : ICachePersister<TPayload>
        where TPayload : ITelemetryPayload
    {
        const string k_ErrorMessage = "Cache persistence isn't supported on the current platform.";

        public bool CanPersist => false;

        public void Persist(CachedPayload<TPayload> cache) => throw new NotSupportedException(k_ErrorMessage);

        public bool TryFetch(out CachedPayload<TPayload> persistedCache)
            => throw new NotSupportedException(k_ErrorMessage);

        public void Delete() => throw new NotSupportedException(k_ErrorMessage);
    }
}
