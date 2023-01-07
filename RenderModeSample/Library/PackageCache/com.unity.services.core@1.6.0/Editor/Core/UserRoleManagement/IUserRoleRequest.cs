using Unity.Services.Core.Internal;

namespace Unity.Services.Core.Editor
{
    interface IUserRoleRequest
    {
        IAsyncOperation<UserRole> GetUserRole();
    }
}
