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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Resources;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.System;

namespace Axantum.AxCrypt.Core.UI
{
    public class UpdateCheck : IDisposable
    {
        public static readonly Version VersionUnknown = new Version();

        private Version _currentVersion;

        private Uri _webServiceUrl;

        private Uri _updateWebpageUrl;

        private DateTime _lastCheckUtc;

        public UpdateCheck(Version currentVersion, Uri webServiceUrl, Uri updateWebpageUrl, DateTime lastCheckUtc)
        {
            _currentVersion = currentVersion;
            _webServiceUrl = webServiceUrl;
            _updateWebpageUrl = updateWebpageUrl;
            _lastCheckUtc = lastCheckUtc;
        }

        public event EventHandler<VersionEventArgs> VersionUpdate;

        private ManualResetEvent _done = new ManualResetEvent(true);

        /// <summary>
        /// Perform a background version check. The VersionUpdate event is guaranteed to be
        /// raised, regardless of response and result. If a check is already in progress, the
        /// later call is ignored and only one check is performed.
        /// </summary>
        public void CheckInBackground()
        {
            if (_done == null)
            {
                throw new ObjectDisposedException("_done");
            }
            if (_lastCheckUtc.AddDays(1) >= AxCryptEnvironment.Current.UtcNow)
            {
                OnVersionUpdate(new VersionEventArgs(_currentVersion, _updateWebpageUrl, CalculateStatus(_currentVersion)));
                return;
            }

            lock (_done)
            {
                if (!_done.WaitOne(TimeSpan.Zero, false))
                {
                    return;
                }
                _done.Reset();
            }
            ThreadPool.QueueUserWorkItem((object state) =>
            {
                try
                {
                    Version newVersion = CheckWebForNewVersion();
                    OnVersionUpdate(new VersionEventArgs(newVersion, _updateWebpageUrl, CalculateStatus(newVersion)));
                    if (newVersion != VersionUnknown)
                    {
                        _lastCheckUtc = AxCryptEnvironment.Current.UtcNow;
                    }
                }
                finally
                {
                    _done.Set();
                }
            });
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "This is one case where anything could go wrong and it is still required to continue.")]
        private Version CheckWebForNewVersion()
        {
            Version newVersion = VersionUnknown;
            try
            {
                IWebCaller webCaller = AxCryptEnvironment.Current.CreateWebCaller();
                string result = webCaller.Go(_webServiceUrl);
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(VersionResponse));
                VersionResponse versionResponse;
                using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(result)))
                {
                    versionResponse = (VersionResponse)serializer.ReadObject(stream);
                }
                newVersion = ParseVersion(versionResponse);
                _updateWebpageUrl = new Uri(versionResponse.WebReference);
            }
            catch (Exception ex)
            {
                if (Logging.IsWarningEnabled)
                {
                    Logging.Warning("Failed call to check for new version with exception {0}.".InvariantFormat(ex.Message));
                }
            }
            return newVersion;
        }

        /// <summary>
        /// Wait for the background check (if any) to be complete. When this method returns, the
        /// VersionUpdate event has already been raised.
        /// </summary>
        public void WaitForBackgroundCheckComplete()
        {
            if (_done == null)
            {
                throw new ObjectDisposedException("_done");
            }
            _done.WaitOne();
        }

        private static Version ParseVersion(VersionResponse versionResponse)
        {
            Version version;
            if (!Version.TryParse(versionResponse.Version, out version))
            {
                version = VersionUnknown;
            }
            return version;
        }

        private VersionUpdateStatus CalculateStatus(Version version)
        {
            if (version > _currentVersion)
            {
                return VersionUpdateStatus.NewerVersionIsAvailable;
            }
            if (version != VersionUnknown)
            {
                return VersionUpdateStatus.IsUpToDateOrRecentlyChecked;
            }
            if (_lastCheckUtc.AddDays(30) >= AxCryptEnvironment.Current.UtcNow)
            {
                return VersionUpdateStatus.ShortTimeSinceLastSuccessfulCheck;
            }
            return VersionUpdateStatus.LongTimeSinceLastSuccessfulCheck;
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
            if (_done == null)
            {
                return;
            }
            if (disposing)
            {
                _done.Dispose();
                _done = null;
            }
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