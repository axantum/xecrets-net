#region Copyright and GPL License

/*
 * Xecrets Cli - Copyright © 2022-2024, Svante Seleborg, All Rights Reserved.
 *
 * This code file is part of Xecrets Cli, parts of which in turn are derived from AxCrypt as licensed under GPL v3 or later.
 * 
 * However, this code is not derived from AxCrypt and is separately copyrighted and only licensed as follows unless
 * explicitly licensed otherwise. If you use any part of this code in your software, please see https://www.gnu.org/licenses/
 * for details of what this means for you.
 *
 * Xecrets Cli is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * Xecrets Cli is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied
 * warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License along with Xecrets Cli.  If not, see <https://www.gnu.org/licenses/>.
 *
 * The source repository can be found at https://github.com/ please go there for more information, suggestions and
 * contributions. You may also visit https://www.axantum.com for more information about the author.
*/

#endregion Copyright and GPL License

using System.Security.Cryptography;

using AxCrypt.Core.Crypto;

namespace Xecrets.Net.Core.Crypto.Asymmetric;

internal class NetPaddingHash : ICryptoHash
{
    private HashAlgorithm? _hash;

    public string AlgorithmName { get; }

    public int HashSize => Hash.HashSize / 8;

    public int BufferLength => HashSize;

    private HashAlgorithm Hash => _hash ??= CreateHash();

    public NetPaddingHash(int keyBits)
    {
        if (keyBits < 1024)
        {
            AlgorithmName = "MD5";
            return;
        }
        if (keyBits < 2048)
        {
            AlgorithmName = "SHA256";
            return;
        }

        AlgorithmName = "SHA512";
    }

    public void BlockUpdate(byte[] input, int offset, int length)
    {
        Hash.TransformBlock(input, offset, length, null, 0);
    }

    public int DoFinal(byte[] output, int offset)
    {
        Hash.TransformFinalBlock([], 0, 0);
        Hash.Hash?.CopyTo(output, offset);

        int hashSize = Hash.HashSize / 8;

        Reset();

        return hashSize;
    }

    public void Reset()
    {
        _hash?.Clear();
        _hash = null;
    }

    private HashAlgorithm CreateHash()
    {
        return AlgorithmName switch
        {
            "MD5" => MD5.Create(),
            "SHA256" => SHA256.Create(),
            "SHA512" => SHA512.Create(),
            _ => throw new InvalidOperationException("Can't create hash"),
        };
    }

    public void Update(byte input)
    {
        Hash.TransformBlock([input], 0, 1, null, 0);
    }
}
