using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Services.Core
{
    /// <summary>
    /// Exception when the project is not linked to a cloud project id
    /// </summary>
    class UnityProjectNotLinkedException : ServicesInitializationException
    {
        /// <summary>
        /// Initialize a new instance of the <see cref="ServicesInitializationException" /> class.
        /// </summary>
        public UnityProjectNotLinkedException() {}

        /// <summary>
        /// Initialize a new instance of the <see cref="ServicesInitializationException" />
        /// class with a specified error message.
        /// </summary>
        /// <param name="message">
        /// The error message that explains the reason for the exception.
        /// </param>
        public UnityProjectNotLinkedException(string message)
            : base(message) {}
    }
}
