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
using Axantum.AxCrypt.Core.Crypto.Asymmetric;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Core.Service
{
    public class LocalAccountService : IAccountService
    {
        private static Regex _userKeyPairFilePattern = new Regex(@"^Keys-([\d]+)-txt\.axx$");

        private IAccountService _service;

        private IDataContainer _workContainer;

        public LocalAccountService(IAccountService service, IDataContainer workContainer)
        {
            _service = service;
            _workContainer = workContainer;
        }

        public SubscriptionLevel Level
        {
            get
            {
                return LoadUserAccount().SubscriptionLevel;
            }
        }

        public async Task<AccountStatus> StatusAsync()
        {
            if (LoadUserAccounts().Accounts.Any(a => EmailAddress.Parse(a.UserName) == _service.Identity.UserEmail) || UserKeyPairFiles().Any())
            {
                return AccountStatus.Verified;
            }
            return await _service.StatusAsync();
        }

        public bool HasAccounts
        {
            get
            {
                if (LoadUserAccounts().Accounts.Any())
                {
                    return true;
                }

                if (UserKeyPairFiles().Any())
                {
                    return true;
                }

                return _service.HasAccounts;
            }
        }

        private bool _hasCachedList = false;

        public async Task<IList<UserKeyPair>> ListAsync()
        {
            List<UserKeyPair> list = new List<UserKeyPair>();
            list.AddRange(TryLoadUserKeyPairs());

            if (_hasCachedList)
            {
                return list;
            }

            IList<UserKeyPair> other = await _service.ListAsync();
            list = list.Union(other).ToList();
            _hasCachedList = true;

            return list;
        }

        public async Task SaveAsync(IEnumerable<UserKeyPair> keyPairs)
        {
            UserAccounts userAccounts = LoadUserAccounts();
            UserAccount userAccount = userAccounts.Accounts.FirstOrDefault(ua => EmailAddress.Parse(ua.UserName) == _service.Identity.UserEmail);
            if (userAccount == null)
            {
                userAccount = new UserAccount(_service.Identity.UserEmail.Address, SubscriptionLevel.Unknown, AccountStatus.Unknown, new AccountKey[0]);
                userAccounts.Accounts.Add(userAccount);
            }

            IEnumerable<AccountKey> accountKeysToUpdate = keyPairs.Select(uk => uk.ToAccountKey(_service.Identity.Passphrase));
            IEnumerable<AccountKey> accountKeys = userAccount.AccountKeys.Except(accountKeysToUpdate);
            accountKeys = accountKeys.Union(accountKeysToUpdate);

            userAccount.AccountKeys.Clear();
            foreach (AccountKey accountKey in accountKeys)
            {
                userAccount.AccountKeys.Add(accountKey);
            }

            using (StreamWriter writer = new StreamWriter(_workContainer.FileItemInfo("UserAccounts.txt").OpenWrite()))
            {
                userAccounts.SerializeTo(writer);
            }

            await _service.SaveAsync(keyPairs);
        }

        public bool ChangePassphrase(Passphrase passphrase)
        {
            if (!_service.ChangePassphrase(passphrase))
            {
                return false;
            }

            SaveAsync(ListAsync().Result).Wait();
            return true;
        }

        /// <summary>
        /// Gets the full account of the user this instance works with.
        /// </summary>
        /// <value>
        /// The account.
        /// </value>
        /// <exception cref="System.NotImplementedException"></exception>
        public UserAccount Account
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets public information about the account.
        /// </summary>
        /// <param name="email">The email identifier of the account.</param>
        /// <returns>
        /// The user accoiunt with only the public inforamation.
        /// </returns>
        public UserAccount PublicAccount(EmailAddress email)
        {
            return null;
        }

        private IList<UserKeyPair> TryLoadUserKeyPairs()
        {
            IEnumerable<AccountKey> userAccountKeys = LoadUserAccount().AccountKeys;
            IEnumerable<UserKeyPair> userKeys = LoadValidUserKeysFromAccountKeys(userAccountKeys);
            if (!userKeys.Any())
            {
                userKeys = UserKeyPair.Load(UserKeyPairFiles(), _service.Identity.UserEmail, _service.Identity.Passphrase);
                userKeys = userKeys.Where(uk => !userAccountKeys.Any(ak => new PublicKeyThumbprint(ak.Thumbprint) == uk.KeyPair.PublicKey.Thumbprint));
            }

            return userKeys.ToList();
        }

        private IEnumerable<IDataStore> UserKeyPairFiles()
        {
            return _workContainer.Files.Where(f => _userKeyPairFilePattern.Match(f.Name).Success);
        }

        private IEnumerable<UserKeyPair> LoadValidUserKeysFromAccountKeys(IEnumerable<AccountKey> userAccountKeys)
        {
            return userAccountKeys.Select(ak => ak.ToUserAsymmetricKeys(_service.Identity.Passphrase)).Where(ak => ak != null);
        }

        private UserAccount LoadUserAccount()
        {
            UserAccounts accounts = LoadUserAccounts();
            IEnumerable<UserAccount> users = accounts.Accounts.Where(ua => EmailAddress.Parse(ua.UserName) == _service.Identity.UserEmail);
            if (!users.Any())
            {
                return new UserAccount(_service.Identity.UserEmail.Address, SubscriptionLevel.Unknown, AccountStatus.Unknown, new AccountKey[0]);
            }

            return users.First();
        }

        private IDataStore UserAccountsStore
        {
            get
            {
                return _workContainer.FileItemInfo("UserAccounts.txt");
            }
        }

        public LogOnIdentity Identity
        {
            get
            {
                return _service.Identity;
            }
        }

        private UserAccounts LoadUserAccounts()
        {
            if (!UserAccountsStore.IsAvailable)
            {
                return new UserAccounts();
            }

            using (StreamReader reader = new StreamReader(UserAccountsStore.OpenRead()))
            {
                return UserAccounts.DeserializeFrom(reader);
            }
        }
    }
}