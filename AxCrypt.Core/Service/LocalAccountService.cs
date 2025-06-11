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
 * The source is maintained at http://bitbucket.org/AxCrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using AxCrypt.Abstractions;
using AxCrypt.Api.Model;
using AxCrypt.Api.Model.Groups;
using AxCrypt.Common;
using AxCrypt.Core.Crypto;
using AxCrypt.Core.Crypto.Asymmetric;
using AxCrypt.Core.Extensions;
using AxCrypt.Core.IO;
using AxCrypt.Core.Session;
using AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Core.Service
{
    public class LocalAccountService : IAccountService
    {
        private static readonly Task _completedTask = Task.FromResult(true);

        private static readonly Regex _userKeyPairFilePattern = new Regex(@"^Keys-([\d]+)-txt\.axx$");

        private readonly IDataContainer _workContainer;

        public static readonly string FileName = "UserAccounts.txt";

        public LocalAccountService(LogOnIdentity identity, IDataContainer workContainer)
        {
            Identity = identity ?? throw new ArgumentNullException(nameof(identity));
            _workContainer = workContainer ?? throw new ArgumentNullException(nameof(workContainer));
        }

        public IAccountService Refresh()
        {
            return this;
        }

        public async Task<AccountStatus> StatusAsync(EmailAddress email)
        {
            return await Task.Run(() =>
            {
                if (LoadUserAccounts().Accounts.Any(a => EmailAddress.Parse(a.UserName) == email) || UserKeyPairFiles().Any())
                {
                    return AccountStatus.Verified;
                }
                return AccountStatus.NotFound;
            }).Free();
        }

        public Task<Offers> OffersAsync()
        {
            if (Identity == LogOnIdentity.Empty)
            {
                return Task.FromResult(Api.Model.Offers.None);
            }
            return Task.FromResult(LoadUserAccount().Offers);
        }

        public Task StartPremiumTrialAsync()
        {
            throw new InvalidOperationException("A premium trial cannot be started locally.");
        }

        public Task<bool> HasAccountsAsync()
        {
            if (Identity.UserEmail == EmailAddress.Empty)
            {
                throw new InvalidOperationException("The account service requires a user.");
            }
            if (LoadUserAccounts().Accounts.Any())
            {
                return Task.FromResult(true);
            }

            if (UserKeyPairFiles().Any())
            {
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

        /// <summary>
        /// Fetches the user user account.
        /// </summary>
        /// <returns>
        /// The complete user account information.
        /// </returns>
        public Task<UserAccount> AccountAsync()
        {
            if (Identity.UserEmail == EmailAddress.Empty)
            {
                throw new InvalidOperationException("The account service requires a user.");
            }
            UserAccount userAccount = LoadUserAccount();
            return Task.FromResult(userAccount);
        }

        public Task<IList<UserKeyPair>> ListAsync()
        {
            if (Identity.UserEmail == EmailAddress.Empty)
            {
                throw new InvalidOperationException("The account service requires a user.");
            }

            return Task.FromResult(TryLoadUserKeyPairs());
        }

        public async Task<UserKeyPair?> CurrentKeyPairAsync()
        {
            if (Identity.UserEmail == EmailAddress.Empty)
            {
                throw new InvalidOperationException("The account service requires a user.");
            }

            UserAccount userAccount = LoadUserAccount();
            UserKeyPair? keyPair = userAccount.AccountKeys.Select(ak => ak.ToUserKeyPair(Identity.Passphrase)).OrderByDescending(ukp => ukp!.Timestamp).FirstOrDefault();
            if (keyPair == null)
            {
                AccountStorage store = new AccountStorage(New<LogOnIdentity, IAccountService>(Identity));
                keyPair = new UserKeyPair(Identity.UserEmail, New<INow>().Utc, New<KeyPairService>().New());
                await store.ImportAsync(keyPair);
            }

            return keyPair;
        }

        public async Task SaveAsync(UserAccount account)
        {
            if (Identity.UserEmail == EmailAddress.Empty)
            {
                throw new InvalidOperationException("The account service requies a user.");
            }

            await Task.Run(() =>
            {
                UserAccounts userAccounts = LoadUserAccounts();
                UserAccount? existingUserAccount = userAccounts.Accounts.FirstOrDefault(ua => EmailAddress.Parse(ua.UserName) == Identity.UserEmail);
                if (existingUserAccount == null)
                {
                    existingUserAccount = new UserAccount(Identity.UserEmail.Address);
                    userAccounts.Accounts.Add(existingUserAccount);
                }

                UserAccount mergedAccount = account.MergeWith(existingUserAccount);
                if (mergedAccount == existingUserAccount)
                {
                    return;
                }

                SaveInternal(userAccounts, mergedAccount);
            }).Free();
        }

        public async Task SaveAsync(IEnumerable<UserKeyPair> keyPairs)
        {
            if (Identity.UserEmail == EmailAddress.Empty)
            {
                throw new InvalidOperationException("The account service requies a user.");
            }

            await Task.Run(() =>
            {
                UserAccounts userAccounts = LoadUserAccounts();
                UserAccount? existingUserAccount = userAccounts.Accounts.FirstOrDefault(ua => EmailAddress.Parse(ua.UserName) == Identity.UserEmail);
                if (existingUserAccount == null)
                {
                    existingUserAccount = new UserAccount(Identity.UserEmail.Address);
                    userAccounts.Accounts.Add(existingUserAccount);
                }

                UserAccount mergedAccount = keyPairs.Select(uk => uk.ToAccountKey(Identity.Passphrase)).MergeWith(existingUserAccount);
                if (mergedAccount == existingUserAccount)
                {
                    return;
                }

                SaveInternal(userAccounts, mergedAccount);
            }).Free();
        }

        private void SaveInternal(UserAccounts userAccounts, UserAccount userAccount)
        {
            UserAccounts userAccountsToSave = new UserAccounts();
            foreach (UserAccount ua in userAccounts.Accounts.Where(a => a.UserName != userAccount.UserName))
            {
                userAccountsToSave.Accounts.Add(ua);
            }
            userAccountsToSave.Accounts.Add(userAccount);

            using StreamWriter writer = new StreamWriter(_workContainer.FileItemInfo("UserAccounts.txt").OpenWrite());
            userAccountsToSave.SerializeTo(writer);
        }

        public Task<bool> ChangePassphraseAsync(Passphrase passphrase)
        {
            if (Identity.UserEmail == EmailAddress.Empty)
            {
                throw new InvalidOperationException("The account service requires a user.");
            }

            return Task.FromResult(true);
        }

        private IList<UserKeyPair> TryLoadUserKeyPairs()
        {
            IEnumerable<AccountKey> userAccountKeys = LoadUserAccount().AccountKeys;
            List<UserKeyPair> userKeys = LoadValidUserKeysFromAccountKeys(userAccountKeys).ToList();
            if (!userKeys.Any())
            {
                IEnumerable<UserKeyPair> fromKeyPairFiles = UserKeyPair.Load(UserKeyPairFiles(), Identity.UserEmail, Identity.Passphrase);
                fromKeyPairFiles = userKeys.Where(ukp => ukp is not null && !userAccountKeys.Any(ak => new PublicKeyThumbprint(ak.Thumbprint) == ukp.KeyPair.PublicKey.Thumbprint));
                userKeys.AddRange(fromKeyPairFiles);
            }

            return userKeys;
        }

        private IEnumerable<IDataStore> UserKeyPairFiles()
        {
            return _workContainer.Files.Where(f => _userKeyPairFilePattern.Match(f.Name).Success);
        }

        private IEnumerable<UserKeyPair> LoadValidUserKeysFromAccountKeys(IEnumerable<AccountKey> userAccountKeys)
        {
            return userAccountKeys.Select(ak => ak.ToUserKeyPair(Identity.Passphrase)).Where(ukp => ukp != null).Cast<UserKeyPair>();
        }

        private UserAccount LoadUserAccount()
        {
            UserAccounts accounts = LoadUserAccounts();
            IEnumerable<UserAccount> users = accounts.Accounts.Where(ua => EmailAddress.Parse(ua.UserName) == Identity.UserEmail);
            if (!users.Any())
            {
                UserAccount userAccount = new UserAccount(Identity.UserEmail.Address)
                {
                    AccountSource = AccountSource.Local
                };
                return userAccount;
            }
            users.First().AccountSource = AccountSource.Local;
            return users.First();
        }

        private IDataStore UserAccountsStore
        {
            get
            {
                return _workContainer.FileItemInfo(FileName);
            }
        }

        public LogOnIdentity Identity
        {
            get;
        }

        public async Task SignupAsync(EmailAddress email, CultureInfo culture)
        {
            await _completedTask;
        }

        private UserAccounts LoadUserAccounts()
        {
            if (!UserAccountsStore.IsAvailable)
            {
                return new UserAccounts();
            }

            using var reader = new StreamReader(UserAccountsStore.OpenRead());
            UserAccounts? accounts = UserAccounts.DeserializeFrom(reader);
            accounts ??= new UserAccounts();

            return accounts;
        }

        public Task PasswordResetAsync(string verificationCode)
        {
            if (Identity.UserEmail == EmailAddress.Empty)
            {
                throw new InvalidOperationException("The account service requires a user.");
            }

            return _completedTask;
        }

        public async Task<UserPublicKey?> OtherPublicKeyAsync(EmailAddress email)
        {
            if (Identity.UserEmail == EmailAddress.Empty)
            {
                throw new InvalidOperationException("The account service requires a user.");
            }

            return await Task.Run(() =>
            {
                using var knowPublicKeys = New<KnownPublicKeys>();
                UserPublicKey? publicKey = knowPublicKeys.PublicKeys.Where(pk => pk.Email == email).FirstOrDefault();
                return publicKey;
            }).Free();
        }

        public async Task<UserPublicKey?> OtherUserInvitePublicKeyAsync(EmailAddress email, CustomMessageParameters? customParameters)
        {
            return await OtherPublicKeyAsync(email);
        }

        public Task SendFeedbackAsync(string subject, string message)
        {
            throw new InvalidOperationException("Feedback sending can't be performed locally.");
        }

        public Task<bool> CreateSubscriptionAsync(StoreKitTransaction[] skTransactions)
        {
            throw new InvalidOperationException("Premium creation cannot be started locally.");
        }

        public Task<PurchaseSettings?> GetInAppPurchaseSettingsAsync()
        {
            throw new InvalidOperationException("In app purchase member cannot be getting locally.");
        }

        public Task<bool> AutoRenewalStatusAsync()
        {
            throw new InvalidOperationException("Cancel subscription cannot be get locally.");
        }

        public Task<bool> DeleteUserAsync()
        {
            throw new InvalidOperationException("Can't perform delete user account locally.");
        }

        public Task<IEnumerable<GroupKeyPairApiModel>> ListMembershipGroupsAsync()
        {
            IEnumerable<GroupKeyPairApiModel> result = new List<GroupKeyPairApiModel>();
            return Task.FromResult(result);
        }
    }
}
