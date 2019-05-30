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
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using Newtonsoft.Json;
using System;
using System.Linq;

namespace Axantum.AxCrypt.Core.Session
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ActiveFileProperties : IEquatable<ActiveFileProperties>
    {
        public ActiveFileProperties(DateTime lastActivityTimeUtc, DateTime lastEncryptionWriteTimeUtc, Guid cryptoId)
        {
            LastActivityTimeUtc = lastActivityTimeUtc;
            CreationTimeUtc = CreationTimeUtc;
            LastEncryptionWriteTimeUtc = lastEncryptionWriteTimeUtc;
            CryptoId = cryptoId;
        }

        [JsonProperty("cryptoId")]
        public Guid CryptoId { get; private set; }

        [JsonProperty("lastActivityTimeUtc")]
        public DateTime LastActivityTimeUtc { get; private set; }

        [JsonProperty("CreationTimeUtc")]
        public DateTime CreationTimeUtc { get; private set; }

        /// <summary>
        /// Records the Last Write Time that was valid at the most recent encryption update of the encrypted file.
        /// </summary>

        [JsonProperty("lastEncryptionWriteTimeUtc")]
        public DateTime LastEncryptionWriteTimeUtc { get; private set; }

        public bool Equals(ActiveFileProperties other)
        {
            if ((object)other == null)
            {
                return false;
            }

            return CryptoId == other.CryptoId && LastActivityTimeUtc == other.LastActivityTimeUtc && LastEncryptionWriteTimeUtc == other.LastEncryptionWriteTimeUtc;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || typeof(ActiveFileProperties) != obj.GetType())
            {
                return false;
            }
            ActiveFileProperties other = (ActiveFileProperties)obj;

            return Equals(other);
        }

        public override int GetHashCode()
        {
            return CryptoId.GetHashCode() ^ LastActivityTimeUtc.GetHashCode() ^ LastEncryptionWriteTimeUtc.GetHashCode();
        }

        public static bool operator ==(ActiveFileProperties left, ActiveFileProperties right)
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

        public static bool operator !=(ActiveFileProperties left, ActiveFileProperties right)
        {
            return !(left == right);
        }
    }
}