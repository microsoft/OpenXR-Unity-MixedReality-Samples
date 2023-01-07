using System;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace Unity.Services.Core.Internal
{
    static class UnityWebRequestUtils
    {
        public const string JsonContentType = "application/json";

        public static bool HasSucceeded(this UnityWebRequest self)
        {
#if UNITY_2020_1_OR_NEWER
            return self.result == UnityWebRequest.Result.Success;
#else
            return !self.isHttpError && !self.isNetworkError;
#endif
        }

        public static Task<string> GetTextAsync(string uri)
        {
            var completionSource = new TaskCompletionSource<string>();
            var request = UnityWebRequest.Get(uri);
            request.SendWebRequest()
                .completed += CompleteFetchTaskOnRequestCompleted;

            return completionSource.Task;

            void CompleteFetchTaskOnRequestCompleted(UnityEngine.AsyncOperation rawOperation)
            {
                try
                {
                    var operation = (UnityWebRequestAsyncOperation)rawOperation;
                    using (var operationRequest = operation.webRequest)
                    {
                        if (operationRequest.HasSucceeded())
                        {
                            completionSource.TrySetResult(operationRequest.downloadHandler.text);
                        }
                        else
                        {
                            var errorMessage = "Couldn't fetch config file." +
                                $"\nURL: {operationRequest.url}" +
                                $"\nReason: {operationRequest.error}";
                            completionSource.TrySetException(new Exception(errorMessage));
                        }
                    }
                }
                catch (Exception e)
                {
                    completionSource.TrySetException(e);
                }
            }
        }
    }
}
