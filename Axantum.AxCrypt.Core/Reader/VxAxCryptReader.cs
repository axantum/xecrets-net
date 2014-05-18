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

using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Header;
using System;
using System.IO;
using System.Linq;

namespace Axantum.AxCrypt.Core.Reader
{
    public class VXAxCryptReader : AxCryptReader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VXAxCryptReader"/> class.
        /// </summary>
        /// <param name="inputStream">The stream. Will NOT be disposed when this instance is disposed.</param>
        public VXAxCryptReader(Stream inputStream)
            : base(inputStream)
        {
        }

        protected override HeaderBlock HeaderBlockFactory(HeaderBlockType headerBlockType, byte[] dataBlock)
        {
            switch (headerBlockType)
            {
                case HeaderBlockType.Preamble:
                    return new PreambleHeaderBlock(dataBlock);

                case HeaderBlockType.Version:
                    return new VersionHeaderBlock(dataBlock);

                case HeaderBlockType.Data:
                    return new DataHeaderBlock(dataBlock);
            }
            return new UnrecognizedHeaderBlock(headerBlockType, dataBlock);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                PreventInputStreamDispose();
            }
            base.Dispose(disposing);
        }

        private void PreventInputStreamDispose()
        {
            InputStream = null;
        }

        public override IAxCryptDocument Document(Passphrase key, Guid cryptoId, Headers headers)
        {
            throw new NotImplementedException();
        }
    }
}