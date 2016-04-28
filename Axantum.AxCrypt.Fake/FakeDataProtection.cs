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

using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Fake
{
    public class FakeDataProtection : IDataProtection
    {
        #region IDataProtection Members

        public byte[] Protect(byte[] unprotectedData)
        {
            unprotectedData = unprotectedData.Append(new byte[] { 1, 2, 3, 4 });
            return (byte[])unprotectedData.Xor(new byte[unprotectedData.Length].Fill(0xff));
        }

        public byte[] Unprotect(byte[] protectedData)
        {
            protectedData = (byte[])protectedData.Clone();
            byte[] unprotected = (byte[])protectedData.Xor(new byte[protectedData.Length].Fill(0xff));
            if (unprotected.Length < 4)
            {
                return null;
            }
            byte[] check = new byte[4];
            Array.Copy(unprotected, unprotected.Length - 4, check, 0, 4);
            if (!check.IsEquivalentTo(new byte[] { 1, 2, 3, 4 }))
            {
                return null;
            }
            byte[] value = new byte[unprotected.Length - 4];
            Array.Copy(unprotected, 0, value, 0, unprotected.Length - 4);
            return value;
        }

        #endregion IDataProtection Members
    }
}