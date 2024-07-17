using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Diagnostics;

namespace Microsoft.MixedReality.OpenXR.Sample
{
    public class OpenXRTimeLogTest : MonoBehaviour
    {
        [SerializeField]
        private bool logEveryFrame;

        private OpenXRTime XRUtils;
        private long prevTime;

#if ENABLE_WINMD_SUPPORT
        // Start is called before the first frame update
        void Start()
        {
            XRUtils = OpenXRTime.Current;
            LogOpenXRTime();
        }

        // Update is called once per frame
        void Update()
        {
            if (logEveryFrame)
            {
                LogOpenXRTime();
            }
        }
#endif
        private void LogOpenXRTime()
        {
            long xrTime = XRUtils.GetPredictedDisplayTimeInXrTime(FrameTime.OnUpdate);
            if (xrTime <= 0 || (xrTime - prevTime) <= 0)
            {
                Debug.LogError($"Error: {xrTime} is not greater than previous xrTime.");
            }

            long predictedTicks = XRUtils.ConvertXrTimeToQpcTime(xrTime);
            var frameDateTime = TimeSpan.FromTicks(predictedTicks);
            Debug.Log("Predicted Display Time:" + frameDateTime.ToString());
        }
    }
}
