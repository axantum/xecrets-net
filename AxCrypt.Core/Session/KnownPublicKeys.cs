using AxCrypt.Abstractions;
using AxCrypt.Core.Crypto.Asymmetric;
using AxCrypt.Core.Extensions;
using AxCrypt.Core.IO;

using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Core.Session
{
    public class KnownPublicKeys : IDisposable
    {
        [AllowNull]
        private IDataStore _store;

        [AllowNull]
        private IStringSerializer _serializer;

        private bool _dirty;

        private List<UserPublicKey> _publicKeys;

        public KnownPublicKeys()
        {
            _publicKeys = new List<UserPublicKey>();
        }

        public void Delete()
        {
            using FileLock fileLock = New<FileLocker>().Acquire(_store);
            _store.Delete();
        }

        [JsonPropertyName("publickeys")]
        public IEnumerable<UserPublicKey> PublicKeys
        {
            get
            {
                return _publicKeys;
            }

            set
            {
                _publicKeys = new List<UserPublicKey>(value);
            }
        }

        public static KnownPublicKeys Load(IDataStore store, IStringSerializer serializer)
        {
            if (store == null)
            {
                throw new ArgumentNullException(nameof(store));
            }
            if (serializer == null)
            {
                throw new ArgumentNullException(nameof(serializer));
            }

            string json = String.Empty;
            using (FileLock fileLock = New<FileLocker>().Acquire(store))
            {
                if (store.IsAvailable)
                {
                    using var reader = new StreamReader(store.OpenRead(), Encoding.UTF8);
                    json = reader.ReadToEnd();
                }
            }
            var knownPublicKeys = serializer.Deserialize<KnownPublicKeys>(json);
            knownPublicKeys ??= new KnownPublicKeys();
            knownPublicKeys._store = store;
            knownPublicKeys._serializer = serializer;
            return knownPublicKeys;
        }

        /// <summary>
        /// Import a public key manually by the user. The key is also marked as imported by the user.
        /// </summary>
        /// <param name="publicKeyStore"></param>
        /// <returns>true if the import was successful</returns>
        public bool UserImport(IDataStore publicKeyStore)
        {
            UserPublicKey? publicKey = null;
            try
            {
                publicKey = _serializer.Deserialize<UserPublicKey>(publicKeyStore);
            }
            catch (JsonException jex)
            {
                New<IReport>().Exception(jex);
            }
            if (publicKey == null)
            {
                return false;
            }

            publicKey.IsUserImported = true;
            AddOrReplace(publicKey);

            return true;
        }

        public void AddOrReplace(UserPublicKey? publicKey)
        {
            if (publicKey == null)
            {
                throw new ArgumentNullException(nameof(publicKey));
            }

            for (int i = 0; i < _publicKeys.Count; ++i)
            {
                if (_publicKeys[i] == publicKey)
                {
                    return;
                }
                if (_publicKeys[i].Email == publicKey.Email)
                {
                    _dirty = true;
                    _publicKeys[i] = publicKey;
                    return;
                }
            }
            _dirty = true;
            _publicKeys.Add(publicKey);
        }

        public void Remove(IEnumerable<UserPublicKey> knownContactsToRemove)
        {
            if (knownContactsToRemove == null)
            {
                throw new ArgumentNullException(nameof(knownContactsToRemove));
            }

            _publicKeys = _publicKeys.Where(pk => !knownContactsToRemove.Contains(pk)).ToList();
            _dirty = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            if (_store == null)
            {
                return;
            }
            if (_dirty)
            {
                string json = _serializer.Serialize(this);
                using FileLock fileLock = New<FileLocker>().Acquire(_store);
                using var writer = new StreamWriter(_store.OpenWrite(), Encoding.UTF8);
                writer.Write(json);
            }
            _dirty = false;
            _store = null;
        }
    }
}
