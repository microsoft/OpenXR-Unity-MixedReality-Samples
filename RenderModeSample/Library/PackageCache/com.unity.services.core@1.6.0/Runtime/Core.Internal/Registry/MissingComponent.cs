using System;

namespace Unity.Services.Core.Internal
{
    /// <summary>
    /// Placeholder for required <see cref="IServiceComponent"/> registered into a <see cref="CoreRegistry"/>.
    /// </summary>
    class MissingComponent : IServiceComponent
    {
        public Type IntendedType { get; }

        internal MissingComponent(Type intendedType)
        {
            IntendedType = intendedType;
        }
    }
}
