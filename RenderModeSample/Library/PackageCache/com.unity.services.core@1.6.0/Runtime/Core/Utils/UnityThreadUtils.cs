using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Unity.Services.Core
{
    static class UnityThreadUtils
    {
        static int s_UnityThreadId;

        internal static TaskScheduler UnityThreadScheduler { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void CaptureUnityThreadInfo()
        {
            s_UnityThreadId = Thread.CurrentThread.ManagedThreadId;
            UnityThreadScheduler = TaskScheduler.FromCurrentSynchronizationContext();
        }

        public static bool IsRunningOnUnityThread => Thread.CurrentThread.ManagedThreadId == s_UnityThreadId;
    }
}
