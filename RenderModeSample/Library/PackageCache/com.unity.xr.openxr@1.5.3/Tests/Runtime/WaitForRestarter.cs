using NUnit.Framework;

namespace UnityEngine.XR.OpenXR.Tests
{
    /// <summary>
    /// Custom yield instruction that waits for the OpenXRRestarter to finish if it is running.
    /// Note that ulink WaitForLoaderRestart and WaitForLoaderShutdown this yield instruction
    /// will not wait if the restarter is not already running.
    /// </summary>
    internal sealed class WaitForRestarter : CustomYieldInstruction
    {
        private float m_Timeout = 0;

        public WaitForRestarter(float timeout = 5.0f)
        {
            m_Timeout = Time.realtimeSinceStartup + timeout;
        }

        public override bool keepWaiting
        {
            get
            {
                // Wait until the restarter is finished
                if (!OpenXRRestarter.Instance.isRunning)
                {
                    return false;
                }

                // Did we time out waiting?
                if (Time.realtimeSinceStartup > m_Timeout)
                {
                    Assert.Fail("WaitForLoaderRestart: Timeout");
                    return false;
                }

                return true;
            }
        }
    }
}
