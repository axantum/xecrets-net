#region Coypright and License

/*
 * AxCrypt - Copyright 2017, Svante Seleborg, All Rights Reserved
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

using Axantum.AxCrypt.Core.Crypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Core.Runtime
{
    public class LicenseCapabilities : IEquatable<LicenseCapabilities>
    {
        private ISet<LicenseCapability> _capabilities;

        public LicenseCapabilities(ISet<LicenseCapability> capabilities)
        {
            _capabilities = capabilities;
        }

        public bool Has(LicenseCapability capability)
        {
            return _capabilities.Contains(capability);
        }

        public ICryptoPolicy CryptoPolicy
        {
            get
            {
                return Has(LicenseCapability.StrongerEncryption) ? new ProCryptoPolicy() as ICryptoPolicy : new FreeCryptoPolicy() as ICryptoPolicy;
            }
        }

        #region IEquatable<LicenseCapabilities> Members

        public bool Equals(LicenseCapabilities other)
        {
            if ((object)other == null)
            {
                return false;
            }
            if (_capabilities != other._capabilities)
            {
                return false;
            }
            return true;
        }

        #endregion IEquatable<LicenseCapabilities> Members

        public override bool Equals(object obj)
        {
            LicenseCapabilities other = obj as LicenseCapabilities;
            if (other == null)
            {
                return false;
            }

            return Equals(other);
        }

        public override int GetHashCode()
        {
            return _capabilities.GetHashCode();
        }

        public static bool operator ==(LicenseCapabilities left, LicenseCapabilities right)
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

        public static bool operator !=(LicenseCapabilities left, LicenseCapabilities right)
        {
            return !(left == right);
        }
    }
}