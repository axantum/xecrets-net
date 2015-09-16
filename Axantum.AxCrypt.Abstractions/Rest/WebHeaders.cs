using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Abstractions.Rest
{
    public class WebHeaders
    {
        public IDictionary<string, string> Collection { get; private set; }

        public WebHeaders()
            : this(new Dictionary<string, string>())
        {
        }

        private WebHeaders(IDictionary<string, string> collection)
        {
            Collection = collection;
        }
    }
}