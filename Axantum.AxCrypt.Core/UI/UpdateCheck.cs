#region Coypright and License

/*
 * AxCrypt - Copyright 2014, Svante Seleborg, All Rights Reserved
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

using Axantum.AxCrypt.Api;
using Axantum.AxCrypt.Api.Model;
using Axantum.AxCrypt.Common;
using Axantum.AxCrypt.Core.Extensions;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.UI
{
    public class UpdateCheck : IDisposable
    {
        private class Pair<T, U>
        {
            public T First { get; set; }

            public U Second { get; set; }

            public Pair(T first, U second)
            {
                First = first;
                Second = second;
            }
        }

        public static readonly Version VersionUnknown = new Version(0, 0, 0, 0);

        private Version _currentVersion;

        public UpdateCheck(Version currentVersion)
        {
            _currentVersion = currentVersion;
        }

        public virtual event EventHandler<VersionEventArgs> VersionUpdate;

        private ManualResetEvent _done = new ManualResetEvent(true);

        private readonly object _doneLock = new object();

        /// <summary>
        /// Perform a background version check. The VersionUpdate event is guaranteed to be
        /// raised, regardless of response and result. If a check is already in progress, the
        /// later call is ignored and only one check is performed.
        /// </summary>
        public virtual void CheckInBackground(DateTime lastCheckTimeUtc, string newestKnownVersion, Uri updateWebpageUrl)
        {
            if (newestKnownVersion == null)
            {
                throw new ArgumentNullException("newestKnownVersion");
            }
            if (updateWebpageUrl == null)
            {
                throw new ArgumentNullException("updateWebpageUrl");
            }

            Version newestKnownVersionValue = ParseVersion(newestKnownVersion);

            if (_done == null)
            {
                throw new ObjectDisposedException("_done");
            }
            if (lastCheckTimeUtc.AddDays(1) >= OS.Current.UtcNow)
            {
                if (Resolve.Log.IsInfoEnabled)
                {
                    Resolve.Log.LogInfo("Attempt to check for new version was ignored because it is too soon. Returning version {0}.".InvariantFormat(newestKnownVersionValue));
                }
                OnVersionUpdate(new VersionEventArgs(newestKnownVersionValue, updateWebpageUrl, CalculateStatus(newestKnownVersionValue, lastCheckTimeUtc)));
                return;
            }

            lock (_doneLock)
            {
                if (!_done.WaitOne(TimeSpan.Zero))
                {
                    return;
                }
                _done.Reset();
            }
            Task.Factory.StartNew(async () =>
            {
                try
                {
                    Pair<Version, Uri> newVersion = await CheckWebForNewVersionAsync(updateWebpageUrl).Free();
                    OnVersionUpdate(new VersionEventArgs(newVersion.First, newVersion.Second, CalculateStatus(newVersion.First, lastCheckTimeUtc)));
                }
                finally
                {
                    _done.Set();
                }
            });
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "This is one case where anything could go wrong and it is still required to continue.")]
        private async Task<Pair<Version, Uri>> CheckWebForNewVersionAsync(Uri updateWebpageUrl)
        {
            Version newVersion = VersionUnknown;
            try
            {
                AxCryptVersion axCryptVersion = await New<GlobalApiClient>().AxCryptUpdateAsync().Free();
                newVersion = ParseVersion(axCryptVersion.FullVersion);
                updateWebpageUrl = new Uri(axCryptVersion.DownloadLink);

                if (Resolve.Log.IsInfoEnabled)
                {
                    Resolve.Log.LogInfo("Update check reports most recent version {0} at web page {1}".InvariantFormat(newVersion, updateWebpageUrl));
                }
            }
            catch (Exception ex)
            {
                if (Resolve.Log.IsWarningEnabled)
                {
                    Resolve.Log.LogWarning("Failed call to check for new version with exception {0}.".InvariantFormat(ex));
                }
            }
            return new Pair<Version, Uri>(newVersion, updateWebpageUrl);
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

        private static bool TryParseVersion(string versionString, out Version version)
        {
            version = VersionUnknown;
            if (String.IsNullOrEmpty(versionString))
            {
                return false;
            }
            string[] parts = versionString.Split('.');
            if (parts.Length > 4)
            {
                return false;
            }
            int[] numbers = new int[4];
            for (int i = 0; i < parts.Length; ++i)
            {
                int number;
                if (!Int32.TryParse(parts[i], NumberStyles.None, CultureInfo.InvariantCulture, out number))
                {
                    return false;
                }
                numbers[i] = number;
            }
            version = new Version(numbers[0], numbers[1], numbers[2], numbers[3]);
            return true;
        }

        private static Version ParseVersion(string versionString)
        {
            Version version;
            if (!TryParseVersion(versionString, out version))
            {
                return VersionUnknown;
            }
            if (version.Major == 0 && version.Minor == 0)
            {
                return VersionUnknown;
            }
            return version;
        }

        private VersionUpdateStatus CalculateStatus(Version version, DateTime lastCheckTimeutc)
        {
            if (version > _currentVersion)
            {
                return VersionUpdateStatus.NewerVersionIsAvailable;
            }
            if (version != VersionUnknown)
            {
                return VersionUpdateStatus.IsUpToDateOrRecentlyChecked;
            }
            if (lastCheckTimeutc.AddDays(30) >= OS.Current.UtcNow)
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