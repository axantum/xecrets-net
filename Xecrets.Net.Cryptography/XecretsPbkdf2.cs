#region Coypright and GPL License

/*
 * Xecrets.Net - Copyright © 2022-2026, Svante Seleborg, All Rights Reserved.
 *
 * This code file is part of Xecrets.Net, parts of which in turn are derived from AxCrypt as licensed under GPL v3 or later.
 * 
 * However, this code is not derived from AxCrypt and is separately copyrighted and only licensed as follows unless
 * explicitly licensed otherwise. If you use any part of this code in your software, please see https://www.gnu.org/licenses/
 * for details of what this means for you.
 *
 * Xecrets.Net is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * Xecrets.Net is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied
 * warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License along with Xecrets.Net.  If not, see <https://www.gnu.org/licenses/>.
 *
 * The source repository can be found at https://github.com/axantum/xecrets-net please go there for more information,
 * suggestions and contributions. You may also visit https://www.axantum.com for more information about the author.
*/

#endregion Coypright and GPL License

using System.Security.Cryptography;
using System.Text;

namespace Xecrets.Net.Cryptography
{
    // This replaces AxCrypt.Core.Crypto.Pbkdf2HmacSha512.
    public class XecretsPbkdf2
    {
        private byte[]? _bytes;

        public XecretsPbkdf2(string password, ReadOnlySpan<byte> salt, int derivationIterations, int outputLength = 64)
        {
            ArgumentNullException.ThrowIfNull(password);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(derivationIterations);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(outputLength);

            // The password is the HMAC key, encoded as UTF-8 without a byte order mark.
            _bytes = Rfc2898DeriveBytes.Pbkdf2(new UTF8Encoding(false).GetBytes(password), salt, derivationIterations,
                HashAlgorithmName.SHA512, outputLength);
        }

        public byte[] GetBytes()
        {
            if (_bytes == null)
            {
                throw new InvalidOperationException("The key bytes can only be read once.");
            }

            byte[] bytes = _bytes;
            _bytes = null;
            return bytes;
        }
    }
}
