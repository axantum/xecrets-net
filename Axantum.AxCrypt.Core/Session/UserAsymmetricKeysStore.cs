﻿using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Crypto.Asymmetric;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.UI;
using Newtonsoft.Json;
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
            public KeysStoreFile(UserAsymmetricKeys userKeys, string id, IRuntimeFileInfo file)
            {
                UserKeys = userKeys;
                Id = id;
                File = file;
            }

            public UserAsymmetricKeys UserKeys { get; private set; }

            public string Id { get; private set; }

            public IRuntimeFileInfo File { get; private set; }
        }

        private static Regex _filePattern = new Regex(@"^Keys-([\d]+)-txt\.axx$");

        private const string _fileFormat = "Keys-{0}.txt";

        private IRuntimeFileInfo _folderPath;

        private KnownKeys _knownKeys;

        private KeysStoreFile _keysStoreFile;

        public UserAsymmetricKeysStore(IRuntimeFileInfo folderPath, KnownKeys knownKeys)
        {
            _folderPath = folderPath;
            _knownKeys = knownKeys;
        }

        public bool Load(EmailAddress userEmail, Passphrase passphrase)
        {
            _keysStoreFile = null;

            _keysStoreFile = TryLoadKeyStoreFile(userEmail, passphrase);
            if (_keysStoreFile == null)
            {
                return false;
            }
            _knownKeys.DefaultEncryptionKey = passphrase;
            return true;
        }

        public bool IsValidAccountLogOn(EmailAddress userEmail, Passphrase passphrase)
        {
            return TryLoadKeyStoreFile(userEmail, passphrase) != null;
        }

        private void CreateInternal(EmailAddress userEmail, Passphrase passphrase)
        {
            UserAsymmetricKeys userKeys = new UserAsymmetricKeys(userEmail, Resolve.UserSettings.AsymmetricKeyBits);
            string id = UniqueFilePart();
            IRuntimeFileInfo file = TypeMap.Resolve.New<IRuntimeFileInfo>(Resolve.Portable.Path().Combine(_folderPath.FullName, _fileFormat.InvariantFormat(id)).CreateEncryptedName());

            _keysStoreFile = new KeysStoreFile(userKeys, id, file);

            Save(passphrase);
        }

        private KeysStoreFile TryLoadKeyStoreFile(EmailAddress userEmail, Passphrase passphrase)
        {
            foreach (IRuntimeFileInfo file in AsymmetricKeyFiles())
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
                return new KeysStoreFile(keys, IdFromFileName(file.Name), file);
            }
            return null;
        }

        private IEnumerable<IRuntimeFileInfo> AsymmetricKeyFiles()
        {
            return _folderPath.Files.Where(f => IdFromFileName(f.Name).Length > 0);
        }

        private string IdFromFileName(string fileName)
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
            _keysStoreFile = TryLoadKeyStoreFile(userEmail, passphrase);
            if (_keysStoreFile != null)
            {
                return;
            }
            CreateInternal(userEmail, passphrase);
        }

        public UserAsymmetricKeys Keys
        {
            get
            {
                return _keysStoreFile.UserKeys;
            }
        }

        public bool HasStore
        {
            get
            {
                return AsymmetricKeyFiles().Any();
            }
        }

        public bool HasKeys
        {
            get
            {
                return _keysStoreFile != null;
            }
        }

        private string UniqueFilePart()
        {
            DateTime now = OS.Current.UtcNow;
            TimeSpan timeSince = now - new DateTime(now.Year, 1, 1);
            return ((int)timeSince.TotalSeconds).ToString();
        }

        public void Save(Passphrase passphrase)
        {
            if (_keysStoreFile == null)
            {
                return;
            }

            string json = JsonConvert.SerializeObject(_keysStoreFile.UserKeys);
            using (Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                TypeMap.Resolve.New<AxCryptFile>().Encrypt(stream, _keysStoreFile.File.Name, _keysStoreFile.File, passphrase, Resolve.CryptoFactory.Default.Id, AxCryptOptions.EncryptWithCompression, new ProgressContext());
            }
        }

        private UserAsymmetricKeys TryLoadKeys(IRuntimeFileInfo file, Passphrase passphrase)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                if (!TypeMap.Resolve.New<AxCryptFile>().Decrypt(file, stream, passphrase))
                {
                    return null;
                }

                string json = Encoding.UTF8.GetString(stream.ToArray(), 0, (int)stream.Length);
                return JsonConvert.DeserializeObject<UserAsymmetricKeys>(json, TypeMap.Resolve.Singleton<IAsymmetricFactory>().GetConverters());
            }
        }
    }
}