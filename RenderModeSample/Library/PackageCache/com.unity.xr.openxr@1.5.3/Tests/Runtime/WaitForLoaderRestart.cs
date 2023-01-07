using System;
using NUnit.Framework;

namespace UnityEngine.XR.OpenXR.Tests
{
    /// <summary>
    /// Custom yield instruction that waits for the OpenXRRestarter to restart the loader.
    /// Note that this will wait until a loader restart has been performed even if a restart
    /// is not in progress when created.
    /// </summary>
    internal sealed class WaitForLoaderRestart : CustomYieldInstruction
    {
        private float m_Timeout = 0;
        private Action m_OldAfterRestart;
        private Action m_OldAfterCoroutine;
        private Action m_OldAfterSuccessfulRestart;
        private bool m_Done;

        public WaitForLoaderRestart(float timeout = 5.0f, bool mustBeSuccessfulRestart = false)
        {
            m_Timeout = Time.realtimeSinceStartup + timeout;

            var restarter = OpenXRRestarter.Instance;
            m_OldAfterRestart = restarter.onAfterRestart;
            m_OldAfterCoroutine = restarter.onAfterCoroutine;
            m_OldAfterSuccessfulRestart = restarter.onAfterSuccessfulRestart;

            if (mustBeSuccessfulRestart)
            {
                // Wait for a successful restart, then wait for that particular coroutine to finish.
                restarter.onAfterSuccessfulRestart = () =>
                {
                    restarter.onAfterCoroutine = () => m_Done = true;
                };
            }
            else
            {
                restarter.onAfterRestart = () =>
                {
                    restarter.onAfterCoroutine = () => m_Done = true;
                };
            }
        }

        private void RestoreCallbacks ()
        {
            var restarter = OpenXRRestarter.Instance;
            restarter.onAfterRestart = m_OldAfterRestart;
            restarter.onAfterCoroutine = m_OldAfterCoroutine;
            restarter.onAfterSuccessfulRestart = m_OldAfterSuccessfulRestart;
        }

        public override bool keepWaiting
        {
            get
            {
                // Wait until the coroutine is done
                if (m_Done)
                {
                    RestoreCallbacks();
                    return false;
                }

                // Did we time out waiting?
                if (Time.realtimeSinceStartup > m_Timeout)
                {
                    Assert.Fail("WaitForLoaderRestart: Timeout");
                    RestoreCallbacks();
                    return false;
                }

                return true;
            }
        }
    }
}
