#region Coypright and License

/*
 * AxCrypt - Copyright 2016, Svante Seleborg, All Rights Reserved
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
 * The source is maintained at http://bitbucket.org/AxCrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using AxCrypt.Abstractions;
using AxCrypt.Core.Crypto;
using AxCrypt.Core.Crypto.Asymmetric;
using AxCrypt.Core.IO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace AxCrypt.Core.UI
{
    public class FileOperationEventArgs : CancelEventArgs
    {
        public FileOperationEventArgs()
        {
            Status = new FileOperationContext(string.Empty, ErrorStatus.Unknown);
        }

        public string SaveFileFullName { get; set; } = string.Empty;

        public string OpenFileFullName { get; set; } = string.Empty;

        public LogOnIdentity LogOnIdentity { get; set; } = LogOnIdentity.Empty;

        public Guid CryptoId { get; set; }

        public bool ConfirmAll { get; set; }

        public bool Skip { get; set; }

        [AllowNull]
        public IDataStore AxCryptFile { get; set; }

        public FileOperationContext Status { get; set; }

        public IEnumerable<UserPublicKey> Recipients { get; set; } = null!;
    }
}
