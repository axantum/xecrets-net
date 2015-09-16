using Axantum.AxCrypt.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Api
{
    public static class Extensions
    {
        public static string UrlEncode(this string value)
        {
            return Resolve.WebCaller.UrlEncode(value);
        }

        public static Uri PathCombine(this Uri baseUrl, string path)
        {
            return new Uri(baseUrl, path);
        }
    }
}