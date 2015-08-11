using Axantum.AxCrypt.Core.Crypto;
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

        private List<KeysStoreFile> _keysStoreFiles;

        public UserAsymmetricKeysStore(IDataContainer folderPath)
        {
            _folderPath = folderPath;
            _keysStoreFiles = new List<KeysStoreFile>();
        }

        protected UserAsymmetricKeysStore()
        {
        }

        public bool Load(EmailAddress userEmail, Passphrase passphrase)
        {
            _keysStoreFiles = new List<KeysStoreFile>(TryLoadKeyStoreFiles(userEmail, passphrase));
            if (!_keysStoreFiles.Any())
            {
                return false;
            }
            return true;
        }

        public void Unload()
        {
            _keysStoreFiles.Clear();
        }

        public bool IsValidAccountLogOn(EmailAddress userEmail, Passphrase passphrase)
        {
            return TryLoadKeyStoreFiles(userEmail, passphrase).Any();
        }

        private void CreateInternal(EmailAddress userEmail, Passphrase passphrase)
        {
            UserAsymmetricKeys userKeys = new UserAsymmetricKeys(userEmail, Resolve.UserSettings.AsymmetricKeyBits);
            AddKeys(userKeys, passphrase);
        }

        private void AddKeys(UserAsymmetricKeys userKeys, Passphrase passphrase)
        {
            if (_keysStoreFiles.Any(k => k.UserKeys == userKeys))
            {
                return;
            }

            _keysStoreFiles.Add(new KeysStoreFile(userKeys, FileForUserKeys(userKeys)));

            Save(passphrase);
        }

        private IDataStore FileForUserKeys(UserAsymmetricKeys userKeys)
        {
            IDataStore file = TypeMap.Resolve.New<IDataStore>(Resolve.Portable.Path().Combine(_folderPath.FullName, _fileFormat.InvariantFormat(userKeys.KeyPair.PublicKey.Tag)).CreateEncryptedName());
            return file;
        }

        private IEnumerable<KeysStoreFile> TryLoadKeyStoreFiles(EmailAddress userEmail, Passphrase passphrase)
        {
            List<KeysStoreFile> keyStoreFiles = new List<KeysStoreFile>();
            foreach (IDataStore file in AsymmetricKeyFiles())
            {
                UserAsymmetricKeys keys = TryLoadKeys(file, passphrase);
                if (keys == null)
                {
                    continue;
                }
                if (String.Compare(userEmail.Address, keys.UserEmail.Address, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    continue;
                }
                keyStoreFiles.Add(new KeysStoreFile(keys, file));
            }
            return keyStoreFiles;
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
            _keysStoreFiles = new List<KeysStoreFile>(TryLoadKeyStoreFiles(userEmail, passphrase));
            if (_keysStoreFiles.Any())
            {
                return;
            }
            CreateInternal(userEmail, passphrase);
        }

        public virtual IEnumerable<UserAsymmetricKeys> Keys
        {
            get
            {
                return KeysStoreFiles.Select(ksf => ksf.UserKeys);
            }
        }

        public UserAsymmetricKeys CurrentKeys
        {
            get
            {
                return CurrentKeysStore.UserKeys;
            }
        }

        private KeysStoreFile CurrentKeysStore
        {
            get
            {
                return KeysStoreFiles.First();
            }
        }

        private IEnumerable<KeysStoreFile> KeysStoreFiles
        {
            get
            {
                return _keysStoreFiles.OrderBy((ksf) => ksf.UserKeys.Timestamp);
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
                return AsymmetricKeyFiles().Any();
            }
        }

        public EmailAddress UserEmail
        {
            get
            {
                return Keys.First().UserEmail;
            }
        }

        public virtual void Save(Passphrase passphrase)
        {
            foreach (KeysStoreFile keysStoreFile in _keysStoreFiles)
            {
                SaveKeysStoreFile(keysStoreFile.File, keysStoreFile.UserKeys, passphrase);
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
                EncryptionParameters encryptionParameters = new EncryptionParameters(Resolve.CryptoFactory.Default.Id, passphrase);
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
            using (MemoryStream stream = new MemoryStream())
            {
                if (!TypeMap.Resolve.New<AxCryptFile>().Decrypt(file, stream, new LogOnIdentity(passphrase)))
                {
                    return null;
                }

                string json = Encoding.UTF8.GetString(stream.ToArray(), 0, (int)stream.Length);
                return Resolve.Serializer.Deserialize<UserAsymmetricKeys>(json);
            }
        }
    }
}