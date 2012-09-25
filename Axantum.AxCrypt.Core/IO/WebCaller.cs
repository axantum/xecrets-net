using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Text;

namespace Axantum.AxCrypt.Core.IO
{
    public class WebCaller : IWebCaller
    {
        public WebCaller()
        {
        }

        #region IWebCaller Members

        public string Go(Uri url)
        {
            string response = String.Empty;
            using (WebClient client = new WebClient())
            {
                client.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
                response = client.DownloadString(url);
            }
            return response;
        }

        #endregion IWebCaller Members
    }
}