using System;

namespace Unity.Services.Core.Scheduler.Internal
{
    class ScheduledInvocation
    {
        public Action Action;

        public DateTime InvocationTime;

        public long ActionId;
    }
}
