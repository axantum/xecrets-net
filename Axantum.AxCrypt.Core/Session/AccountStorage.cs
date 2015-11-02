#region Coypright and License

/*
 * AxCrypt - Copyright 2015, Svante Seleborg, All Rights Reserved
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

using Axantum.AxCrypt.Api.Model;
using Axantum.AxCrypt.Core.Crypto;
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

        public async Task<bool> HasKeyPairAsync()
        {
            return (await _service.ListAsync().ConfigureAwait(false)).Any();
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

            IList<UserKeyPair> keyPairs = await _service.ListAsync();
            if (keyPairs.Any(k => k == keyPair))
            {
                return;
            }

            keyPairs.Add(keyPair);
            await _service.SaveAsync(keyPairs).ConfigureAwait(false);
        }

        public async virtual Task<IEnumerable<UserKeyPair>> AllKeyPairsAsync()
        {
            return (await _service.ListAsync().ConfigureAwait(false)).OrderByDescending(uk => uk.Timestamp);
        }

        public async Task<UserKeyPair> ActiveKeyPairAsync()
        {
            return (await AllKeyPairsAsync().ConfigureAwait(false)).First();
        }

        public EmailAddress UserEmail
        {
            get
            {
                return _service.Identity.UserEmail;
            }
        }

        public async Task<AccountStatus> StatusAsync()
        {
            return await _service.StatusAsync().ConfigureAwait(false);
        }

        public async virtual void ChangePassphraseAsync(Passphrase passphrase)
        {
            if (passphrase == null)
            {
                throw new ArgumentNullException(nameof(passphrase));
            }

            _service.ChangePassphrase(passphrase);
            await _service.SaveAsync(await AllKeyPairsAsync().ConfigureAwait(false)).ConfigureAwait(false);
        }
    }
}