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

namespace Axantum.AxCrypt.Core.Session
{
    /// <summary>
    /// Persists a users asymmetric keys in the file system, encrypted with AxCrypt
    /// </summary>
    public class UserAsymmetricKeysStore
    {
        private static Regex _userKeyPairFilePattern = new Regex(@"^Keys-([\d]+)-txt\.axx$");

        private IDataContainer _workContainer;

        private EmailAddress _userEmail;

        private Passphrase _passphrase;

        private Lazy<IList<UserKeyPair>> _userKeyPairs;

        public UserAsymmetricKeysStore(IDataContainer workContainer, EmailAddress userEmail, Passphrase passphrase)
        {
            _workContainer = workContainer;
            _userEmail = userEmail;
            _passphrase = passphrase;
            _userKeyPairs = new Lazy<IList<UserKeyPair>>(() => TryLoadUserKeyPairs(userEmail, passphrase));
        }

        private IList<UserKeyPair> TryLoadUserKeyPairs(EmailAddress userEmail, Passphrase passphrase)
        {
            IEnumerable<AccountKey> userAccountKeys = LoadAllAccountKeysForUser(userEmail);
            IEnumerable<UserKeyPair> userKeys = LoadValidUserKeysFromAccountKeys(userAccountKeys, passphrase);
            if (!userKeys.Any())
            {
                userKeys = UserKeyPair.Load(UserKeyPairFiles(_workContainer), userEmail, passphrase);
                userKeys = userKeys.Where(uk => !userAccountKeys.Any(ak => new PublicKeyThumbprint(ak.Thumbprint) == uk.KeyPair.PublicKey.Thumbprint));
            }

            return userKeys.OrderByDescending(uk => uk.Timestamp).ToList();
        }

        private static IEnumerable<UserKeyPair> LoadValidUserKeysFromAccountKeys(IEnumerable<AccountKey> userAccountKeys, Passphrase passphrase)
        {
            return userAccountKeys.Select(ak => ak.ToUserAsymmetricKeys(passphrase)).Where(ak => ak != null);
        }

        private static IEnumerable<AccountKey> LoadAllAccountKeysForUser(EmailAddress userEmail)
        {
            UserAccounts accounts = LoadUserAccounts();
            IEnumerable<UserAccount> users = accounts.Accounts.Where(ua => EmailAddress.Parse(ua.UserName) == userEmail);
            IEnumerable<AccountKey> accountKeys = users.SelectMany(u => u.AccountKeys);
            return accountKeys;
        }

        private static IDataStore UserAccountsStore
        {
            get
            {
                return Resolve.WorkFolder.FileInfo.FileItemInfo("UserAccounts.txt");
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

        public void Unload()
        {
            _userKeyPairs.Value.Clear();
        }

        public bool IsValidAccountLogOn()
        {
            return _userKeyPairs.Value.Any();
        }

        private void CreateInternal(EmailAddress userEmail, Passphrase passphrase)
        {
            UserKeyPair userKeys = new UserKeyPair(userEmail, Resolve.UserSettings.AsymmetricKeyBits);
            Import(userKeys, passphrase);
        }

        public void Import(UserKeyPair keyPair, Passphrase passphrase)
        {
            if (_userKeyPairs.Value.Any(k => k == keyPair))
            {
                return;
            }

            _userKeyPairs.Value.Add(keyPair);
            Save(keyPair.UserEmail, passphrase);
        }

        private static IEnumerable<IDataStore> UserKeyPairFiles(IDataContainer workContainer)
        {
            return workContainer.Files.Where(f => _userKeyPairFilePattern.Match(f.Name).Success);
        }

        public void Create()
        {
            if (_userKeyPairs.Value.Any())
            {
                return;
            }

            CreateInternal(_userEmail, _passphrase);
        }

        public virtual IEnumerable<UserKeyPair> UserKeyPairs
        {
            get
            {
                return _userKeyPairs.Value;
            }
        }

        public UserKeyPair UserKeyPair
        {
            get
            {
                return UserKeyPairs.First();
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has any shareable identities, i.e. key pairs.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has a store; otherwise, <c>false</c>.
        /// </value>
        public virtual bool HasStore
        {
            get
            {
                return UserAccountsStore.IsAvailable || UserKeyPairFiles(_workContainer).Any();
            }
        }

        public static bool HasKeyPairs(IDataContainer workFolder)
        {
            if (!workFolder.IsAvailable)
            {
                return false;
            }

            if (LoadUserAccounts().Accounts.Any())
            {
                return true;
            }

            return UserKeyPairFiles(workFolder).Any();
        }

        public EmailAddress UserEmail
        {
            get
            {
                return UserKeyPairs.First().UserEmail;
            }
        }

        public virtual void Save(EmailAddress userEmail, Passphrase passphrase)
        {
            UserAccounts userAccounts = LoadUserAccounts();
            UserAccount userAccount = userAccounts.Accounts.FirstOrDefault(ua => EmailAddress.Parse(ua.UserName) == userEmail);
            if (userAccount == null)
            {
                userAccount = new UserAccount(userEmail.Address, SubscriptionLevel.Unknown, new AccountKey[0]);
                userAccounts.Accounts.Add(userAccount);
            }

            IEnumerable<AccountKey> accountKeysToUpdate = _userKeyPairs.Value.Select(uk => uk.ToAccountKey(passphrase));
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
    }
}