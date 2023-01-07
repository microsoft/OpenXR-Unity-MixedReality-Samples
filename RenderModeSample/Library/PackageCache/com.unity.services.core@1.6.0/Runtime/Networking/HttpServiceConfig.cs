using System;
using Unity.Services.Core.Networking.Internal;

namespace Unity.Services.Core.Networking
{
    [Serializable]
    struct HttpServiceConfig
    {
        public string ServiceId;

        public string BaseUrl;

        public HttpOptions DefaultOptions;
    }
}
