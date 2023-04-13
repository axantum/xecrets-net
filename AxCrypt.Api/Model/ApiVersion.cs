using System.Text.Json.Serialization;

namespace AxCrypt.Api.Model
{
    /// <summary>
    /// The current API version. Increment by one for every API change to allow the client to verify that
    /// it is using the right version, and otherwise warn the user.
    /// </summary>
    public class ApiVersion : IEquatable<ApiVersion>
    {
        private const int VERSION = 3;

        public static readonly ApiVersion Zero = new ApiVersion(0);

        [JsonPropertyName("version")]
        public int Version { get; set; }

        public ApiVersion()
        {
            Version = VERSION;
        }

        private ApiVersion(int version)
        {
            Version = version;
        }

        public bool Equals(ApiVersion? other)
        {
            if ((object?)other == null)
            {
                return false;
            }

            return Version == other.Version;
        }

        public override bool Equals(object? obj)
        {
            if (obj == null || typeof(ApiVersion) != obj.GetType())
            {
                return false;
            }
            ApiVersion other = (ApiVersion)obj;

            return Equals(other);
        }

        public override int GetHashCode()
        {
            return Version.GetHashCode();
        }

        public static bool operator ==(ApiVersion? left, ApiVersion? right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }
            if (left is null)
            {
                return false;
            }
            return left.Equals(right);
        }

        public static bool operator !=(ApiVersion? left, ApiVersion? right)
        {
            return !(left == right);
        }
    }
}
