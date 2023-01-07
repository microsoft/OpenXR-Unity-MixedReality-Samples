using System.Collections.Generic;

namespace Unity.Services.Core.Scheduler.Internal
{
    class ScheduledInvocationComparer : IComparer<ScheduledInvocation>
    {
        public int Compare(ScheduledInvocation x, ScheduledInvocation y)
        {
            if (ReferenceEquals(x, y))
            {
                return 0;
            }

            if (ReferenceEquals(null, y))
            {
                return 1;
            }

            if (ReferenceEquals(null, x))
            {
                return -1;
            }

            var compareResult = x.InvocationTime.CompareTo(y.InvocationTime);

            // Actions with same invocation time will execute in id order (schedule order).
            if (compareResult == 0)
            {
                compareResult = x.ActionId.CompareTo(y.ActionId);
            }

            return compareResult;
        }
    }
}
