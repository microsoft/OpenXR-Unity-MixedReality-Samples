using System;
using UnityEngine;

namespace Unity.Services.Core
{
    class ExternalUserIdProperty
    {
        public event Action<string> UserIdChanged;

        string m_UserId;

        public string UserId
        {
            get => m_UserId;
            set
            {
                m_UserId = value;
                UserIdChanged?.Invoke(m_UserId);
            }
        }
    }
}
