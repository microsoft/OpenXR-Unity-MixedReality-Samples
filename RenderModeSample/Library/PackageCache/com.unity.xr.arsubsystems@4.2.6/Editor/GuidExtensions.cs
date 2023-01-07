using System;

namespace UnityEditor.XR.ARSubsystems
{
    /// <summary>
    /// Extensions to [System.Guid](xref:System.Guid)
    /// </summary>
    public static class GuidExtensions
    {
        /// <summary>
        /// Decomposes a 16-byte [Guid](xref:System.Guid) into two 8-byte <c>ulong</c>s.
        /// Recompose using [UnityEngine.XR.ARSubsystems.GuidUtil.Compose](xref:UnityEngine.XR.ARSubsystems.GuidUtil.Compose(System.UInt64,System.UInt64)).
        /// </summary>
        /// <param name="guid">The <c>Guid</c> being extended</param>
        /// <param name="low">The lower 8 bytes of the <c>Guid</c>.</param>
        /// <param name="high">The upper 8 bytes of the <c>Guid</c>.</param>
        public static void Decompose(this Guid guid, out ulong low, out ulong high)
        {
            var bytes = guid.ToByteArray();
            low = BitConverter.ToUInt64(bytes, 0);
            high = BitConverter.ToUInt64(bytes, 8);
        }
    }
}
