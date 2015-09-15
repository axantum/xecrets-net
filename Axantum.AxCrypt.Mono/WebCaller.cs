#region Coypright and License

/*
 * AxCrypt - Copyright 2014, Svante Seleborg, All Rights Reserved
 *
 * This file is part of AxCrypt.
 *
 * AxCrypt is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * AxCrypt is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with AxCrypt.  If not, see <http://www.gnu.org/licenses/>.
 *
 * The source is maintained at http://bitbucket.org/axantum/axcrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axantum.com for more information about the author.
*/

#endregion Coypright and License

using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Mono
{
    internal class WebCaller : IWebCaller
    {
        public WebCaller()
        {
        }

        #region IWebCaller Members

        public WebCallerResponse Send(LogOnIdentity identity, WebCallerRequest request)
        {
            if (identity == null)
            {
                throw new ArgumentNullException("identity");
            }
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            switch (request.Method)
            {
                case "GET":
                    if (request.Content.Text.Length > 0)
                    {
                        throw new ArgumentException("You can't send content with a GET request.", "content");
                    }
                    return SendGet(request).Result;

                default:
                    throw new NotSupportedException("The method '{0}' is not supported.".InvariantFormat(request.Method));
            }
        }

        private async static Task<WebCallerResponse> SendGet(WebCallerRequest request)
        {
            string content = String.Empty;
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(request.Url.GetLeftPart(UriPartial.Authority));
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue();
                client.DefaultRequestHeaders.CacheControl.NoCache = true;
                client.DefaultRequestHeaders.CacheControl.NoStore = true;
                foreach (string key in request.Headers.Collection.Keys)
                {
                    client.DefaultRequestHeaders.Add(key, request.Headers.Collection[key]);
                }

                HttpResponseMessage httpResponse = await client.GetAsync(request.Url.PathAndQuery);
                if (!httpResponse.IsSuccessStatusCode)
                {
                    return new WebCallerResponse(httpResponse.StatusCode, String.Empty);
                }
                content = await httpResponse.Content.ReadAsStringAsync();
            }
            return new WebCallerResponse(HttpStatusCode.OK, content);
        }

        #endregion IWebCaller Members
    }
}