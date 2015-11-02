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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Core.Service
{
    public class NullAccountService : IAccountService
    {
        public NullAccountService(LogOnIdentity identity)
        {
            Identity = identity;
        }

        public Task<UserAccount> AccountAsync()
        {
            return Task.Run(() => new UserAccount(Identity.UserEmail.Address));
        }

        public bool HasAccounts
        {
            get
            {
                return false;
            }
        }

        public LogOnIdentity Identity
        {
            get; private set;
        }

        public SubscriptionLevel Level
        {
            get
            {
                return SubscriptionLevel.Unknown;
            }
        }

        public Task<AccountStatus> StatusAsync()
        {
            return Task.FromResult(AccountStatus.Unknown);
        }

        public bool ChangePassphrase(Passphrase passphrase)
        {
            Identity = new LogOnIdentity(Identity.UserEmail, passphrase);
            return true;
        }

        public Task<IList<UserKeyPair>> ListAsync()
        {
            return Task.FromResult((IList<UserKeyPair>)new UserKeyPair[0]);
        }

        public Task SaveAsync(IEnumerable<UserKeyPair> keyPairs)
        {
            return Task.Run(() => { });
        }

        public Task SignupAsync(string emailAddress)
        {
            return Task.Run(() => { });
        }

        public Task PasswordResetAsync(string verificationCode)
        {
            return Task.Run(() => { });
        }
    }
}