using System.Collections.Generic;
using UnityEditor.PackageManager;

namespace Unity.Services.Core.Editor
{
    class PackageInfoNameComparer : IEqualityComparer<PackageInfo>
    {
        public bool Equals(PackageInfo x, PackageInfo y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (ReferenceEquals(x, null))
            {
                return false;
            }

            if (ReferenceEquals(y, null))
            {
                return false;
            }

            return x.name == y.name;
        }

        public int GetHashCode(PackageInfo obj) => obj.name != null ? obj.name.GetHashCode() : 0;
    }
}
