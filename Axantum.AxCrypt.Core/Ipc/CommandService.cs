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