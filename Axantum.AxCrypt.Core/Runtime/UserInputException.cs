using Axantum.AxCrypt.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Core.Runtime
{
    public class UserInputException : AxCryptException
    {
        public UserInputException()
            : base()
        {
        }

        public UserInputException(string message)
            : this(message, ErrorStatus.BadUserInput)
        {
        }

        public UserInputException(string message, ErrorStatus errorStatus)
            : base(message, errorStatus)
        {
        }

        public UserInputException(string message, Exception innerException)
            : this(message, ErrorStatus.BadUserInput, innerException)
        {
        }

        public UserInputException(string message, ErrorStatus errorStatus, Exception innerException)
            : base(message, errorStatus, innerException)
        {
        }
    }
}