using System.Runtime.CompilerServices;

namespace UnityEngine.XR.ARFoundation
{
    /// <summary>
    /// Use in place of Debug.Assert to conditionally generate the string for the assert message.
    /// </summary>
    static class DebugAssert
    {
        public struct Message
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void WithMessage(string message)
            {
                Debug.Assert(false, message);
            }
        }

        /// <summary>
        /// When both
        /// - `UNITY_ASSERTIONS` is defined (for example, in a development build) AND
        /// - <paramref name="condition"/> is `false`
        /// a new <see cref="Message"/> object is returned. This allows you to write code like
        /// <code>
        /// DebugAssert.That(foo == null)?.WithMessage($"{nameof(foo)} should be null but was {foo} instead");
        /// </code>
        /// Note the use of the null conditional (?.) -- this means the interpolated string in
        /// <see cref="Message.WithMessage"/> is not evaluated if the assert condition passes. This can prevent the
        /// creation of GC-allocated string objects when asserts are used several times per frame.
        ///
        /// <see cref="Message.WithMessage"/> is what actually calls Debug.Assert, so make sure to provide a message.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Message? That(bool condition)
        {
#if UNITY_ASSERTIONS
            if (!condition)
            {
                return new Message();
            }
#endif
            return null;
        }
    }

    /// <summary>
    /// Use in place of Debug.LogWarning to conditionally generate the string for the warning message.
    /// </summary>
    static class DebugWarn
    {
        public struct Message
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void WithMessage(string message)
            {
                Debug.LogWarning(message);
            }
        }

        /// <summary>
        /// When both
        /// - `DEVELOPMENT_BUILD` is defined (for example, in a development build) AND
        /// - <paramref name="condition"/> is `false`
        /// a new <see cref="Message"/> object is returned. This allows you to write code like
        /// <code>
        /// DebugWarn.WhenFalse(foo == null)?.WithMessage($"{nameof(foo)} should be null but was {foo} instead");
        /// </code>
        /// Note the use of the null conditional (?.) -- this means the interpolated string in
        /// <see cref="Message.WithMessage"/> is not evaluated if the assert condition passes. This can prevent the
        /// creation of GC-allocated string objects when asserts are used several times per frame.
        ///
        /// <see cref="Message.WithMessage"/> is what actually calls Debug.Assert, so make sure to provide a message.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Message? WhenFalse(bool condition)
        {
#if DEVELOPMENT_BUILD
            if (!condition)
            {
                return new Message();
            }
#endif
            return null;
        }
    }
}
