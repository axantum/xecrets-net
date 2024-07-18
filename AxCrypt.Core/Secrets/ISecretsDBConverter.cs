using System;
using System.Text;

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

using System.Collections.Generic;

namespace AxCrypt.Core.Secrets
{
    public interface ISecretsDBConverter
    {
        /// <summary>
        /// Generates an encrypted XMLDocument string for each secrets for database migration.
        /// </summary>
        /// <param name="secrets">The secrets.</param>
        StringBuilder SecretsXMLDoc(IEnumerable<Secret> secrets);

        /// <summary>
        /// Generates an encrypted XMLDocument string for each secrets for database migration.
        /// </summary>
        /// <param name="secrets">The secrets.</param>
        SecretCollection SecretsFromXMLDocList(IEnumerable<Api.Model.Secret.SecretApiModel> secrets, IEnumerable<EncryptionKey> keys);

        SecretCollection SecretsFromXMLDoc(Api.Model.Secret.SecretsApiModel secrets, IEnumerable<EncryptionKey> keys);
    }
}