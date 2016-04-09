/*
Icarus is a simple JSON datastore.

Copyright (c) 2015, Matthew Kelly (Badgerati)
Company: Cadaeic Studios
License: MIT (see LICENSE for details)
 */

using System;

namespace Icarus.Core
{
    public class IcarusException : Exception
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="IcarusException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public IcarusException(string message)
            : base(message)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IcarusException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public IcarusException(string message, Exception innerException)
            : base(message, innerException)
        {

        }

    }
}
