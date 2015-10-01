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
        private class KeysStoreFile
        {
            public KeysStoreFile(UserAsymmetricKeys userKeys, IDataStore file)
            {
                UserKeys = userKeys;
                File = file;
            }

            public UserAsymmetricKeys UserKeys { get; private set; }

            public IDataStore File { get; private set; }
        }

        private static Regex _filePattern = new Regex(@"^Keys-([\d]+)-txt\.axx$");

        private const string _fileFormat = "Keys-{0}.txt";

        private IDataContainer _folderPath;

        private IList<UserAsymmetricKeys> _userKeysList = new List<UserAsymmetricKeys>();

        public UserAsymmetricKeysStore(IDataContainer folderPath)
        {
            _folderPath = folderPath;
        }

        protected UserAsymmetricKeysStore()
        {
        }

        public bool Load(EmailAddress userEmail, Passphrase passphrase)
        {
            _userKeysList = TryLoadUserKeys(userEmail, passphrase);
            return _userKeysList.Any();
        }

        private IList<UserAsymmetricKeys> TryLoadUserKeys(EmailAddress userEmail, Passphrase passphrase)
        {
            IEnumerable<AccountKey> userAccountKeys = LoadAllAccountKeysForUser(userEmail);
            IEnumerable<UserAsymmetricKeys> userKeys = LoadValidUserKeysFromAccountKeys(userAccountKeys, passphrase);
            if (!userKeys.Any())
            {
                userKeys = LoadValidUserKeysLegacyKeyStoreFiles(userEmail, passphrase);
                userKeys = userKeys.Where(uk => !userAccountKeys.Any(ak => new PublicKeyThumbprint(ak.Thumbprint) == uk.KeyPair.PublicKey.Thumbprint));
            }

            return userKeys.OrderByDescending(uk => uk.Timestamp).ToList();
        }

        private static IEnumerable<UserAsymmetricKeys> LoadValidUserKeysFromAccountKeys(IEnumerable<AccountKey> userAccountKeys, Passphrase passphrase)
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
            _userKeysList.Clear();
        }

        public bool IsValidAccountLogOn(EmailAddress userEmail, Passphrase passphrase)
        {
            return TryLoadUserKeys(userEmail, passphrase).Any();
        }

        private void CreateInternal(EmailAddress userEmail, Passphrase passphrase)
        {
            UserAsymmetricKeys userKeys = new UserAsymmetricKeys(userEmail, Resolve.UserSettings.AsymmetricKeyBits);
            AddKeys(userKeys, passphrase);
        }

        private void AddKeys(UserAsymmetricKeys userKeys, Passphrase passphrase)
        {
            if (_userKeysList.Any(k => k == userKeys))
            {
                return;
            }

            _userKeysList.Add(userKeys);

            Save(userKeys.UserEmail, passphrase);
        }

        private IDataStore FileForUserKeys(UserAsymmetricKeys userKeys)
        {
            IDataStore file = TypeMap.Resolve.New<IDataStore>(Resolve.Portable.Path().Combine(_folderPath.FullName, _fileFormat.InvariantFormat(userKeys.KeyPair.PublicKey.Tag)).CreateEncryptedName());
            return file;
        }

        private IEnumerable<UserAsymmetricKeys> LoadValidUserKeysLegacyKeyStoreFiles(EmailAddress userEmail, Passphrase passphrase)
        {
            List<KeysStoreFile> keyStoreFiles = new List<KeysStoreFile>();
            foreach (IDataStore file in AsymmetricKeyFiles())
            {
                UserAsymmetricKeys keys = TryLoadKeys(file, passphrase);
                if (keys == null)
                {
                    continue;
                }
                if (userEmail != keys.UserEmail)
                {
                    continue;
                }
                keyStoreFiles.Add(new KeysStoreFile(keys, file));
            }
            return keyStoreFiles.Select(ksf => ksf.UserKeys);
        }

        private IEnumerable<IDataStore> AsymmetricKeyFiles()
        {
            return _folderPath.Files.Where(f => IdFromFileName(f.Name).Length > 0);
        }

        private static string IdFromFileName(string fileName)
        {
            Match match = _filePattern.Match(fileName);
            if (!match.Success)
            {
                return String.Empty;
            }
            return match.Groups[1].Value;
        }

        public void Create(EmailAddress userEmail, Passphrase passphrase)
        {
            if (Load(userEmail, passphrase))
            {
                return;
            }

            CreateInternal(userEmail, passphrase);
        }

        public virtual IEnumerable<UserAsymmetricKeys> Keys
        {
            get
            {
                return _userKeysList;
            }
        }

        public UserAsymmetricKeys CurrentKeys
        {
            get
            {
                return CurrentKeysStore;
            }
        }

        private UserAsymmetricKeys CurrentKeysStore
        {
            get
            {
                return _userKeysList.First();
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
                return UserAccountsStore.IsAvailable || AsymmetricKeyFiles().Any();
            }
        }

        public EmailAddress UserEmail
        {
            get
            {
                return Keys.First().UserEmail;
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

            IEnumerable<AccountKey> accountKeysToUpdate = _userKeysList.Select(uk => uk.ToAccountKey(passphrase));
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

        private static void SaveKeysStoreFile(IDataStore saveFile, UserAsymmetricKeys userKeys, Passphrase passphrase)
        {
            string originalFileName = _fileFormat.InvariantFormat(IdFromFileName(saveFile.Name));
            byte[] save = GetSaveDataForKeys(userKeys, originalFileName, passphrase);
            using (Stream exportStream = saveFile.OpenWrite())
            {
                exportStream.Write(save, 0, save.Length);
            }
        }

        private static byte[] GetSaveDataForKeys(UserAsymmetricKeys keys, string originalFileName, Passphrase passphrase)
        {
            string json = Resolve.Serializer.Serialize(keys);
            using (Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                EncryptionParameters encryptionParameters = new EncryptionParameters(Resolve.CryptoFactory.Preferred.Id, passphrase);
                EncryptedProperties properties = new EncryptedProperties(originalFileName);
                using (MemoryStream exportStream = new MemoryStream())
                {
                    AxCryptFile.Encrypt(stream, exportStream, properties, encryptionParameters, AxCryptOptions.EncryptWithCompression, new ProgressContext());
                    return exportStream.ToArray();
                }
            }
        }

        public byte[] ExportCurrentKeys(Passphrase passphrase)
        {
            return GetSaveDataForKeys(CurrentKeys, _fileFormat.InvariantFormat(CurrentKeys.KeyPair.PublicKey.Tag), passphrase);
        }

        public EmailAddress ImportKeysStore(Stream keysStore, Passphrase passphrase)
        {
            UserAsymmetricKeys keys = TryLoadKeys(keysStore, passphrase);
            if (keys == null)
            {
                return EmailAddress.Empty;
            }

            SaveKeysStoreFile(FileForUserKeys(keys), keys, passphrase);
            return keys.UserEmail;
        }

        private static UserAsymmetricKeys TryLoadKeys(Stream encryptedStream, Passphrase passphrase)
        {
            using (MemoryStream decryptedStream = new MemoryStream())
            {
                if (!TypeMap.Resolve.New<AxCryptFile>().Decrypt(encryptedStream, decryptedStream, new LogOnIdentity(passphrase)).IsValid)
                {
                    return null;
                }

                string json = Encoding.UTF8.GetString(decryptedStream.ToArray(), 0, (int)decryptedStream.Length);
                return Resolve.Serializer.Deserialize<UserAsymmetricKeys>(json);
            }
        }

        private static UserAsymmetricKeys TryLoadKeys(IDataStore file, Passphrase passphrase)
        {
            using (Stream encryptedStream = file.OpenRead())
            {
                using (MemoryStream decryptedStream = new MemoryStream())
                {
                    EncryptedProperties properties = TypeMap.Resolve.New<AxCryptFile>().Decrypt(encryptedStream, decryptedStream, new DecryptionParameter[] { new DecryptionParameter(passphrase, Resolve.CryptoFactory.Preferred.Id) });
                    if (!properties.IsValid)
                    {
                        return null;
                    }

                    string json = Encoding.UTF8.GetString(decryptedStream.ToArray(), 0, (int)decryptedStream.Length);
                    return Resolve.Serializer.Deserialize<UserAsymmetricKeys>(json);
                }
            }
        }
    }
}