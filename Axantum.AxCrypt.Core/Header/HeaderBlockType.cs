using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Header
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
        /// An arbitrary string defined by the caller.
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