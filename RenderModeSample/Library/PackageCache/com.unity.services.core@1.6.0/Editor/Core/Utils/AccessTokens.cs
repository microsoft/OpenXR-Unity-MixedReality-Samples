using System.Threading.Tasks;
using UnityEditor;

namespace Unity.Services.Core.Editor
{
    /// <summary>
    /// Helper class to get the different kind of tokens used by services at editor time.
    /// </summary>
    public class AccessTokens
    {
        readonly TokenExchange m_TokenExchange;

        internal AccessTokens(TokenExchange tokenExchange)
        {
            m_TokenExchange = tokenExchange;
        }

        /// <summary>
        /// Initialize a new instance of the <see cref="AccessTokens"/> class.
        /// </summary>
        public AccessTokens()
        {
            var env = new CloudEnvironmentConfigProvider();
            if (env.IsStaging())
            {
                m_TokenExchange = new TokenExchange(new StagingTokenExchangeUrls());
            }
            else
            {
                m_TokenExchange = new TokenExchange(new ProductionTokenExchangeUrls());
            }
        }

        /// <summary>
        /// The access token used by Genesis.
        /// </summary>
        /// <returns>
        /// Genesis Access Token.
        /// </returns>
        public static string GetGenesisToken()
        {
            return CloudProjectSettings.accessToken;
        }

        /// <summary>
        /// Task that represents an asynchronous operation to get services gateway token.
        /// </summary>
        /// <returns>
        /// Task with a result that represents the services gateway token.
        /// </returns>
        public async Task<string> GetServicesGatewayTokenAsync()
        {
            var genesisToken = GetGenesisToken();
            var token = await m_TokenExchange.ExchangeServicesGatewayTokenAsync(genesisToken);
            return token;
        }
    }
}
