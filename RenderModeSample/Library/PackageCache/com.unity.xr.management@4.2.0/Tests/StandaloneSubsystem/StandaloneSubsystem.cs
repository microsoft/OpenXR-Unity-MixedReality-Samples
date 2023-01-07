using System;

namespace UnityEngine.XR.Management.Tests.Standalone
{
    public class StandaloneSubsystem : Subsystem
    {
        private bool isRunning = false;
        public override bool running
        {
            get
            {
                return isRunning;
            }
        }

        public event Action startCalled;
        public event Action stopCalled;
        public event Action destroyCalled;


        public override void Start()
        {
            isRunning = true;
            if (startCalled != null)
                startCalled.Invoke();
        }

        public override void Stop()
        {
            isRunning = false;
            if (stopCalled != null)
                stopCalled.Invoke();
        }

        protected override void OnDestroy()
        {
            isRunning = false;
            if (destroyCalled != null)
                destroyCalled.Invoke();
        }
    }
}
