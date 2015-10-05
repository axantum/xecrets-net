using Axantum.AxCrypt.Abstractions;
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

        private IList<UserKeyPair> _userKeyPairs = new List<UserKeyPair>();

        public UserAsymmetricKeysStore(IDataContainer workContainer)
        {
            _workContainer = workContainer;
        }

        public bool Load(EmailAddress userEmail, Passphrase passphrase)
        {
            _userKeyPairs = TryLoadUserKeyPairs(userEmail, passphrase);
            return _userKeyPairs.Any();
        }

        private IList<UserKeyPair> TryLoadUserKeyPairs(EmailAddress userEmail, Passphrase passphrase)
        {
            IEnumerable<AccountKey> userAccountKeys = LoadAllAccountKeysForUser(userEmail);
            IEnumerable<UserKeyPair> userKeys = LoadValidUserKeysFromAccountKeys(userAccountKeys, passphrase);
            if (!userKeys.Any())
            {
                userKeys = LoadValidUserKeysLegacyKeyStoreFiles(userEmail, passphrase);
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
            _userKeyPairs.Clear();
        }

        public bool IsValidAccountLogOn(EmailAddress userEmail, Passphrase passphrase)
        {
            return TryLoadUserKeyPairs(userEmail, passphrase).Any();
        }

        private void CreateInternal(EmailAddress userEmail, Passphrase passphrase)
        {
            UserKeyPair userKeys = new UserKeyPair(userEmail, Resolve.UserSettings.AsymmetricKeyBits);
            AddKeyPair(userKeys, passphrase);
        }

        private void AddKeyPair(UserKeyPair keyPair, Passphrase passphrase)
        {
            if (_userKeyPairs.Any(k => k == keyPair))
            {
                return;
            }

            _userKeyPairs.Add(keyPair);

            Save(keyPair.UserEmail, passphrase);
        }

        private IEnumerable<UserKeyPair> LoadValidUserKeysLegacyKeyStoreFiles(EmailAddress userEmail, Passphrase passphrase)
        {
            List<UserKeyPair> userKeyPairs = new List<UserKeyPair>();
            foreach (IDataStore file in UserKeyPairFiles())
            {
                UserKeyPair keys = TryLoadUserKeyPair(file, passphrase);
                if (keys == null)
                {
                    continue;
                }
                if (userEmail != keys.UserEmail)
                {
                    continue;
                }
                userKeyPairs.Add(keys);
            }
            return userKeyPairs;
        }

        private IEnumerable<IDataStore> UserKeyPairFiles()
        {
            return _workContainer.Files.Where(f => _userKeyPairFilePattern.Match(f.Name).Success);
        }

        public void Create(EmailAddress userEmail, Passphrase passphrase)
        {
            if (Load(userEmail, passphrase))
            {
                return;
            }

            CreateInternal(userEmail, passphrase);
        }

        public virtual IEnumerable<UserKeyPair> UserKeyPairs
        {
            get
            {
                return _userKeyPairs;
            }
        }

        public UserKeyPair UserKeyPair
        {
            get
            {
                return _userKeyPairs.First();
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
                return UserAccountsStore.IsAvailable || UserKeyPairFiles().Any();
            }
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

            IEnumerable<AccountKey> accountKeysToUpdate = _userKeyPairs.Select(uk => uk.ToAccountKey(passphrase));
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

        private static void SaveKeysStoreFile(IDataStore saveFile, UserKeyPair userKeys, Passphrase passphrase)
        {
            byte[] save = userKeys.ToArray(passphrase);
            using (Stream exportStream = saveFile.OpenWrite())
            {
                exportStream.Write(save, 0, save.Length);
            }
        }

        public EmailAddress ImportKeysStore(Stream keysStore, Passphrase passphrase)
        {
            UserKeyPair keyPair = TryLoadKeys(keysStore, passphrase);
            if (keyPair == null)
            {
                return EmailAddress.Empty;
            }

            AddKeyPair(keyPair, passphrase);
            return keyPair.UserEmail;
        }

        private static UserKeyPair TryLoadKeys(Stream encryptedStream, Passphrase passphrase)
        {
            using (MemoryStream decryptedStream = new MemoryStream())
            {
                if (!TypeMap.Resolve.New<AxCryptFile>().Decrypt(encryptedStream, decryptedStream, new LogOnIdentity(passphrase)).IsValid)
                {
                    return null;
                }

                string json = Encoding.UTF8.GetString(decryptedStream.ToArray(), 0, (int)decryptedStream.Length);
                return Resolve.Serializer.Deserialize<UserKeyPair>(json);
            }
        }

        private static UserKeyPair TryLoadUserKeyPair(IDataStore file, Passphrase passphrase)
        {
            UserKeyPair userKeyPair;
            if (UserKeyPair.TryLoad(file.ToArray(), passphrase, out userKeyPair))
            {
                return userKeyPair;
            }
            return null;
        }
    }
}