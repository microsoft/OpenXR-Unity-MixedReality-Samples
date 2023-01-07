using NUnit.Framework;
using Unity.Collections;

namespace Unity.XR.CoreUtils.EditorTests
{
    class NativeArrayUtilsTests
    {
        [Test]
        [TestCase(0, 10)]
        [TestCase(0, 0)]
        [TestCase(10, 20)]
        [TestCase(20, 10)]
        [TestCase(10, 10)]
        public void ArrayCapacityInitialized(int initialCapacity, int newCapacity)
        {
            var array = new NativeArray<int>(initialCapacity, Allocator.Temp);
            if (initialCapacity >= 0)
            {
                Assert.IsTrue(array.IsCreated);
            }
            else
            {
                Assert.IsFalse(array.IsCreated);
            }

            NativeArrayUtils.EnsureCapacity(ref array, newCapacity, Allocator.Temp);
            if (newCapacity > initialCapacity)
            {
                Assert.AreEqual(array.Length, newCapacity);
            }
            else
            {
                Assert.AreEqual(array.Length, initialCapacity);
            }

            array.Dispose();
        }

        [Test]
        [TestCase(20)]
        [TestCase(0)]
        [TestCase(-10)]
        public void ArrayCapacityUninitialized(int newCapacity)
        {
            NativeArray<int> array = default;
            Assert.IsFalse(array.IsCreated);

            NativeArrayUtils.EnsureCapacity(ref array, newCapacity, Allocator.Temp);
            if (newCapacity > 0)
            {
                Assert.AreEqual(array.Length, newCapacity);

                array.Dispose();
            }
            else
            {
                Assert.IsFalse(array.IsCreated);
            }
        }
    }
}
