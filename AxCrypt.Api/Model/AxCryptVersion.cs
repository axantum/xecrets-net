using AxCrypt.Common;

using System.Text.Json.Serialization;

namespace AxCrypt.Api.Model
{
    public class AxCryptVersion
    {
        public AxCryptVersion(string downloadLink, VersionUpdateKind kind)
        {
            if (downloadLink == null)
            {
                throw new ArgumentNullException(nameof(downloadLink));
            }
            if (kind == null)
            {
                throw new ArgumentNullException(nameof(kind));
            }

            DownloadLink = downloadLink;
            FullVersion = kind.NewVersion.ToString();
            Revision = kind.NewVersion.Build;
            IsCriticalReliabilityUpdate = kind.NeedsCriticalReliabilityUpdate;
            IsCriticalSecurityUpdate = kind.NeedsCriticalSecurityUpdate;
        }

        [JsonConstructor]
        public AxCryptVersion()
        {
        }

        public static AxCryptVersion Empty { get; } = new AxCryptVersion(String.Empty, VersionUpdateKind.Empty);

        [JsonPropertyName("url")]
        public string? DownloadLink { get; set; }

        [JsonPropertyName("version")]
        public string? FullVersion { get; set; }

        [JsonPropertyName("revision")]
        public int Revision { get; set; }

        [JsonPropertyName("is_critical_reliability_update")]
        public bool IsCriticalReliabilityUpdate { get; set; }

        [JsonPropertyName("is_critical_security_update")]
        public bool IsCriticalSecurityUpdate { get; set; }

        [JsonIgnore]
        public bool IsEmpty
        {
            get
            {
                return String.IsNullOrEmpty(DownloadLink) && FullVersion == Empty.FullVersion && Revision == Empty.Revision && IsCriticalReliabilityUpdate == Empty.IsCriticalReliabilityUpdate && IsCriticalSecurityUpdate == Empty.IsCriticalSecurityUpdate;
            }
        }

        [JsonIgnore]
        public DownloadVersion DownloadVersion
        {
            get
            {
                return new DownloadVersion(DownloadLink, FullVersion, IsCriticalReliabilityUpdate, IsCriticalSecurityUpdate);
            }
        }
    }
}
