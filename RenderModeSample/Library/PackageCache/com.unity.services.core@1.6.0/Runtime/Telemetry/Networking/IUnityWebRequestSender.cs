using System;
using UnityEngine.Networking;

namespace Unity.Services.Core.Telemetry.Internal
{
    interface IUnityWebRequestSender
    {
        void SendRequest(UnityWebRequest request, Action<WebRequest> callback);
    }
}
