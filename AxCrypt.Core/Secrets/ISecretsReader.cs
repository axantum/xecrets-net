#region License

/*
 *  AxCrypt.Xecrets.Core - Xecrets Core and Reference Implementation
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
using System.Collections;

namespace AxCrypt.Core.Secrets
{
    public enum FormatConfidence
    {
        InvalidConfidenceLevel = 0,
        DefinitelyNot,
        CannotDetermine,
        Maybe,
        Probably,
        Definitely
    }

    public interface ISecretsReader
    {
        /// <summary>
        /// Gets the level of confidence in that we can read the data.
        /// </summary>
        /// <param name="keys">The key collection.</param>
        /// <returns>A value indicating the level of confidence.</returns>
        FormatConfidence DetermineFormatConfidence(EncryptionKeyCollection keyCollection);

        /// <summary>
        /// Gets the secrets.
        /// </summary>
        /// <param name="keys">The key collection.</param>
        /// <returns>A collection of secrets.</returns>
        /// <exception cref="System.FormatException">If the format cannot be interpreted.</exception>
        SecretCollection FindSecrets(IEnumerable<EncryptionKey> keys);

        /// <summary>
        /// Determines whether the read data has more secrets not being decryptable with the specified key collection.
        /// </summary>
        /// <param name="keys">The key collection.</param>
        /// <returns>
        /// 	<c>true</c> if [has more secrets] [the specified key collection]; otherwise, <c>false</c>.
        /// </returns>
        bool HasMoreSecrets(IEnumerable<EncryptionKey> keys);
    }
}