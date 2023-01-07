using System;
using Unity.Services.Core.Internal;
using UnityEngine.Networking;

namespace Unity.Services.Core.Telemetry.Internal
{
    class UnityWebRequestSender : IUnityWebRequestSender
    {
        public void SendRequest(UnityWebRequest request, Action<WebRequest> callback)
        {
            var sendingOperation = request.SendWebRequest();
            sendingOperation.completed += OnSendingRequestCompleted;

            void OnSendingRequestCompleted(UnityEngine.AsyncOperation operation)
            {
                using (var webRequest = ((UnityWebRequestAsyncOperation)operation).webRequest)
                {
                    if (callback is null)
                        return;

                    var simplifiedRequest = Simplify(webRequest);
                    callback(simplifiedRequest);
                }
            }
        }

        static WebRequest Simplify(UnityWebRequest webRequest)
        {
            var simplifiedRequest = new WebRequest
            {
                ResponseCode = webRequest.responseCode,
            };

            if (webRequest.HasSucceeded())
            {
                simplifiedRequest.Result = WebRequestResult.Success;
            }
            else
            {
#if UNITY_2020_1_OR_NEWER
                switch (webRequest.result)
                {
                    case UnityWebRequest.Result.ConnectionError:
                    {
                        simplifiedRequest.Result = WebRequestResult.ConnectionError;
                        break;
                    }
                    case UnityWebRequest.Result.ProtocolError:
                    {
                        simplifiedRequest.Result = WebRequestResult.ProtocolError;
                        break;
                    }
                    default:
                    {
                        simplifiedRequest.Result = WebRequestResult.UnknownError;
                        break;
                    }
                }
#else
                if (webRequest.isHttpError)
                {
                    simplifiedRequest.Result = WebRequestResult.ProtocolError;
                }
                else if (webRequest.isNetworkError)
                {
                    simplifiedRequest.Result = WebRequestResult.ConnectionError;
                }
                else
                {
                    simplifiedRequest.Result = WebRequestResult.UnknownError;
                }
#endif

                simplifiedRequest.ErrorMessage = webRequest.error;
                simplifiedRequest.ErrorBody = webRequest.downloadHandler.text;
            }

            return simplifiedRequest;
        }
    }
}
