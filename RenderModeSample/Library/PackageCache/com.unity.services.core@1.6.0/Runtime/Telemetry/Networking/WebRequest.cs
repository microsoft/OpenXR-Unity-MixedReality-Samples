namespace Unity.Services.Core.Telemetry.Internal
{
    enum WebRequestResult
    {
        Success,
        ConnectionError,
        ProtocolError,
        UnknownError,
    }

    struct WebRequest
    {
        public WebRequestResult Result;

        public string ErrorMessage;

        public string ErrorBody;

        public long ResponseCode;

        public bool IsSuccess => Result == WebRequestResult.Success;
    }
}
