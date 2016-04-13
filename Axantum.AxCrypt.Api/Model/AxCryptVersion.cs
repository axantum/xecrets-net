using Axantum.AxCrypt.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Api.Model
{
    [JsonObject(MemberSerialization.OptIn)]
    public class AxCryptVersion
    {
        public AxCryptVersion(string downloadLink, VersionUpdateKind kind)
        {
            DownloadLink = downloadLink;
            FullVersion = kind.NewVersion.ToString();
            Revision = kind.NewVersion.Build;
            IsCriticalReliabilityUpdate = kind.NeedsCriticalReliabilityUpdate;
            IsCriticalSecurityUpdate = kind.NeedsCriticalSecurityUpdate;
        }

        [JsonConstructor]
        private AxCryptVersion()
        {
        }

        public static AxCryptVersion Empty { get; } = new AxCryptVersion(String.Empty, VersionUpdateKind.Empty);

        [JsonProperty("url")]
        public string DownloadLink { get; private set; }

        [JsonProperty("version")]
        public string FullVersion { get; private set; }

        [JsonProperty("revision")]
        public int Revision { get; private set; }

        [JsonProperty("is_critical_reliability_update")]
        public bool IsCriticalReliabilityUpdate { get; private set; }

        [JsonProperty("is_critical_security_update")]
        public bool IsCriticalSecurityUpdate { get; private set; }

        public bool IsEmpty
        {
            get
            {
                return String.IsNullOrEmpty(DownloadLink) && FullVersion == Empty.FullVersion && Revision == Empty.Revision && IsCriticalReliabilityUpdate == Empty.IsCriticalReliabilityUpdate && IsCriticalSecurityUpdate == Empty.IsCriticalSecurityUpdate;
            }
        }
    }
}