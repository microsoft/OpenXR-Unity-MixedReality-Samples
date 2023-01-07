using System.Collections.Generic;
using Unity.Services.Core.Internal;

namespace Unity.Services.Core.Editor
{
    interface IServiceFlagRequest
    {
        IAsyncOperation<IDictionary<string, bool>> FetchServiceFlags();
    }
}
