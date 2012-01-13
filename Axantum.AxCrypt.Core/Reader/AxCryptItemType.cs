using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Reader
{
    public enum AxCryptItemType
    {
        /// <summary>
        /// Initial state before we have read any items at all
        /// </summary>
        None,
        /// <summary>
        /// We have seen the AxCrypt Guid
        /// </summary>
        MagicGuid,
        /// <summary>
        /// A header block of HeaderBlockType has been found
        /// </summary>
        HeaderBlock,
        /// <summary>
        /// A (part) of Encrypted Compressed data has been found
        /// </summary>
        EncryptedCompressedData,
        /// <summary>
        /// A (part) of Compressed data has been found
        /// </summary>
        EncryptedData,
        /// <summary>
        /// The end of the stream has been reached
        /// </summary>
        EndOfStream,
    }
}