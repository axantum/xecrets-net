using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Axantum.AxCrypt.Core.IO
{
    public class WebAnswer
    {
        public HttpStatusCode StatusCode { get; private set; }

        public string Content { get; private set; }

        public WebAnswer(HttpStatusCode statusCode, string content)
        {
            StatusCode = HttpStatusCode.OK;
            Content = content;
        }
    }
}