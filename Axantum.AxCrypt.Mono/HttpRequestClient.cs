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