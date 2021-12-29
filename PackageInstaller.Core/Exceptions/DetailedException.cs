using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PackageInstaller.Core.Exceptions
{
    public class DetailedException : Exception
    {
        public string Details { get; }

        public DetailedException()
        {
            Details = String.Empty;
        }

        public DetailedException(string? message, string? details) : base(message)
        {
            Details = details ?? String.Empty;
        }

        public DetailedException(string? message, Exception? innerException)
            : base(message, innerException) { }

        protected DetailedException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
