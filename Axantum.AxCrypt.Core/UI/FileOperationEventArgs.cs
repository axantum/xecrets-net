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

using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Crypto.Asymmetric;
using Axantum.AxCrypt.Core.IO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Axantum.AxCrypt.Core.UI
{
    public class FileOperationEventArgs : CancelEventArgs
    {
        public FileOperationEventArgs()
        {
            Status = new FileOperationContext(String.Empty, ErrorStatus.Unknown);
            SharedPublicKeys = new UserPublicKey[0];
        }

        public string SaveFileFullName { get; set; }

        public string OpenFileFullName { get; set; }

        public LogOnIdentity LogOnIdentity { get; set; }

        public IEnumerable<UserPublicKey> SharedPublicKeys { get; set; }

        public Guid CryptoId { get; set; }

        public bool ConfirmAll { get; set; }

        public bool Skip { get; set; }

        public IDataStore AxCryptFile { get; set; }

        public FileOperationContext Status { get; set; }
    }
}