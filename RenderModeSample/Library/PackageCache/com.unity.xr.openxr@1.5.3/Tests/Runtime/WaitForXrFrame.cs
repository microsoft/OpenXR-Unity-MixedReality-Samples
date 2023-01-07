using System.Diagnostics;
using UnityEngine.XR.OpenXR.Features.Mock;
using NUnit.Framework;

namespace UnityEngine.XR.OpenXR.Tests
{
    /// <summary>
    /// Custom yield instruction that waits for xrEndFrame to be called within OpenXR
    /// </summary>
    internal class WaitForXrFrame  : CustomYieldInstruction
    {
        private int m_Frames = 0;
        private long m_Timeout;
        private Stopwatch m_Timer;

        public override bool keepWaiting
        {
            get
            {
                if (m_Frames <= 0)
                    return false;

                if (m_Timer.ElapsedMilliseconds < m_Timeout)
                    return true;

                MockRuntime.onScriptEvent -= OnScriptEvent;
                Assert.Fail("WaitForXrFrame: Timeout");
                return false;
            }
        }

        public WaitForXrFrame(int frames = 1, float timeout = 10.0f)
        {
            m_Frames = frames;
            m_Timeout = (long)(timeout * 1000.0);
            if (frames == 0)
                return;

            // Start waiting for a new frame count
            MockRuntime.onScriptEvent += OnScriptEvent;

            m_Timer = new Stopwatch();
            m_Timer.Restart();
        }

        private void OnScriptEvent(MockRuntime.ScriptEvent evt, ulong param)
        {
            if (evt != MockRuntime.ScriptEvent.EndFrame)
                return;

            m_Frames--;
            if (m_Frames > 0)
                return;

            m_Frames = 0;
            MockRuntime.onScriptEvent -= OnScriptEvent;
        }
    }
}