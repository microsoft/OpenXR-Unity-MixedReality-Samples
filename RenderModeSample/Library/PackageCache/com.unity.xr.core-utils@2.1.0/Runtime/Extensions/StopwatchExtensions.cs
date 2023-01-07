#if !NET_4_6
using System.Diagnostics;

namespace Unity.XR.CoreUtils
{
    /// <summary>
    /// Extension methods for Stopwatch objects
    /// </summary>
    public static class StopwatchExtensions
    {
        /// <summary>
        /// Restarts the stopwatch by stopping, resetting, and then starting it
        /// </summary>
        /// <param name="stopwatch">The stopwatch to restart</param>
        public static void Restart(this Stopwatch stopwatch)
        {
            stopwatch.Stop();
            stopwatch.Reset();
            stopwatch.Start();
        }
    }
}
#endif
