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

using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.Header;
using Axantum.AxCrypt.Core.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Forms.Implementation
{
    public class DataProtection : IProtectedData
    {
        private static byte[] _axCryptGuid = AxCrypt1Guid.GetBytes();

        #region IDataProtection Members

        public byte[] Protect(byte[] unprotectedData, byte[] optionalEntropy)
        {
            return ProtectedData.Protect(unprotectedData, optionalEntropy, DataProtectionScope.CurrentUser);
        }

        public byte[] Unprotect(byte[] protectedData, byte[] optionalEntropy)
        {
            if (protectedData.Locate(_axCryptGuid, 0, _axCryptGuid.Length) == 0)
            {
                return null;
            }
            try
            {
                byte[] bytes = ProtectedData.Unprotect(protectedData, optionalEntropy, DataProtectionScope.CurrentUser);
                return bytes;
            }
            catch (CryptographicException cex)
            {
                New<IReport>().Exception(cex);
                return null;
            }
        }

        #endregion IDataProtection Members
    }
}