using System;

namespace UnityEngine.XR.ARFoundation
{
    /// <summary>
    /// Event arguments for <see cref="ARSession.stateChanged"/>.
    /// </summary>
    public struct ARSessionStateChangedEventArgs : IEquatable<ARSessionStateChangedEventArgs>
    {
        /// <summary>
        /// The new session state.
        /// </summary>
        public ARSessionState state { get; private set; }

        /// <summary>
        /// Constructor for these event arguments.
        /// </summary>
        /// <param name="state">The new session state.</param>
        public ARSessionStateChangedEventArgs(ARSessionState state)
        {
            this.state = state;
        }

        /// <summary>
        /// Generates a hash suitable for use with containers like `HashSet` and `Dictionary`.
        /// </summary>
        /// <returns>A hash code generated from this object's fields.</returns>
        public override int GetHashCode() => ((int)state).GetHashCode();

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="obj">The `object` to compare against.</param>
        /// <returns>`True` if <paramref name="obj"/> is of type <see cref="ARSessionStateChangedEventArgs"/> and
        /// <see cref="Equals(ARSessionStateChangedEventArgs)"/> also returns `true`; otherwise `false`.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is ARSessionStateChangedEventArgs))
                return false;

            return Equals((ARSessionStateChangedEventArgs)obj);
        }

        /// <summary>
        /// Generates a string representation of this <see cref="ARSessionStateChangedEventArgs"/>.
        /// </summary>
        /// <returns>A string representation of this <see cref="ARSessionStateChangedEventArgs"/>.</returns>
        public override string ToString() => state.ToString();

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="other">The other <see cref="ARSessionStateChangedEventArgs"/> to compare against.</param>
        /// <returns>`True` if every field in <paramref name="other"/> is equal to this <see cref="ARSessionStateChangedEventArgs"/>, otherwise false.</returns>
        public bool Equals(ARSessionStateChangedEventArgs other)
        {
            return state == other.state;
        }

        /// <summary>
        /// Tests for equality. Same as <see cref="Equals(ARSessionStateChangedEventArgs)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if <paramref name="lhs"/> is equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        public static bool operator ==(ARSessionStateChangedEventArgs lhs, ARSessionStateChangedEventArgs rhs)
        {
            return lhs.Equals(rhs);
        }

        /// <summary>
        /// Tests for inequality. Same as `!`<see cref="Equals(ARSessionStateChangedEventArgs)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if <paramref name="lhs"/> is not equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        public static bool operator !=(ARSessionStateChangedEventArgs lhs, ARSessionStateChangedEventArgs rhs)
        {
            return !lhs.Equals(rhs);
        }
    }
}
