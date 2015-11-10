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
        public AxCryptVersion(string downloadLink, string fullVersion, int revision)
        {
            DownloadLink = downloadLink;
            FullVersion = fullVersion;
            Revision = revision;
        }

        public static AxCryptVersion Empty { get; } = new AxCryptVersion(String.Empty, String.Empty, 0);

        [JsonProperty("url")]
        public string DownloadLink { get; private set; }

        [JsonProperty("version")]
        public string FullVersion { get; private set; }

        [JsonProperty("revision")]
        public int Revision { get; private set; }

        public bool IsEmpty
        {
            get
            {
                return String.IsNullOrEmpty(DownloadLink) && String.IsNullOrEmpty(FullVersion) && Revision == 0;
            }
        }
    }
}