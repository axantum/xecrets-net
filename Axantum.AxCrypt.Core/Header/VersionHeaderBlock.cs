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
        public VersionHeaderBlock(HeaderBlockType headerBlockType, byte[] dataBlock)
            : base(headerBlockType, dataBlock)
        {
            FileVersionMajor = dataBlock[0];
            FileVersionMinor = dataBlock[1];
            VersionMajor = dataBlock[2];
            VersionMinor = dataBlock[3];
            VersionMinuscule = dataBlock[4];
        }

        /// <summary>
        /// FileMajor - Older versions cannot not read the format.
        /// </summary>
        public byte FileVersionMajor { get; private set; }

        /// <summary>
        /// FileMinor - Older versions can read the format, but will not retain on save.
        /// </summary>
        public byte FileVersionMinor { get; private set; }

        /// <summary>
        /// Major - New release, major functionality change.
        /// </summary>
        public byte VersionMajor { get; private set; }

        /// <summary>
        /// Minor - Changes, but no big deal.
        /// </summary>
        public byte VersionMinor { get; private set; }

        /// <summary>
        /// Minuscule - bug fix.
        /// </summary>
        public byte VersionMinuscule { get; private set; }
    }
}