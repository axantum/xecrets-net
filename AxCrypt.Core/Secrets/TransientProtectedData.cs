#if DEBUG
#define CODE_ANALYSIS
#endif

#region License

/*
 *  AxCrypt.Xecrets.Core - Xecrets Core and Reference Implementation
 *
 *  Copyright (C) 2010 Svante Seleborg
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  If you'd like to license this program under any other terms than the
 *  above, please contact the author and copyright holder.
 *
 *  Contact: mailto:svante@axantum.com
 */

#endregion License

using AxCrypt.Abstractions;
using AxCrypt.Core.Crypto;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Core.Secrets
{
    /// <summary>
    /// This is essentially a wrapper around the ProtectedData API, with the addition of AppDomain-specific entropy, causing encrypted values to
    /// only be decryptable within the life-time of the AppDomain. If the AppDomain is restarted, new entropy is generated and old values are
    /// no longer possible to decrypt.
    /// </summary>
    public class TransientProtectedData
    {
        /// <summary>
        /// This is what makes the encryption unique to this instance of the AppDomain.
        /// </summary>
        ///
        private readonly object _entropyLock = new object();

        [AllowNull]
        private byte[] _entropy;

        private byte[] Entropy()
        {
            lock (_entropyLock)
            {
                if (_entropy == null)
                {
                    _entropy = New<IRandomGenerator>().Generate(16);
                }
            }
            return _entropy;
        }

        public void Entropy(byte[] value)
        {
            lock (_entropyLock)
            {
                if (value != null)
                {
                    throw new ArgumentException("Only resetting is supported, this will invalidate all previous encryptions.", nameof(value));
                }
                _entropy = value;
            }
        }

        public byte[] Protect(string value)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(value);
            byte[] protectedBytes = Protect(bytes);
            Array.Clear(bytes, 0, bytes.Length);
            return protectedBytes;
        }

        public byte[] Protect(byte[] value)
        {
            byte[] protectedBytes = New<IProtectedData>().Protect(value, Entropy());
            return protectedBytes;
        }

        public bool TryUnprotect(byte[] protectedValue, out string? value)
        {
            value = null;
            if (!TryUnprotect(protectedValue, out byte[]? bytes))
            {
                return false;
            }
            try
            {
                value = Encoding.Unicode.GetString(bytes!, 0, bytes!.Length);
            }
            catch
            {
                return false;
            }
            finally
            {
                if (bytes != null)
                {
                    Array.Clear(bytes, 0, bytes.Length);
                }
            }
            return true;
        }

        public bool TryUnprotect(byte[] protectedValue, out byte[]? bytes)
        {
            bytes = null;
            try
            {
                bytes = New<IProtectedData>().Unprotect(protectedValue, _entropy);
            }
            catch (AxCryptException)
            {
                return false;
            }
            catch (FormatException)
            {
                return false;
            }
            return bytes != null;
        }
    }
}
