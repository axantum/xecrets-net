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

using AxCrypt.Core.Crypto;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace AxCrypt.Core.UI.ViewModel
{
    public class LogOnEventArgs : EventArgs
    {
        public LogOnEventArgs()
        {
            Passphrase = Passphrase.Empty;
            Identity = LogOnIdentity.Empty;
            UserEmail = String.Empty;
        }

        public bool Cancel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is asking for previously unknown passphrase. This typically results in the UI
        /// asking for a verification of the passphrase in order to ensure correctness.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is asking for previously unknown passphrase; otherwise, <c>false</c>.
        /// </value>
        public bool IsAskingForPreviouslyUnknownPassphrase { get; set; }

        public bool DisplayPassphrase { get; set; }

        public Passphrase Passphrase { get; set; }

        [AllowNull]
        public string Name { get; set; }

        public string UserEmail { get; set; }

        [AllowNull]
        public string EncryptedFileFullName { get; set; }

        public LogOnIdentity Identity { get; set; }
    }
}
