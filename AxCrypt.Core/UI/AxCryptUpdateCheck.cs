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
using AxCrypt.Api;
using AxCrypt.Api.Model;
using AxCrypt.Common;
using AxCrypt.Core.Extensions;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading.Tasks;
using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Core.UI
{
    public class AxCryptUpdateCheck
    {
        private Version _currentVersion;

        public AxCryptUpdateCheck(Version currentVersion)
        {
            _currentVersion = currentVersion;
        }

        public virtual event EventHandler<VersionEventArgs> AxCryptUpdate;

        private bool _inProgress;

        /// <summary>
        /// Perform a background version check. The VersionUpdate event is guaranteed to be
        /// raised, regardless of response and result. If a check is already in progress, the
        /// later call is ignored and only one check is performed.
        /// </summary>
        public virtual async Task CheckInBackgroundAsync(DateTime lastCheckTimeUtc, string newestKnownVersion, Uri updateWebpageUrl, string cultureName)
        {
            if (newestKnownVersion == null)
            {
                throw new ArgumentNullException(nameof(newestKnownVersion));
            }
            if (updateWebpageUrl == null)
            {
                throw new ArgumentNullException(nameof(updateWebpageUrl));
            }
            if (cultureName == null)
            {
                throw new ArgumentNullException(nameof(cultureName));
            }

            Version newestKnownVersionValue = ParseVersion(newestKnownVersion);

            if (lastCheckTimeUtc.AddDays(1) >= New<INow>().Utc)
            {
                if (Resolve.Log.IsInfoEnabled)
                {
                    Resolve.Log.LogInfo("Attempt to check for new version was ignored because it is too soon. Returning version {0}.".InvariantFormat(newestKnownVersionValue));
                }
                OnVersionUpdate(new VersionEventArgs(new DownloadVersion(updateWebpageUrl, newestKnownVersionValue), lastCheckTimeUtc));
                return;
            }

            if (_inProgress)
            {
                return;
            }
            _inProgress = true;
            try
            {
                DownloadVersion newVersion = await CheckWebForNewVersionAsync(updateWebpageUrl, cultureName).Free();
                if (newVersion.Url != null)
                {
                    OnVersionUpdate(new VersionEventArgs(newVersion, lastCheckTimeUtc));
                }
            }
            finally
            {
                _inProgress = false;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "This is one case where anything could go wrong and it is still required to continue.")]
        private async Task<DownloadVersion> CheckWebForNewVersionAsync(Uri updateWebpageUrl, string cultureName)
        {
            Version newVersion = DownloadVersion.VersionUnknown;
            try
            {
                ClientPlatformKind platform = ClientPlatformKind.WindowsDesktop;
                switch (OS.Current.Platform)
                {
                    case Runtime.Platform.WindowsDesktop:
                        platform = ClientPlatformKind.WindowsDesktop;
                        break;

                    case Runtime.Platform.MacOsx:
                        platform = ClientPlatformKind.Mac;
                        break;

                    default:
                        throw new NotSupportedException($"App doesn't support updating on {OS.Current.Platform} platform");
                }

                AxCryptVersion axCryptVersion = await New<AxCryptApiClient>().AxCryptUpdateAsync(_currentVersion, cultureName, platform).Free();

                if (!axCryptVersion.IsEmpty)
                {
                    if (Resolve.Log.IsInfoEnabled)
                    {
                        Resolve.Log.LogInfo("Update check reports most recent version {0} at web page {1}".InvariantFormat(newVersion, updateWebpageUrl));
                    }
                }

                return axCryptVersion.DownloadVersion;
            }
            catch (ApiException aex)
            {
                await aex.HandleApiExceptionAsync();
            }
            catch (Exception ex)
            {
                New<IReport>().Exception(ex);
                if (Resolve.Log.IsWarningEnabled)
                {
                    Resolve.Log.LogWarning("Failed call to check for new version with exception {0}.".InvariantFormat(ex));
                }
            }
            return new DownloadVersion(updateWebpageUrl, DownloadVersion.VersionUnknown);
        }

        private static bool TryParseVersion(string versionString, out Version version)
        {
            version = DownloadVersion.VersionUnknown;
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
                return DownloadVersion.VersionUnknown;
            }
            if (version.Major == 0 && version.Minor == 0)
            {
                return DownloadVersion.VersionUnknown;
            }
            return version;
        }

        protected virtual void OnVersionUpdate(VersionEventArgs e)
        {
            AxCryptUpdate?.Invoke(this, e);
        }
    }
}