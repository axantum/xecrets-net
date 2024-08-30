#region Xecrets Cli Copyright and GPL License notice

/*
 * Xecrets Cli - Changes and additions copyright © 2022-2024, Svante Seleborg, All Rights Reserved.
 *
 * This code file is part of Xecrets Cli, but is derived from AxCrypt as licensed under GPL v3 or later.
 * 
 * The changes and additions are separately copyrighted and only licensed under GPL v3 or later as detailed below,
 * unless explicitly licensed otherwise. If you use any part of these changes and additions in your software,
 * please see https://www.gnu.org/licenses/ for details of what this means for you.
 * 
 * Warning: If you are using the original AxCrypt code under a non-GPL v3 or later license, these changes and additions
 * are not included in that license. If you use these changes under those circumstances, all your code becomes subject to
 * the GPL v3 or later license, according to the principle of strong copyleft as applied to GPL v3 or later.
 *
 * Xecrets Cli is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * Xecrets Cli is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied
 * warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License along with Xecrets Cli. If not, see
 * https://www.gnu.org/licenses/.
 *
 * The source repository can be found at https://github.com/axantum/xecrets-net please go there for more information,
 * suggestions and contributions, as well for commit history detailing changes and additions that fall under the strong
 * copyleft provisions mentioned above. You may also visit https://www.axantum.com for more information about the author.
*/

#endregion Xecrets Cli Copyright and GPL License notice

using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;

using AxCrypt.Core.Crypto;

namespace Xecrets.Net.Core.Test.LegacyImplementation;

internal class FakePaddingHashFactory : IPaddingHashFactory
{
    private readonly string _paddingHashAlgorithm;

    public FakePaddingHashFactory(string paddingHashAlgorithm)
    {
        _paddingHashAlgorithm = paddingHashAlgorithm;
    }

    public ICryptoHash CreatePaddingHash(int keyBits)
    {
        return new FakePaddingHash(_paddingHashAlgorithm);
    }

    private class FakePaddingHash : ICryptoHash
    {
        [AllowNull]
        private HashAlgorithm _hash;

        private readonly string _paddingHashAlgorithm;

        public FakePaddingHash(string paddingHashAlgorithm)
        {
            _paddingHashAlgorithm = paddingHashAlgorithm;
            Reset();
        }

        public string AlgorithmName
        {
            get { return _paddingHashAlgorithm; }
        }

        public int HashSize
        {
            get { return _hash.HashSize / 8; }
        }

        public int BufferLength
        {
            get { return _hash.InputBlockSize / 8; }
        }

        public void Update(byte input)
        {
            byte[] buffer = new byte[] { input };
            _ = _hash.TransformBlock(buffer, 0, 1, buffer, 0);
        }

        public void BlockUpdate(byte[] input, int offset, int length)
        {
            _ = _hash.TransformBlock(input, offset, length, input, offset);
        }

        public int DoFinal(byte[] output, int offset)
        {
            _ = _hash.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
            _hash.Hash!.CopyTo(array: output, offset);
            Reset();
            return _hash.HashSize / 8;
        }

        public void Reset()
        {
            _hash = _paddingHashAlgorithm switch
            {
                "SHA1" => SHA1.Create(),
                "MD5" => MD5.Create(),
                _ => throw new ArgumentException("Unsupported hash algorithm.", nameof(_paddingHashAlgorithm)),
            };
        }
    }
}
