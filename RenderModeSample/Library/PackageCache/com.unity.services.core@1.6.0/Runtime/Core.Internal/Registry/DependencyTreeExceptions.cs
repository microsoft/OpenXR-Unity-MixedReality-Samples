using System;
using System.Collections;
using System.Collections.Generic;

namespace Unity.Services.Core.Internal
{
    internal class DependencyTreeSortFailedException : Exception
    {
        public DependencyTreeSortFailedException(DependencyTree tree, ICollection<int> target)
            : base(CreateExceptionMessage(tree, target))
        {
        }

        public DependencyTreeSortFailedException(DependencyTree tree, ICollection<int> target, Exception inner)
            : base(CreateExceptionMessage(tree, target, inner), inner)
        {
        }

        private static string CreateExceptionMessage(DependencyTree tree, ICollection<int> target, Exception inner = null)
        {
            var orderedTreeJson = tree.ToJson(target);
            var errorMessage = $"Failed to sort tree! It is likely there is a missing required dependency:\n{orderedTreeJson}";
            errorMessage = errorMessage + (inner != null ? $"\n Error: {inner.Message}" : string.Empty);
            return errorMessage;
        }
    }
}
