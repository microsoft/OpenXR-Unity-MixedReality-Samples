using NUnit.Framework;
using System;

namespace UnityEngine.XR.OpenXR.Tests
{
    /// <summary>
    /// Custom yield instruction that waits for the OpenXRRestarter to finish shuting down the loader
    /// without restarting the loader.
    /// Note that this will wait until a loader shutdown has been performed even if a shutdown
    /// is not in progress when created.
    /// </summary>
    internal sealed class WaitForLoaderShutdown : CustomYieldInstruction
    {
        private float m_Timeout = 0;
        private Action m_OldAfterShutdown;
        private Action m_OldAfterRestart;
        private Action m_OldAfterCoroutine;
        private bool m_Shutdown;
        private bool m_Restarted;
        private bool m_Done;

        public WaitForLoaderShutdown(float timeout = 5.0f)
        {
            m_Timeout = Time.realtimeSinceStartup + timeout;

            var restarter = OpenXRRestarter.Instance;
            m_OldAfterShutdown = restarter.onAfterShutdown;
            m_OldAfterRestart = restarter.onAfterRestart;
            m_OldAfterCoroutine = restarter.onAfterCoroutine;

            restarter.onAfterShutdown = () =>
            {
                m_Shutdown = true;
                restarter.onAfterRestart = () => m_Restarted = true;
                restarter.onAfterCoroutine = () => m_Done = true;
            };
        }

        private void RestoreCallbacks ()
        {
            var restarter = OpenXRRestarter.Instance;
            restarter.onAfterShutdown = m_OldAfterShutdown;
            restarter.onAfterRestart = m_OldAfterRestart;
            restarter.onAfterCoroutine = m_OldAfterCoroutine;
        }

        public override bool keepWaiting
        {
            get
            {
                if (m_Done)
                {
                    if (!m_Shutdown)
                    {
                        Assert.Fail("WaitForLoaderShutdown: Coroutine finished without shutting down");
                    }
                    else if (m_Restarted)
                    {
                        Assert.Fail("WaitForLoaderShutdown: Waiting for shutdown but loader was restarted");
                    }

                    RestoreCallbacks();
                    return false;
                }

                // Did we time out waiting?
                if (Time.realtimeSinceStartup > m_Timeout)
                {
                    Assert.Fail("WaitForLoaderShutdown: Timeout");
                    RestoreCallbacks();
                    return false;
                }

                return true;
            }
        }
    }
}
