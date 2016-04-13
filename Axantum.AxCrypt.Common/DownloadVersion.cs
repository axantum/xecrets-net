using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Common
{
    public class DownloadVersion
    {
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

        public UpdateLevel Level { get; }

        public Uri Url { get; }

        public Version Version { get; }
    }
}