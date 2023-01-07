using System;
using Unity.Services.Core.Internal;

namespace Unity.Services.Core.Scheduler.Internal
{
    /// <summary>
    /// Unity Service Scheduler to schedule actions on main thread.
    /// </summary>
    public interface IActionScheduler : IServiceComponent
    {
        /// <summary>
        /// Schedules the action to be invoked on the main thead, on the first frame that occurs after the given delay in seconds
        /// </summary>
        /// <param name="action">
        /// Action to be scheduled.
        /// </param>
        /// <param name="delaySeconds">
        /// time in seconds to delay execute action
        /// </param>
        /// <returns>unique Id for the scheduled action</returns>
        long ScheduleAction(Action action, double delaySeconds = 0);

        /// <summary>
        /// Removes all instances of the given action from the queue
        /// </summary>
        /// <param name="actionId">
        /// unique Id for action to be canceled.
        /// </param>
        void CancelAction(long actionId);
    }
}
