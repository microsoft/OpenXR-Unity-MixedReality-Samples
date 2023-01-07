using System;

namespace Unity.Services.Core.Scheduler.Internal
{
    interface ITimeProvider
    {
        DateTime Now { get; }
    }
}
