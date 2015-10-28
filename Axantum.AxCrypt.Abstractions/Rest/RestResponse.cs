using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Axantum.AxCrypt.Abstractions.Rest
{
    public class RestResponse
    {
        public HttpStatusCode StatusCode { get; private set; }

        public string Content { get; private set; }

        public RestResponse(HttpStatusCode statusCode, string content)
        {
            StatusCode = statusCode;
            Content = content;
        }
    }
}