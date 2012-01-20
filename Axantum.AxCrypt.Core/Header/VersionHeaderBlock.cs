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
 * The source is maintained at http://AxCrypt.codeplex.com/ please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axantum.com for more information about the author.
*/

#endregion Coypright and License

namespace Axantum.AxCrypt.Core.Header
{
    public class VersionHeaderBlock : HeaderBlock
    {
        public VersionHeaderBlock(byte[] dataBlock)
            : base(HeaderBlockType.Version, dataBlock)
        {
        }

        public VersionHeaderBlock()
            : base(HeaderBlockType.Version, new byte[5])
        {
            FileVersionMajor = 3;
            FileVersionMinor = 2;
            VersionMajor = 2;
            VersionMinor = 0;
            VersionMinuscule = 0;
        }

        /// <summary>
        /// FileMajor - Older versions cannot not read the format.
        /// </summary>
        public byte FileVersionMajor
        {
            get
            {
                return GetDataBlockBytesReference()[0];
            }
            private set
            {
                GetDataBlockBytesReference()[0] = value;
            }
        }

        /// <summary>
        /// FileMinor - Older versions can read the format, but will not retain on save.
        /// </summary>
        public byte FileVersionMinor
        {
            get
            {
                return GetDataBlockBytesReference()[1];
            }
            private set
            {
                GetDataBlockBytesReference()[1] = value;
            }
        }

        /// <summary>
        /// Major - New release, major functionality change.
        /// </summary>
        public byte VersionMajor
        {
            get
            {
                return GetDataBlockBytesReference()[2];
            }
            private set
            {
                GetDataBlockBytesReference()[2] = value;
            }
        }

        /// <summary>
        /// Minor - Changes, but no big deal.
        /// </summary>
        public byte VersionMinor
        {
            get
            {
                return GetDataBlockBytesReference()[3];
            }
            private set
            {
                GetDataBlockBytesReference()[3] = value;
            }
        }

        /// <summary>
        /// Minuscule - bug fix.
        /// </summary>
        public byte VersionMinuscule
        {
            get
            {
                return GetDataBlockBytesReference()[4];
            }
            private set
            {
                GetDataBlockBytesReference()[4] = value;
            }
        }
    }
}