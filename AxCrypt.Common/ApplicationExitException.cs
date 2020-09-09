using AxCrypt.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCrypt.Common
{
    public class ApplicationExitException : AxCryptException
    {
        public ApplicationExitException()
            : base()
        {
        }

        public ApplicationExitException(string message)
            : this(message, ErrorStatus.Exit)
        {
        }

        public ApplicationExitException(string message, ErrorStatus errorStatus)
            : base(message, errorStatus)
        {
        }

        public ApplicationExitException(string message, Exception innerException)
            : this(message, ErrorStatus.Exit, innerException)
        {
        }

        public ApplicationExitException(string message, ErrorStatus errorStatus, Exception innerException)
            : base(message, errorStatus, innerException)
        {
        }
    }
}