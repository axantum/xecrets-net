using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Api.Model
{
    /// <summary>
    /// The current API version. Increment by one for every API change to allow the client to verify that
    /// it is using the right version, and otherwise warn the user.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class ApiVersion : IEquatable<ApiVersion>
    {
        private const int VERSION = 3;

        public static readonly ApiVersion Zero = new ApiVersion(0);

        [JsonProperty("version")]
        public int Version { get; }

        public ApiVersion()
        {
            Version = VERSION;
        }

        private ApiVersion(int version)
        {
            Version = version;
        }

        public bool Equals(ApiVersion other)
        {
            if ((object)other == null)
            {
                return false;
            }

            return Version == other.Version;
        }

        public override bool Equals(object obj)
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

        public static bool operator ==(ApiVersion left, ApiVersion right)
        {
            if (Object.ReferenceEquals(left, right))
            {
                return true;
            }
            if ((object)left == null)
            {
                return false;
            }
            return left.Equals(right);
        }

        public static bool operator !=(ApiVersion left, ApiVersion right)
        {
            return !(left == right);
        }
    }
}