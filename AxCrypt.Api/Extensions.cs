using AxCrypt.Abstractions.Rest;
using System;
using System.Globalization;

using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Api
{
    public static class Extensions
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings")]
        public static string UrlEncode(this string value)
        {
            return New<IRestCaller>().UrlEncode(value);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2234:PassSystemUriObjectsInsteadOfStrings")]
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