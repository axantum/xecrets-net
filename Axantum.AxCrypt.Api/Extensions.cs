using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Abstractions.Rest;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Api
{
    public static class Extensions
    {
        public static string UrlEncode(this string value)
        {
            return TypeMap.Resolve.New<IRestCaller>().UrlEncode(value);
        }

        public static Uri PathCombine(this Uri baseUrl, string path)
        {
            return new Uri(baseUrl, path);
        }

        public static string With(this string format, params string[] args)
        {
            return String.Format(CultureInfo.InvariantCulture, format, args);
        }
    }
}