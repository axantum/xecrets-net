﻿#region Coypright and License

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
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Crypto.Asymmetric;
using Axantum.AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Core.Service
{
    public class NullAccountService : IAccountService
    {
        private static readonly Task _completedTask = Task.FromResult(true);

        public NullAccountService(LogOnIdentity identity)
        {
            Identity = identity;
        }

        public IAccountService Refresh()
        {
            return this;
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

        public Task<AccountStatus> StatusAsync(EmailAddress email)
        {
            return Task.FromResult(AccountStatus.Unknown);
        }

        public Task<Offers> OffersAsync()
        {
            return Task.FromResult(Offers.None);
        }

        public Task StartPremiumTrialAsync()
        {
            return Task.FromResult(default(object));
        }

        public Task<bool> ChangePassphraseAsync(Passphrase passphrase)
        {
            Identity = new LogOnIdentity(Identity.UserEmail, passphrase);
            return Task.FromResult(true);
        }

        /// <summary>
        /// Fetches the user user account.
        /// </summary>
        /// <returns>
        /// The complete user account information.
        /// </returns>
        public Task<UserAccount> AccountAsync()
        {
            return Task.FromResult(new UserAccount(Identity.UserEmail.Address, SubscriptionLevel.Unknown, AccountStatus.Unknown, Offers.None));
        }

        public Task<IList<UserKeyPair>> ListAsync()
        {
            return Task.FromResult((IList<UserKeyPair>)new UserKeyPair[0]);
        }

        public Task SaveAsync(UserAccount account)
        {
            return _completedTask;
        }

        public Task SaveAsync(IEnumerable<UserKeyPair> keyPairs)
        {
            return _completedTask;
        }

        public Task SignupAsync(EmailAddress email, CultureInfo culture)
        {
            return _completedTask;
        }

        public Task PasswordResetAsync(string verificationCode)
        {
            return _completedTask;
        }

        public Task<UserPublicKey> OtherPublicKeyAsync(EmailAddress email)
        {
            return Task.FromResult((UserPublicKey)null);
        }

        public Task<UserKeyPair> CurrentKeyPairAsync()
        {
            return Task.FromResult((UserKeyPair)null);
        }

        public Task SendFeedbackAsync(string subject, string message)
        {
            return Task.FromResult(default(object));
        }
    }
}