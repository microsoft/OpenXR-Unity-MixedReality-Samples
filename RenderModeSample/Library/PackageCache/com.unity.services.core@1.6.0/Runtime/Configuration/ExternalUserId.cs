using System;
using Unity.Services.Core.Configuration.Internal;

namespace Unity.Services.Core.Configuration
{
    class ExternalUserId : IExternalUserId
    {
        public string UserId => UnityServices.ExternalUserIdProperty.UserId;

        public event Action<string> UserIdChanged
        {
            add => UnityServices.ExternalUserIdProperty.UserIdChanged += value;
            remove => UnityServices.ExternalUserIdProperty.UserIdChanged -= value;
        }
    }
}
