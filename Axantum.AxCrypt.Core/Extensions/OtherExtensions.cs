using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Core.Extensions
{
    public static class OtherExtensions
    {
        public static string Messages(this Exception exception)
        {
            StringBuilder msg = new StringBuilder();
            while (exception != null)
            {
                if (msg.Length > 0)
                {
                    msg.Append(" -> ");
                }
                msg.Append(exception.Message);
                exception = exception.InnerException;
            }
            return msg.ToString();
        }

        public static Exception Innermost(this Exception exception)
        {
            while (exception.InnerException != null)
            {
                exception = exception.InnerException;
            }
            return exception;
        }
    }
}