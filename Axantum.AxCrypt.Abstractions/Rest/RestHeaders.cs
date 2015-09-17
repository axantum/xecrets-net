using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Abstractions.Rest
{
    public class RestHeaders
    {
        public IDictionary<string, string> Collection { get; private set; }

        public RestHeaders()
            : this(new Dictionary<string, string>())
        {
        }

        private RestHeaders(IDictionary<string, string> collection)
        {
            Collection = collection;
        }
    }
}