using Axantum.AxCrypt.Abstractions.Rest;
using Axantum.AxCrypt.Api.Model;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Crypto.Asymmetric;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

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

        public AccountStatus Status
        {
            get
            {
                return AccountStatus.Offline;
            }
        }

        public IEnumerable<UserKeyPair> List()
        {
            return TryLoadUserKeyPairs();
        }

        public void Save(IEnumerable<UserKeyPair> keyPairs)
        {
            UserAccounts userAccounts = LoadUserAccounts();
            UserAccount userAccount = userAccounts.Accounts.FirstOrDefault(ua => EmailAddress.Parse(ua.UserName) == EmailAddress.Parse(_service.Identity.User));
            if (userAccount == null)
            {
                userAccount = new UserAccount(_service.Identity.User, SubscriptionLevel.Unknown, new AccountKey[0]);
                userAccounts.Accounts.Add(userAccount);
            }

            IEnumerable<AccountKey> accountKeysToUpdate = List().Select(uk => uk.ToAccountKey(new Passphrase(_service.Identity.Password)));
            IEnumerable<AccountKey> accountKeys = userAccount.AccountKeys.Except(accountKeysToUpdate);
            accountKeys = accountKeys.Union(accountKeysToUpdate);

            userAccount.AccountKeys.Clear();
            foreach (AccountKey accountKey in accountKeys)
            {
                userAccount.AccountKeys.Add(accountKey);
            }

            using (StreamWriter writer = new StreamWriter(Resolve.WorkFolder.FileInfo.FileItemInfo("UserAccounts.txt").OpenWrite()))
            {
                userAccounts.SerializeTo(writer);
            }
        }

        public bool ChangePassword(string password)
        {
            if (!_service.ChangePassword(password))
            {
                return false;
            }

            Save(List());
            return true;
        }

        private IList<UserKeyPair> TryLoadUserKeyPairs()
        {
            IEnumerable<AccountKey> userAccountKeys = LoadUserAccount().AccountKeys;
            IEnumerable<UserKeyPair> userKeys = LoadValidUserKeysFromAccountKeys(userAccountKeys);
            if (!userKeys.Any())
            {
                userKeys = UserKeyPair.Load(UserKeyPairFiles(_workContainer), EmailAddress.Parse(_service.Identity.User), new Passphrase(_service.Identity.Password));
                userKeys = userKeys.Where(uk => !userAccountKeys.Any(ak => new PublicKeyThumbprint(ak.Thumbprint) == uk.KeyPair.PublicKey.Thumbprint));
            }

            return userKeys.OrderByDescending(uk => uk.Timestamp).ToList();
        }

        private static IEnumerable<IDataStore> UserKeyPairFiles(IDataContainer workContainer)
        {
            return workContainer.Files.Where(f => _userKeyPairFilePattern.Match(f.Name).Success);
        }

        private IEnumerable<UserKeyPair> LoadValidUserKeysFromAccountKeys(IEnumerable<AccountKey> userAccountKeys)
        {
            return userAccountKeys.Select(ak => ak.ToUserAsymmetricKeys(new Passphrase(_service.Identity.Password))).Where(ak => ak != null);
        }

        private UserAccount LoadUserAccount()
        {
            UserAccounts accounts = LoadUserAccounts();
            IEnumerable<UserAccount> users = accounts.Accounts.Where(ua => EmailAddress.Parse(ua.UserName) == EmailAddress.Parse(_service.Identity.User));
            if (!users.Any())
            {
                return new UserAccount(_service.Identity.User, SubscriptionLevel.Unknown, new AccountKey[0]);
            }

            return users.First();
        }

        private static IDataStore UserAccountsStore
        {
            get
            {
                return Resolve.WorkFolder.FileInfo.FileItemInfo("UserAccounts.txt");
            }
        }

        public RestIdentity Identity
        {
            get
            {
                return _service.Identity;
            }
        }

        private static UserAccounts LoadUserAccounts()
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