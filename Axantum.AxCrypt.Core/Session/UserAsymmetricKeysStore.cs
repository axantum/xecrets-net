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
            _userKeyPairs = new Lazy<IList<UserKeyPair>>(() => TryLoadUserKeyPairs());
        }

        private IList<UserKeyPair> TryLoadUserKeyPairs()
        {
            IEnumerable<AccountKey> userAccountKeys = LoadAllAccountKeysForUser();
            IEnumerable<UserKeyPair> userKeys = LoadValidUserKeysFromAccountKeys(userAccountKeys);
            if (!userKeys.Any())
            {
                userKeys = UserKeyPair.Load(UserKeyPairFiles(_workContainer), _userEmail, _passphrase);
                userKeys = userKeys.Where(uk => !userAccountKeys.Any(ak => new PublicKeyThumbprint(ak.Thumbprint) == uk.KeyPair.PublicKey.Thumbprint));
            }

            return userKeys.OrderByDescending(uk => uk.Timestamp).ToList();
        }

        private IEnumerable<UserKeyPair> LoadValidUserKeysFromAccountKeys(IEnumerable<AccountKey> userAccountKeys)
        {
            return userAccountKeys.Select(ak => ak.ToUserAsymmetricKeys(_passphrase)).Where(ak => ak != null);
        }

        private IEnumerable<AccountKey> LoadAllAccountKeysForUser()
        {
            UserAccounts accounts = LoadUserAccounts();
            IEnumerable<UserAccount> users = accounts.Accounts.Where(ua => EmailAddress.Parse(ua.UserName) == _userEmail);
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

        public bool HasKeyPair
        {
            get
            {
                return _userKeyPairs.Value.Any();
            }
        }

        public void Import(UserKeyPair keyPair)
        {
            if (keyPair.UserEmail != _userEmail)
            {
                throw new ArgumentException("User email mismatch in key pair and store.", nameof(keyPair));
            }

            if (_userKeyPairs.Value.Any(k => k == keyPair))
            {
                return;
            }

            _userKeyPairs.Value.Add(keyPair);
            Save(_passphrase);
        }

        private static IEnumerable<IDataStore> UserKeyPairFiles(IDataContainer workContainer)
        {
            return workContainer.Files.Where(f => _userKeyPairFilePattern.Match(f.Name).Success);
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
                return UserKeyPair.UserEmail;
            }
        }

        public virtual void Save(Passphrase passphrase)
        {
            _passphrase = passphrase;

            UserAccounts userAccounts = LoadUserAccounts();
            UserAccount userAccount = userAccounts.Accounts.FirstOrDefault(ua => EmailAddress.Parse(ua.UserName) == _userEmail);
            if (userAccount == null)
            {
                userAccount = new UserAccount(_userEmail.Address, SubscriptionLevel.Unknown, new AccountKey[0]);
                userAccounts.Accounts.Add(userAccount);
            }

            IEnumerable<AccountKey> accountKeysToUpdate = _userKeyPairs.Value.Select(uk => uk.ToAccountKey(_passphrase));
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