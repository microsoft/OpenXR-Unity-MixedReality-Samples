using System;

namespace Unity.Services.Core.Internal
{
    internal class HashException : Exception
    {
        public int Hash { get; }

        public HashException(int hash)
        {
            Hash = hash;
        }

        public HashException(int hash, string message)
        {
            Hash = hash;
        }

        public HashException(int hash, string message, Exception inner)
            : base(message, inner)
        {
            Hash = hash;
        }
    }

    internal class DependencyTreePackageHashException : HashException
    {
        public DependencyTreePackageHashException(int hash)
            : base(hash)
        {
        }

        public DependencyTreePackageHashException(int hash, string message)
            : base(hash, message)
        {
        }

        public DependencyTreePackageHashException(int hash, string message, Exception inner)
            : base(hash, message, inner)
        {
        }
    }

    internal class DependencyTreeComponentHashException : HashException
    {
        public DependencyTreeComponentHashException(int hash)
            : base(hash)
        {
        }

        public DependencyTreeComponentHashException(int hash, string message)
            : base(hash, message)
        {
        }

        public DependencyTreeComponentHashException(int hash, string message, Exception inner)
            : base(hash, message, inner)
        {
        }
    }
}
