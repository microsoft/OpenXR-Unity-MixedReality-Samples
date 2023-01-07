using UnityEngine;
using Unity.Services.Core.Configuration.Internal;

namespace Unity.Services.Core.Configuration
{
    class CloudProjectId : ICloudProjectId
    {
        public string GetCloudProjectId()
        {
            return Application.cloudProjectId;
        }
    }
}
