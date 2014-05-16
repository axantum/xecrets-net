#region Coypright and License

/*
 * AxCrypt - Copyright 2014, Svante Seleborg, All Rights Reserved
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
using System.Linq;

namespace Axantum.AxCrypt.Core.Crypto
{
    public class Passphrase : IEquatable<Passphrase>
    {
        public static readonly Passphrase Empty = new Passphrase(String.Empty);

        public Passphrase(string text)
        {
            Text = text;
        }

        public string Text { get; private set; }

        private SymmetricKeyThumbprint _thumbprint;

        public SymmetricKeyThumbprint Thumbprint
        {
            get
            {
                if (_thumbprint == null)
                {
                    _thumbprint = new SymmetricKeyThumbprint(this, Instance.UserSettings.ThumbprintSalt, Instance.UserSettings.GetKeyWrapIterations(Instance.CryptoFactory.Minimum.Id));
                }
                return _thumbprint;
            }
        }

        #region IEquatable<Passphrase> Members

        public bool Equals(Passphrase other)
        {
            if ((object)other == null)
            {
                return false;
            }
            return Text == other.Text;
        }

        #endregion IEquatable<Passphrase> Members

        public override bool Equals(object obj)
        {
            if (obj == null || !typeof(Passphrase).IsAssignableFrom(obj.GetType()))
            {
                return false;
            }
            Passphrase other = (Passphrase)obj;

            return Equals(other);
        }

        public override int GetHashCode()
        {
            return Text.GetHashCode();
        }

        public static bool operator ==(Passphrase left, Passphrase right)
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

        public static bool operator !=(Passphrase left, Passphrase right)
        {
            return !(left == right);
        }
    }
}