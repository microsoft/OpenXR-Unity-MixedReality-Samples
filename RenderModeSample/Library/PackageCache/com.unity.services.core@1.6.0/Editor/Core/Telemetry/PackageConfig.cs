using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.PackageManager;
using CanBeNull = JetBrains.Annotations.CanBeNullAttribute;

namespace Unity.Services.Core.Editor
{
    [Serializable]
    struct PackageConfig
    {
        public string Name;

        public string Version;

        public List<string> InitializerNames;

        public PackageConfig([CanBeNull] PackageInfo packageInfo, IEnumerable<Type> initializerTypes)
        {
            Name = packageInfo?.name;
            Version = packageInfo?.version;
            InitializerNames = initializerTypes.Select(x => x.AssemblyQualifiedName).ToList();
        }
    }
}
