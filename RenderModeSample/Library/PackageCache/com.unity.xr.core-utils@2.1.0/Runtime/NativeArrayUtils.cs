using Unity.Collections;

namespace Unity.XR.CoreUtils
{
    /// <summary>
    /// Utility methods for NativeArray&lt;T&gt;
    /// </summary>
    public static class NativeArrayUtils
    {
        /// <summary>
        /// Ensure that this array is large enough to contain the given capacity
        /// </summary>
        /// <typeparam name="T">The type of array element</typeparam>
        /// <param name="array">The array reference. This will be overwritten when the array grows in size</param>
        /// <param name="capacity">The minimum number of elements that the array must contain</param>
        /// <param name="allocator">The allocator to use when creating a new array, if needed</param>
        /// <param name="options">The options to use when creating the new array, if needed</param>
        public static void EnsureCapacity<T>(ref NativeArray<T> array, int capacity, Allocator allocator, NativeArrayOptions options = NativeArrayOptions.ClearMemory) where T : struct
        {
            if (array.Length < capacity)
            {
                if (array.IsCreated)
                {
                    array.Dispose();
                }

                array = new NativeArray<T>(capacity, allocator, options);
            }
        }
    }
}
