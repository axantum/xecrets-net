#region Coypright and License

/*
 * AxCrypt - Copyright 2012, Svante Seleborg, All Rights Reserved
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using Axantum.AxCrypt.Core.IO;

namespace Axantum.AxCrypt.Core.UI
{
    public class UpdateCheck : IDisposable
    {
        private Version _currentVersion;

        private Uri _url;

        public UpdateCheck(Version currentVersion, Uri url)
        {
            _currentVersion = currentVersion;
            _url = url;
        }

        public event EventHandler<VersionEventArgs> VersionUpdate;

        private ManualResetEvent _done = new ManualResetEvent(false);

        public void Check()
        {
            _done.Reset();
            ThreadPool.QueueUserWorkItem((object state) =>
            {
                IWebCaller webCaller = AxCryptEnvironment.Current.CreateWebCaller();
                string result = webCaller.Go(_url);
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(VersionResponse));
                VersionResponse versionResponse;
                using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(result)))
                {
                    versionResponse = (VersionResponse)serializer.ReadObject(stream);
                }
                CheckVersion(versionResponse);
                _done.Set();
            });
        }

        public void Wait()
        {
            _done.WaitOne();
        }

        private void CheckVersion(VersionResponse versionResponse)
        {
            Version version;
            if (!Version.TryParse(versionResponse.Version, out version))
            {
                return;
            }

            OnVersionUpdate(new VersionEventArgs(version > _currentVersion, version, new Uri(versionResponse.WebReference), AxCryptEnvironment.Current.UtcNow));
        }

        protected virtual void OnVersionUpdate(VersionEventArgs e)
        {
            EventHandler<VersionEventArgs> handler = VersionUpdate;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }
            if (_done == null)
            {
                return;
            }
            _done.Dispose();
            _done = null;
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Members
    }
}