#region Coypright and License

/*
 * AxCrypt - Copyright 2016, Svante Seleborg, All Rights Reserved
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
 * The source is maintained at http://bitbucket.org/AxCrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using AxCrypt.Abstractions;
using AxCrypt.Core;
using AxCrypt.Core.Ipc;
using AxCrypt.Core.Runtime;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Mono
{
    public class HttpRequestClient : IRequestClient
    {
        private static readonly HttpClient _client = new HttpClient();

        public CommandStatus Dispatch(CommandServiceEventArgs command)
        {
            string json = Resolve.Serializer.Serialize(command);

            while (!New<IRuntimeEnvironment>().IsFirstInstanceReady(TimeSpan.FromMilliseconds(100)))
            {
            }

            try
            {
                return DoRequestInternal("POST", json);
            }
            catch (WebException wex)
            {
                if (wex.Status == WebExceptionStatus.ConnectFailure)
                {
                    New<IReport>().Exception(wex);
                    return CommandStatus.NoResponse;
                }
                throw;
            }
        }

        private static CommandStatus DoRequestInternal(string method, string content)
        {
            HttpMethod httpMethod = method switch 
            {
                "POST" => HttpMethod.Post,
                "PUT" => HttpMethod.Put,
                "GET" => HttpMethod.Get,
                _ => throw new ArgumentException("Method not suppported.", nameof(method))
            };

            HttpContent httpContent = new StringContent(content);
            var message = new HttpRequestMessage(httpMethod, HttpRequestServer.Url)
            {
                 Content = httpContent,
            };

            HttpResponseMessage response = _client.Send(message);
            if (response.IsSuccessStatusCode)
            {
                return CommandStatus.Success;
            }

            return CommandStatus.Error;
        }
    }
}
