using AxCrypt.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCrypt.Core.Runtime
{
    public class PasswordException : AxCryptException
    {
        public PasswordException()
            : base()
        {
        }

        public PasswordException(string message)
            : this(message, ErrorStatus.WrongPassword)
        {
        }

        public PasswordException(string message, ErrorStatus errorStatus)
            : base(message, errorStatus)
        {
        }

        public PasswordException(string message, Exception innerException)
            : this(message, ErrorStatus.WrongPassword, innerException)
        {
        }

        public PasswordException(string message, ErrorStatus errorStatus, Exception innerException)
            : base(message, errorStatus, innerException)
        {
        }
    }
}