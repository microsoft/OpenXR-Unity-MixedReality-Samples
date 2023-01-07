using System;
using UnityEngine;
using NotNull = JetBrains.Annotations.NotNullAttribute;

namespace Unity.Services.Core.Telemetry.Internal
{
    class ExponentialBackOffRetryPolicy
    {
        int m_MaxTryCount = 10;

        public int MaxTryCount
        {
            get => m_MaxTryCount;
            set => m_MaxTryCount = Math.Max(value, 0);
        }

        float m_BaseDelaySeconds = 2;

        public float BaseDelaySeconds
        {
            get => m_BaseDelaySeconds;
            set => m_BaseDelaySeconds = Math.Max(value, 0);
        }

        public bool CanRetry(WebRequest webRequest, int sendCount)
        {
            return sendCount < MaxTryCount
                && IsTransientError(webRequest);
        }

        public static bool IsTransientError(WebRequest webRequest)
        {
            return webRequest.Result == WebRequestResult.ConnectionError
                || webRequest.Result == WebRequestResult.ProtocolError && IsServerErrorCode(webRequest.ResponseCode);

            bool IsServerErrorCode(long responseCode)
            {
                return responseCode >= 500
                    && responseCode < 600;
            }
        }

        public float GetDelayBeforeSendingSeconds(int sendCount)
        {
            if (sendCount <= 0)
            {
                return BaseDelaySeconds;
            }

            return Mathf.Pow(BaseDelaySeconds, sendCount);
        }
    }
}
