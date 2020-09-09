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

using AxCrypt.Core;
using AxCrypt.Core.Ipc;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace AxCrypt.Mono
{
    public class HttpRequestServer : IRequestServer, IDisposable
    {
        private static readonly int _sessionId = Process.GetCurrentProcess().SessionId;

        internal static readonly Uri Url = new Uri($"http://localhost:53414/AxCrypt/{_sessionId}/");

        private HttpListener _listener;

        public void Start()
        {
            _listener = new HttpListener();

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
            HttpListenerContext context = null;
            try
            {
                context = listener.EndGetContext(result);
            }
            catch (HttpListenerException)
            {
                return;
            }
            if (listener.IsListening)
            {
                listener.BeginGetContext(ListenerCallback, listener);
            }
            if (context == null)
            {
                return;
            }
            HttpListenerRequest request = context.Request;
            if (request.Url != Url)
            {
                throw new InvalidOperationException($"Request received with wrong URL: '{request.Url}'.");
            }
            RequestCommandEventArgs args = ReadCommand(request);
            try
            {
                using (HttpListenerResponse response = context.Response)
                {
                    response.StatusCode = (int)HttpStatusCode.OK;
                    response.StatusDescription = "OK";
                }
            }
            catch (HttpListenerException)
            {
                return;
            }
            OnRequest(args);
        }

        private static RequestCommandEventArgs ReadCommand(HttpListenerRequest request)
        {
            using (TextReader reader = new StreamReader(request.InputStream, Encoding.UTF8))
            {
                string requestJson = reader.ReadToEnd();
                CommandServiceEventArgs requestArgs = Resolve.Serializer.Deserialize<CommandServiceEventArgs>(requestJson);
                RequestCommandEventArgs args = new RequestCommandEventArgs(requestArgs);
                return args;
            }
        }

        public void Shutdown()
        {
            DisposeInternal();
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
                _listener = null;
            }
        }
    }
}