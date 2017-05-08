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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Abstractions
{
    public interface IProtectedData
    {
        /// <summary>
        /// Protects the userData parameter and returns a byte array
        /// </summary>
        /// <param name="userData">A byte array containing data to protect</param>
        /// <param name="optionalEntropy">An additional byte array used to encrypt the data</param>
        /// <returns>A byte array representing the encrypted data</returns>
        /// <exception cref="System.ArgumentNullException">The userData parameter is null</exception>
        /// <exception cref="System.NotSupportedException">The operating system does not support this method</exception>
        /// <exception cref="AxCryptException">The cryptographic protection failed</exception>
        /// <exception cref="System.OutOfMemoryException">Out of memory</exception>
        byte[] Protect(byte[] userData, byte[] optionalEntropy);

        /// <summary>
        /// Unprotects the encryptedData parameter and returns a byte array
        /// </summary>
        /// <param name="encryptedData">A byte array containing data encrypted using the System.Security.Cryptography.ProtectedData.Protect(System.Byte[],System.Byte[],System.Security.Cryptography.DataProtectionScope) method</param>
        /// <param name="optionalEntropy">An additional byte array used to encrypt the data</param>
        /// <returns>A byte array representing the encrypted data</returns>
        /// <exception cref="System.ArgumentNullException">The encryptedData parameter is null</exception>
        /// <exception cref="System.NotSupportedException">The operating system does not support this method</exception>
        /// <exception cref="AxCryptException">The cryptographic protection failed</exception>
        /// <exception cref="System.OutOfMemoryException">Out of memory</exception>
        byte[] Unprotect(byte[] encryptedData, byte[] optionalEntropy);
    }
}