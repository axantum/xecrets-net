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
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Axantum.AxCrypt.Mono
{
    public class HttpRequestServer : IRequestServer, IDisposable
    {
        internal static readonly Uri Url = new Uri("http://localhost:53414/AxCrypt/");

        private HttpListener _listener = new HttpListener();

        public void Start()
        {
            _listener.Prefixes.Add(Url.ToString());
            _listener.Start();
            _listener.BeginGetContext(ListenerCallback, _listener);
        }

        private void ListenerCallback(IAsyncResult result)
        {
            HttpListener listener = (HttpListener)result.AsyncState;
            if (!listener.IsListening)
            {
                return;
            }
            HttpListenerContext context = listener.EndGetContext(result);
            _listener.BeginGetContext(ListenerCallback, _listener);
            HttpListenerRequest request = context.Request;
            using (TextReader reader = new StreamReader(request.InputStream, Encoding.UTF8))
            {
                string requestJson = reader.ReadToEnd();
                CommandServiceEventArgs requestArgs = JsonConvert.DeserializeObject<CommandServiceEventArgs>(requestJson);
                RequestCommandEventArgs args = new RequestCommandEventArgs(requestArgs);
                OnRequest(args);
            }
            using (HttpListenerResponse response = context.Response)
            {
                response.StatusCode = (int)HttpStatusCode.OK;
                response.StatusDescription = "OK";
            }
        }

        public void Shutdown()
        {
            _listener.Stop();
        }

        public event EventHandler<RequestCommandEventArgs> Request;

        protected virtual void OnRequest(RequestCommandEventArgs e)
        {
            EventHandler<RequestCommandEventArgs> handler = Request;
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
            if (_listener != null)
            {
                _listener.Stop();
                _listener.Close();
            }
        }
    }
}