using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Api.Response
{
    public class ErrorResponse : ResponseBase
    {
        public ErrorResponse(int status, string message)
        {
            Status = status;
            Message = message;
        }
    }
}