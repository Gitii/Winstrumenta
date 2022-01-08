using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PackageInstaller.Core.Exceptions
{
    /// <summary>
    /// An extended version of <seealso cref="Exception"/> which has a <seealso cref="Details"/> property (in addition to the normal <seealso cref="Exception.Message"/>).
    /// </summary>
    public class DetailedException : Exception
    {
        public string Details { get; }

        /// <summary>
        /// An extended version of <seealso cref="Exception"/> which has a <seealso cref="Details"/> property (in addition to the normal <seealso cref="Exception.Message"/>).
        /// </summary>
        public DetailedException()
        {
            Details = String.Empty;
        }

        /// <summary>
        /// An extended version of <seealso cref="Exception"/> which has a <seealso cref="Details"/> property (in addition to the normal <seealso cref="Exception.Message"/>).
        /// </summary>
        /// <param name="message">The message for the exception.</param>
        /// <param name="details">The details for the exception.</param>
        public DetailedException(string? message, string? details) : base(message)
        {
            Details = details ?? String.Empty;
        }

        /// <summary>
        /// An extended version of <seealso cref="Exception"/> which has a <seealso cref="Details"/> property (in addition to the normal <seealso cref="Exception.Message"/>).
        /// </summary>
        /// <param name="message">The message for the exception.</param>
        /// <param name="innerException">Optional inner exception. Will be used as details.</param>
        public DetailedException(string? message, Exception? innerException)
            : base(message, innerException)
        {
            Details = innerException?.Message ?? String.Empty;
        }

        protected DetailedException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
