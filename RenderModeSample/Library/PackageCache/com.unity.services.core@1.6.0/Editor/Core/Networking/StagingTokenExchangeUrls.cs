using System;

namespace Unity.Services.Core.Editor
{
    class StagingTokenExchangeUrls : ITokenExchangeUrls
    {
        public string ServicesGatewayTokenExchangeUrl => "https://staging.services.unity.com/api/auth/v1/genesis-token-exchange/unity";
    }
}
