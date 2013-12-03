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

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Ipc
{
    public class CommandService : IDisposable
    {
        private IRequestServer _server;
        private IRequestClient _client;

        public CommandService(IRequestServer server, IRequestClient client)
        {
            _server = server;
            _client = client;

            _server.Request += HandleServerRequest;
        }

        private void HandleServerRequest(object sender, RequestCommandArgs e)
        {
            CommandServiceArgs requestArgs = JsonConvert.DeserializeObject<CommandServiceArgs>(e.CommandMessage);
            OnReceived(requestArgs);
        }

        public CommandStatus Call(CommandVerb verb, params string[] paths)
        {
            return Call(verb, new List<string>(paths));
        }

        public CommandStatus Call(CommandVerb verb, IEnumerable<string> paths)
        {
            CommandStatus status;
            string json = JsonConvert.SerializeObject(new CommandServiceArgs(verb, paths), Formatting.None, new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Include, NullValueHandling = NullValueHandling.Ignore, });
            status = _client.Dispatch("POST", json);
            return status;
        }

        public void StartListening()
        {
            _server.Start();
        }

        public event EventHandler<CommandServiceArgs> Received;

        protected virtual void OnReceived(CommandServiceArgs e)
        {
            EventHandler<CommandServiceArgs> handler = Received;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeInternal();
            }
        }

        private void DisposeInternal()
        {
            if (_server != null)
            {
                _server.Shutdown();
                _server = null;
            }
        }
    }
}