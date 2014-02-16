#region Coypright and License

/*
 * AxCrypt - Copyright 2014, Svante Seleborg, All Rights Reserved
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

using Axantum.AxCrypt.Core.Extensions;
using System;
using System.IO;

namespace Axantum.AxCrypt.Core.Header
{
    public abstract class HeaderBlock : ICloneable, IEquatable<HeaderBlock>
    {
        private byte[] _dataBlock;

        protected HeaderBlock(HeaderBlockType headerBlockType, byte[] dataBlock)
        {
            HeaderBlockType = headerBlockType;
            _dataBlock = dataBlock;
        }

        protected HeaderBlock(HeaderBlockType headerBlockType)
        {
            HeaderBlockType = headerBlockType;
        }

        public HeaderBlockType HeaderBlockType { get; protected set; }

        /// <summary>
        /// Get a reference to the internally maintained data block. Beware modifying the contents of the array!
        /// </summary>
        /// <returns></returns>
        protected byte[] GetDataBlockBytesReference()
        {
            return _dataBlock;
        }

        /// <summary>
        /// Get a copy of the raw data block.
        /// </summary>
        /// <returns>Returns a cloned copy of the data block.</returns>
        public byte[] GetDataBlockBytes()
        {
            return (byte[])GetDataBlockBytesReference().Clone();
        }

        /// <summary>
        /// Set a reference to the internally maintained data block. Beware modifying the contents of the array!
        /// </summary>
        /// <param name="dataBlock"></param>
        protected void SetDataBlockBytesReference(byte[] dataBlock)
        {
            _dataBlock = dataBlock;
        }

        protected byte[] GetPrefixBytes()
        {
            byte[] headerBlockPrefix = new byte[4 + 1];
            BitConverter.GetBytes(headerBlockPrefix.Length + _dataBlock.Length).CopyTo(headerBlockPrefix, 0);
            headerBlockPrefix[4] = (byte)HeaderBlockType;

            return headerBlockPrefix;
        }

        protected void WritePrefix(Stream stream)
        {
            byte[] headerPrefixBytes = GetPrefixBytes();
            stream.Write(headerPrefixBytes, 0, headerPrefixBytes.Length);
        }

        public virtual void Write(Stream stream)
        {
            WritePrefix(stream);
            stream.Write(_dataBlock, 0, _dataBlock.Length);
        }

        public abstract object Clone();

        public bool Equals(HeaderBlock other)
        {
            if (other == null)
            {
                return false;
            }
            return GetDataBlockBytesReference().IsEquivalentTo(other.GetDataBlockBytesReference());
        }

        public override bool Equals(object obj)
        {
            HeaderBlock other = obj as HeaderBlock;
            if (other == null)
            {
                return false;
            }

            return Equals(other);
        }

        public override int GetHashCode()
        {
            int hashcode = 0;
            foreach (byte b in GetDataBlockBytesReference())
            {
                hashcode += b;
            }
            return hashcode;
        }

        public static bool operator ==(HeaderBlock left, HeaderBlock right)
        {
            if (Object.ReferenceEquals(left, right))
            {
                return true;
            }
            if ((object)left == null)
            {
                return false;
            }
            return left.Equals(right);
        }

        public static bool operator !=(HeaderBlock left, HeaderBlock right)
        {
            return !(left == right);
        }
    }
}