using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.IO
{
    public class WebHeaders
    {
        public static WebHeaders Empty = new WebHeaders(new Dictionary<string, string>());

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