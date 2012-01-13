using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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