#region Coypright and License

/*
 * AxCrypt - Copyright 2012, Svante Seleborg, All Rights Reserved
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

namespace Axantum.AxCrypt.Core.Crypto
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
        Preamble,
        /// <summary>
        /// Version information etc.
        /// </summary>
        Version,
        /// <summary>
        /// A 128-bit Data Enc Key and IV wrapped with 128-bit KEK
        /// </summary>
        KeyWrap1,
        /// <summary>
        /// Some other kind of KEK, DEK, IV scheme... Future use.
        /// </summary>
        KeyWrap2,
        /// <summary>
        /// An arbitrary string encoded as Ansi Code Page 1252 defined by the caller.
        /// </summary>
        IdTag,
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
        FileNameInfo,
        /// <summary>
        /// Sizes of the original data file before encryption
        /// </summary>
        EncryptionInfo,
        /// <summary>
        /// Indicates that the data is compressed and the sizes.
        /// </summary>
        CompressionInfo,
        /// <summary>
        /// Time stamps and size of the original file
        /// </summary>
        FileInfo,
        /// <summary>
        /// Indicates if the data is compressed. 1.2.2.
        /// </summary>
        Compression,
        /// <summary>
        /// Original file name in Unicode. 1.6.3.3
        /// </summary>
        UnicodeFileNameInfo,
    }
}