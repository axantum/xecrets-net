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
using AxCrypt.Core.Crypto.Asymmetric;
using AxCrypt.Core.Header;
using AxCrypt.Core.Session;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AxCrypt.Core
{
    public interface IAxCryptDocument : IDisposable
    {
        bool PassphraseIsValid { get; }

        string FileName { get; set; }

        DateTime CreationTimeUtc { get; set; }

        DateTime LastAccessTimeUtc { get; set; }

        DateTime LastWriteTimeUtc { get; set; }

        DecryptionParameter DecryptionParameter { get; set; }

        EncryptedProperties Properties { get; }

        ICryptoFactory CryptoFactory { get; }

        IEnumerable<UserPublicKey> AsymmetricRecipients { get; }

        IAsymmetricPublicKey AsymmetricMasterKey { get; }

        IEnumerable<IAsymmetricPublicKey> AsymmetricMasterKeys { get; }

        bool Load(Passphrase passphrase, Guid cryptoId, Headers headers);

        bool Load(IAsymmetricPrivateKey privateKey, Guid cryptoId, Headers headers);

        void EncryptTo(Stream inputStream, Stream outputStream, AxCryptOptions options);

        void DecryptTo(Stream outputPlaintextStream);

        /// <summary>
        /// Verifies the file HMAC
        /// </summary>
        /// <returns>True if the HMAC was correct, false otherwise.</returns>
        bool VerifyHmac();
    }
}