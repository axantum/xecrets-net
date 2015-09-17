using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Abstractions.Rest
{
    public class RestRequest
    {
        public RestContent Content { get; private set; }

        public RestHeaders Headers { get; private set; }

        public TimeSpan Timeout { get; private set; }

        public string Method { get; private set; }

        public Uri Url { get; private set; }

        public RestRequest()
        {
            Content = new RestContent();
            Headers = new RestHeaders();
            Timeout = TimeSpan.FromSeconds(1);
        }

        public RestRequest(string method, Uri url)
            : this()
        {
            Method = method;
            Url = url;
        }

        public RestRequest(Uri url) : this("GET", url)
        {
        }
    }
}