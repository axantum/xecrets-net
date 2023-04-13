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

using System;
using System.Linq;
using System.Text.Json.Serialization;

namespace AxCrypt.Core.Session
{
    public class ActiveFileProperties : IEquatable<ActiveFileProperties>
    {
        public ActiveFileProperties(DateTime lastActivityTimeUtc, DateTime lastEncryptionWriteTimeUtc, Guid cryptoId)
        {
            LastActivityTimeUtc = lastActivityTimeUtc;
            LastEncryptionWriteTimeUtc = lastEncryptionWriteTimeUtc;
            CryptoId = cryptoId;
        }

        [JsonPropertyName("cryptoId")]
        public Guid CryptoId { get; set; }

        [JsonPropertyName("lastActivityTimeUtc")]
        public DateTime LastActivityTimeUtc { get; set; }

        /// <summary>
        /// Records the Last Write Time that was valid at the most recent encryption update of the encrypted file.
        /// </summary>

        [JsonPropertyName("lastEncryptionWriteTimeUtc")]
        public DateTime LastEncryptionWriteTimeUtc { get; set; }

        public bool Equals(ActiveFileProperties? other)
        {
            if (other is null)
            {
                return false;
            }

            return CryptoId == other.CryptoId && LastActivityTimeUtc == other.LastActivityTimeUtc && LastEncryptionWriteTimeUtc == other.LastEncryptionWriteTimeUtc;
        }

        public override bool Equals(object? obj)
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

        public static bool operator ==(ActiveFileProperties? left, ActiveFileProperties? right)
        {
            if (Object.ReferenceEquals(left, right))
            {
                return true;
            }
            if (left is null)
            {
                return false;
            }
            return left.Equals(right);
        }

        public static bool operator !=(ActiveFileProperties? left, ActiveFileProperties? right)
        {
            return !(left == right);
        }
    }
}
