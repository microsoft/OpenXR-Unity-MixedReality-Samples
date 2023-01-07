using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine.Networking;

namespace Unity.Services.Core.Editor
{
    [Serializable]
    class TokenExchangeRequest
    {
        public TokenExchangeRequest(string token)
        {
            Token = token;
        }

        public TokenExchangeRequest() { }

        [JsonProperty("token")]
        public string Token;
    }

    [Serializable]
    class TokenExchangeResponse
    {
        [JsonProperty("token")]
        public string Token;
    }

    [Serializable]
    class TokenExchangeRequestError
    {
        [JsonProperty("status")]
        public int Status;

        [JsonProperty("title")]
        public string Title;

        [JsonProperty("requestId")]
        public string RequestId;

        [JsonProperty("detail")]
        public string Detail;
    }

    class TokenExchange
    {
        const string k_RequestContentType = "application/json";

        readonly ITokenExchangeUrls m_TokenExchangeUrls;

        internal TokenExchange(ITokenExchangeUrls tokenExchangeUrls)
        {
            m_TokenExchangeUrls = tokenExchangeUrls;
        }

        Task<UnityWebRequest> SendTokenExchangeUnityWebRequestAsync(TokenExchangeRequest tokenExchangeRequest)
        {
            var taskCompletionSource = new TaskCompletionSource<UnityWebRequest>();
            var jsonPayload = JsonConvert.SerializeObject(tokenExchangeRequest);
            var postBytes = Encoding.UTF8.GetBytes(jsonPayload);
            var unityWebRequest = new UnityWebRequest(
                m_TokenExchangeUrls.ServicesGatewayTokenExchangeUrl, UnityWebRequest.kHttpVerbPOST);
            unityWebRequest.uploadHandler = new UploadHandlerRaw(postBytes)
            {
                contentType = k_RequestContentType
            };
            unityWebRequest.downloadHandler = new DownloadHandlerBuffer();
            unityWebRequest.SendWebRequest().completed += OnRequestCompleted;

            return taskCompletionSource.Task;

            void OnRequestCompleted(UnityEngine.AsyncOperation operation)
            {
                var request = ((UnityWebRequestAsyncOperation)operation).webRequest;
                using (request)
                {
                    taskCompletionSource.TrySetResult(unityWebRequest);
                }
            }
        }

        internal async Task<string> ExchangeServicesGatewayTokenAsync(string genesisToken)
        {
            var tokenExchangeRequest = new TokenExchangeRequest(genesisToken);
            var responseJson = await SendUnityWebRequestAndGetResponseAsync(tokenExchangeRequest);
            var tokenExchangeResponse = JsonConvert.DeserializeObject<TokenExchangeResponse>(responseJson);

            if (tokenExchangeResponse?.Token == null)
            {
                var tokenExchangeError = JsonConvert.DeserializeObject<TokenExchangeRequestError>(responseJson);
                throw new RequestFailedException(tokenExchangeError.Status, tokenExchangeError.Detail);
            }

            return tokenExchangeResponse.Token;
        }

        /// <remarks>
        /// Made virtual to allow mocking in tests.
        /// </remarks>
        internal virtual async Task<string> SendUnityWebRequestAndGetResponseAsync(
            TokenExchangeRequest tokenExchangeRequest)
        {
            using (var exchangeRequest = await SendTokenExchangeUnityWebRequestAsync(tokenExchangeRequest))
            {
                var responseJson = exchangeRequest.downloadHandler.text;
                return responseJson;
            }
        }
    }
}
