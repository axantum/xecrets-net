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
 * The source is maintained at http://bitbucket.org/axantum/axcrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axantum.com for more information about the author.
*/

#endregion Coypright and License

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Common
{
    public class DownloadVersion
    {
        public static readonly DownloadVersion Empty = new DownloadVersion(null, VersionZero);

        public static readonly Version VersionUnknown = new Version(0, 0, 0, 0);

        public static readonly Version VersionZero = new Version("2.0.0.0");

        public DownloadVersion(string link, string version, bool isReliablity, bool isSecurity)
        {
            if (link == null)
            {
                throw new ArgumentNullException(nameof(link));
            }
            if (version == null)
            {
                throw new ArgumentNullException(nameof(version));
            }

            Url = link.Length == 0 ? null : new Uri(link);
            Version = Version.Parse(version);

            Level |= isSecurity ? UpdateLevel.Security : UpdateLevel.None;
            Level |= isReliablity ? UpdateLevel.Reliability : UpdateLevel.None;
        }

        public DownloadVersion(Uri url, Version version)
        {
            Url = url;
            Version = version;
        }

        public VersionUpdateStatus CalculateStatus(Version currentVersion, DateTime utcNow, DateTime lastCheckTimeutc)
        {
            if (Version > currentVersion)
            {
                return VersionUpdateStatus.NewerVersionIsAvailable;
            }
            if (Version != VersionUnknown)
            {
                return VersionUpdateStatus.IsUpToDateOrRecentlyChecked;
            }
            if (lastCheckTimeutc.AddDays(30) >= utcNow)
            {
                return VersionUpdateStatus.ShortTimeSinceLastSuccessfulCheck;
            }
            return VersionUpdateStatus.LongTimeSinceLastSuccessfulCheck;
        }

        public UpdateLevel Level { get; }

        public Uri Url { get; }

        public Version Version { get; }
    }
}