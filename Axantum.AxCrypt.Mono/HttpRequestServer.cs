using Axantum.AxCrypt.Core.Ipc;
using System;
using System.Collections.Generic;
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
            HttpListenerContext context = listener.EndGetContext(result);
            _listener.BeginGetContext(ListenerCallback, _listener);
            HttpListenerRequest request = context.Request;
            using (TextReader reader = new StreamReader(request.InputStream, Encoding.UTF8))
            {
                string requestJson = reader.ReadToEnd();
                RequestCommandArgs args = new RequestCommandArgs(requestJson);
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

        public event EventHandler<RequestCommandArgs> Request;

        protected virtual void OnRequest(RequestCommandArgs e)
        {
            EventHandler<RequestCommandArgs> handler = Request;
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
                _listener.Close();
            }
        }
    }
}