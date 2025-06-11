﻿#region Coypright and License

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
 * The source is maintained at http://bitbucket.org/AxCrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

namespace AxCrypt.Core.Header
{
    public enum HeaderBlockType
    {
        /// <summary>
        /// Matches no type.
        /// </summary>
        None = 0,

        /// <summary>
        /// Matches any type.
        /// </summary>
        Any = 1,

        /// <summary>
        /// Must be first.
        /// </summary>
        Preamble = 2,

        /// <summary>
        /// Version information etc.
        /// </summary>
        Version = 3,

        /// <summary>
        /// A 128-bit Data Enc Key and IV wrapped with 128-bit KEK
        /// </summary>
        KeyWrap1 = 4,

        /// <summary>
        /// Some other kind of KEK, DEK, IV scheme... Future use.
        /// </summary>
        KeyWrap2 = 5,

        /// <summary>
        /// An arbitrary string encoded as Ansi Code Page 1252 defined by the caller.
        /// </summary>
        IdTag = 6,

        /// <summary>
        /// HMAC-SHA-512 and end of file marker. Key stream index 0 generates the key.
        /// </summary>
        V2Hmac = 11,

        /// <summary>
        /// NIST Key Wrap Algorithm wrap of the Master Data Encrypting Key, IV, Derviation Salt & Iterations etc.
        /// </summary>
        V2KeyWrap = 13,

        /// <summary>
        /// A RSA OAEP encryption of the Master Data Encrypting Key and IV
        /// </summary>
        V2AsymmetricKeyWrap = 14,

        /// <summary>
        /// A block of encrypted data in the data stream.
        /// </summary>
        /// <remarks>
        /// Technically it's encrypted, but it's not an encrypted header, therefore it's not classified as such.
        /// </remarks>
        EncryptedDataPart = 20,

        /// <summary>
        /// The code should accept and skip header block types that are defined later. This is here to simulate that
        /// condition for tests.
        /// </summary>
        Unrecognized = 61,

        /// <summary>
        /// An undefined header block type, used for tests
        /// </summary>
        Undefined = 62,

        /// <summary>
        /// The data, compressed and/or encrypted.
        /// </summary>
        Data = 63,

        /// <summary>
        /// Start of headers containing encrypted header data
        /// </summary>
        Encrypted = 64,

        /// <summary>
        /// Original file name
        /// </summary>
        FileNameInfo = 65,

        /// <summary>
        /// Sizes of the original data file before encryption
        /// </summary>
        EncryptionInfo = 66,

        /// <summary>
        /// Indicates that the data is compressed and the sizes.
        /// </summary>
        CompressionInfo = 67,

        /// <summary>
        /// Time stamps and size of the original file
        /// </summary>
        FileInfo = 68,

        /// <summary>
        /// Indicates if the data is compressed. 1.2.2.
        /// </summary>
        Compression = 69,

        /// <summary>
        /// Original file name in Unicode. 1.6.3.3
        /// </summary>
        UnicodeFileNameInfo = 70,

        /// <summary>
        /// 2.x - Lengths in the trailer
        /// </summary>
        PlaintextLengths = 101,

        /// <summary>
        /// 2.x - Json-encoded array of email addresses representing key sharing recipients.
        /// </summary>
        AsymmetricRecipients = 102,

        /// <summary>
        /// 2.x - The algorithm verifier header/trailer. Consists of a 16-byte random sequence, followed by 16 bytes 1's complement.
        /// </summary>
        AlgorithmVerifier = 103,

        /// <summary>
        /// [2.x - Json-encoded business subscription public key representing master key.
        /// </summary>
        AsymmetricMasterKey = 104,
        
        /// <summary>
        /// 2.x - Json-encoded business subscription group public keys representing master key.
        /// </summary>
        AsymmetricMasterKeys = 105,
    }
}