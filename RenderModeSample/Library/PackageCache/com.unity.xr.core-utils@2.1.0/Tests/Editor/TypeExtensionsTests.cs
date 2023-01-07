using System;
using NUnit.Framework;
using UnityEngine;

namespace Unity.XR.CoreUtils.EditorTests
{
    class TypeExtensionsTests
    {
        class DummyClassBase { }
        class DummyClassChildA : DummyClassBase { }
        class DummyClassChildB : DummyClassBase { }
        class DummyClassChildC : DummyClassChildA { }
        class DummyClassNotChildOfBase { }
        enum DummyEnum
        {
            None = 0,
            One
        }

        [TestCase(typeof(DummyClassBase), typeof(DummyClassBase), true)]
        [TestCase(typeof(DummyClassBase), typeof(DummyClassChildA), true)]
        [TestCase(typeof(DummyClassBase), typeof(DummyClassChildB), true)]
        [TestCase(typeof(DummyClassChildA), typeof(DummyClassChildB), false)]
        [TestCase(typeof(DummyClassChildA), typeof(DummyClassChildC), true)]
        [TestCase(typeof(DummyClassChildB), typeof(DummyClassChildC), false)]
        [TestCase(typeof(DummyClassBase), typeof(DummyClassNotChildOfBase), false)]
        [TestCase(typeof(Enum), typeof(DummyClassNotChildOfBase), false)]
        [TestCase(typeof(Enum), typeof(DummyEnum), true)]
        public void IsAssignableFromOrSubclassOf(Type baseType, Type checkType, bool testResult)
        {
            var result = checkType.IsAssignableFromOrSubclassOf(baseType);
            Assert.True(result == testResult);
        }
    }
}
