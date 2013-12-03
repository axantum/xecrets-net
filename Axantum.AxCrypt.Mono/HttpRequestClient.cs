#region Coypright and License

/*
 * AxCrypt - Copyright 2013, Svante Seleborg, All Rights Reserved
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

using Axantum.AxCrypt.Core.Ipc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Axantum.AxCrypt.Mono
{
    public class HttpRequestClient : IRequestClient
    {
        public CommandStatus Dispatch(string method, string content)
        {
            WebRequest request = HttpWebRequest.Create(HttpRequestServer.Url);
            try
            {
                return DoRequestInternal(method, content, request);
            }
            catch (WebException wex)
            {
                if (wex.Status == WebExceptionStatus.ConnectFailure)
                {
                    return CommandStatus.NoResponse;
                }
                throw;
            }
        }

        private static CommandStatus DoRequestInternal(string method, string content, WebRequest request)
        {
            request.Method = method;
            if (method == "POST" || method == "PUT")
            {
                using (StreamWriter writer = new StreamWriter(request.GetRequestStream(), Encoding.UTF8))
                {
                    writer.Write(content);
                }
            }
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return CommandStatus.Success;
                }
                return CommandStatus.Error;
            }
        }
    }
}