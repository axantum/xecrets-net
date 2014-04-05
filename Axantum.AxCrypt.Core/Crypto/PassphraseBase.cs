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
    public abstract class PassphraseBase : IPassphrase
    {
        public Guid CryptoId
        {
            get;
            protected set;
        }

        public SymmetricKey DerivedKey
        {
            get;
            protected set;
        }

        public string Passphrase
        {
            get;
            protected set;
        }

        public Salt DerivationSalt
        {
            get;
            protected set;
        }

        public long DerivationIterations { get; protected set; }

        private SymmetricKeyThumbprint _thumbprint;

        public SymmetricKeyThumbprint Thumbprint
        {
            get
            {
                if (_thumbprint == null)
                {
                    _thumbprint = new SymmetricKeyThumbprint(Passphrase, Instance.UserSettings.ThumbprintSalt, Instance.UserSettings.GetKeyWrapIterations(Instance.CryptoFactory.Preferrred.Id));
                }
                return _thumbprint;
            }
        }

        #region IEquatable<SymmetricKey> Members

        /// <summary>
        /// Check if one instance is equivalent to another.
        /// </summary>
        /// <param name="other">The instance to compare to</param>
        /// <returns>true if the keys are equivalent</returns>
        public bool Equals(IPassphrase other)
        {
            if ((object)other == null)
            {
                return false;
            }
            return CryptoId == other.CryptoId && DerivedKey == other.DerivedKey && Passphrase == other.Passphrase;
        }

        #endregion IEquatable<SymmetricKey> Members

        public override bool Equals(object obj)
        {
            if (obj == null || !typeof(IPassphrase).IsAssignableFrom(obj.GetType()))
            {
                return false;
            }
            IPassphrase other = (IPassphrase)obj;

            return Equals(other);
        }

        public override int GetHashCode()
        {
            return CryptoId.GetHashCode() ^ DerivedKey.GetHashCode() ^ Passphrase.GetHashCode();
        }
    }
}