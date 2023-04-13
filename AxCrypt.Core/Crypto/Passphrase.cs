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

using AxCrypt.Core.Extensions;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace AxCrypt.Core.Crypto
{
    public class Passphrase : IEquatable<Passphrase>
    {
        public static readonly Passphrase Empty = new Passphrase(string.Empty);

        public Passphrase(string text)
        {
            Text = text;
        }

        private Passphrase(string text, byte[] extra)
            : this(text)
        {
            _extra = (byte[])extra.Clone();
        }

        private Passphrase(SymmetricKeyThumbprint thumbprint)
        {
            Thumbprint = thumbprint;
        }

        public static Passphrase Create(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return Empty;
            }

            return new Passphrase(text);
        }

        public static Passphrase Create(string text, byte[] extra)
        {
            if (extra == null)
            {
                throw new ArgumentNullException(nameof(extra));
            }
            if (string.IsNullOrEmpty(text) && extra.Length == 0)
            {
                return Empty;
            }

            return new Passphrase(text, extra);
        }

        [NotNull]
        [JsonIgnore]
        public string? Text { get; private set; }

        private readonly byte[] _extra = Array.Empty<byte>();

        public byte[] Extra()
        {
            return (byte[])_extra.Clone();
        }

        private SymmetricKeyThumbprint? _thumbprint;

        [JsonPropertyName("thumbprint")]
        public SymmetricKeyThumbprint? Thumbprint
        {
            get
            {
                if (_thumbprint != null)
                {
                    return _thumbprint;
                }
                if (Text == null && _extra.Length == 0)
                {
                    return null;
                }
                _thumbprint = new SymmetricKeyThumbprint(this, Resolve.UserSettings.ThumbprintSalt, Resolve.UserSettings.GetKeyWrapIterations(Resolve.CryptoFactory.Minimum.CryptoId));
                return _thumbprint;
            }

            set
            {
                _thumbprint = value;
            }
        }

        #region IEquatable<Passphrase> Members

        public bool Equals(Passphrase? other)
        {
            if (other is null)
            {
                return false;
            }
            if (Text != other.Text)
            {
                return false;
            }
            return _extra.IsEquivalentTo(other._extra);
        }

        #endregion IEquatable<Passphrase> Members

        public override bool Equals(object? obj)
        {
            Passphrase? other = obj as Passphrase;
            if (other == null)
            {
                return false;
            }

            return Equals(other);
        }

        public override int GetHashCode()
        {
            return Text.GetHashCode();
        }

        public static bool operator ==(Passphrase? left, Passphrase? right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }
            if ((object?)left == null)
            {
                return false;
            }
            return left.Equals(right);
        }

        public static bool operator !=(Passphrase? left, Passphrase? right)
        {
            return !(left == right);
        }
    }
}
