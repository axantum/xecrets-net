#region License

/*
 *  Axantum.Xecrets.Core - Xecrets Core and Reference Implementation
 *
 *  Copyright (C) 2008 Svante Seleborg
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  If you'd like to license this program under any other terms than the
 *  above, please contact the author and copyright holder.
 *
 *  Contact: mailto:svante@axantum.com
 */

#endregion License

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace AxCrypt.Core.Secrets
{
    public interface ISecretsWriter
    {
        /// <summary>
        /// Gets a stream open for reading the persisted data.
        /// </summary>
        /// <returns>A stream to read the data from.</returns>
        Stream OpenDataStream();

        /// <summary>
        /// Saves the secrets to the data, possibly preserving metadata and undecrypted
        /// secrets left from the reader.
        /// </summary>
        /// <param name="secrets">The secrets.</param>
        void SaveSecrets(IEnumerable<Secret> secrets, Stream stream);
    }
}