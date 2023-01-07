using System;

namespace Unity.Services.Core.Scheduler.Internal
{
    class UtcTimeProvider : ITimeProvider
    {
        public DateTime Now => DateTime.UtcNow;
    }
}
