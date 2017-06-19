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
 * The source is maintained at http://bitbucket.org/axantum/axcrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using Axantum.AxCrypt.Api.Model;
using Axantum.AxCrypt.Common;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Crypto.Asymmetric;
using Axantum.AxCrypt.Core.Service;
using Axantum.AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Core.Session
{
    /// <summary>
    /// Persists a users account information using an IAccountService instance as medium
    /// </summary>
    public class AccountStorage
    {
        private IAccountService _service;

        public AccountStorage(IAccountService service)
        {
            _service = service;
        }

        public async Task<bool> IsIdentityValidAsync()
        {
            return await _service.IsIdentityValidAsync().Free();
        }

        public AccountStorage Refresh()
        {
            _service.Refresh();
            return this;
        }

        public async Task ImportAsync(UserKeyPair keyPair)
        {
            if (keyPair == null)
            {
                throw new ArgumentNullException(nameof(keyPair));
            }

            if (keyPair.UserEmail != _service.Identity.UserEmail)
            {
                throw new ArgumentException("User email mismatch in key pair and store.", nameof(keyPair));
            }

            IList<UserKeyPair> keyPairs = await _service.ListAsync().Free();
            if (keyPairs.Any(k => k == keyPair))
            {
                return;
            }

            keyPairs.Add(keyPair);
            await _service.SaveAsync(keyPairs).Free();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public async virtual Task<IEnumerable<UserKeyPair>> AllKeyPairsAsync()
        {
            return (await _service.ListAsync().Free()).OrderByDescending(uk => uk.Timestamp);
        }

        public async Task<UserKeyPair> ActiveKeyPairAsync()
        {
            return (await AllKeyPairsAsync().Free()).First();
        }

        public EmailAddress UserEmail
        {
            get
            {
                return _service.Identity.UserEmail;
            }
        }

        public async Task<AccountStatus> StatusAsync(EmailAddress email)
        {
            return await _service.StatusAsync(email).Free();
        }

        public async virtual Task ChangePassphraseAsync(Passphrase passphrase)
        {
            if (passphrase == null)
            {
                throw new ArgumentNullException(nameof(passphrase));
            }

            await _service.ChangePassphraseAsync(passphrase).Free();
        }

        public async Task<UserPublicKey> GetOtherUserPublicKeyAsync(EmailAddress email)
        {
            return await _service.OtherPublicKeyAsync(email).Free();
        }
    }
}