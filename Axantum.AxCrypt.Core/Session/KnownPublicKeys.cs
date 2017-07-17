using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Core.Crypto.Asymmetric;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.IO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.Session
{
    [JsonObject(MemberSerialization.OptIn)]
    public class KnownPublicKeys
    {
        private IDataStore _store;

        private IStringSerializer _serializer;

        private List<UserPublicKeyWithStatus> _publicKeysWithStatus;

        protected KnownPublicKeys()
        {
            _publicKeysWithStatus = new List<UserPublicKeyWithStatus>();
        }

        public void Delete()
        {
            using (FileLock fileLock = New<FileLocker>().Acquire(_store))
            {
                _store.Delete();
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Used by Json.NET serializer.")]
        [JsonProperty("publickeys")]
        public IEnumerable<UserPublicKeyWithStatus> PublicKeysWithStatus
        {
            get
            {
                return _publicKeysWithStatus;
            }

            private set
            {
                _publicKeysWithStatus = new List<UserPublicKeyWithStatus>(value);
            }
        }

        public static KnownPublicKeys Load(IDataStore store, IStringSerializer serializer)
        {
            if (store == null)
            {
                throw new ArgumentNullException("store");
            }
            if (serializer == null)
            {
                throw new ArgumentNullException("serializer");
            }

            string json = String.Empty;
            using (FileLock fileLock = New<FileLocker>().Acquire(store))
            {
                if (store.IsAvailable)
                {
                    using (StreamReader reader = new StreamReader(store.OpenRead(), Encoding.UTF8))
                    {
                        json = reader.ReadToEnd();
                    }
                }
            }
            KnownPublicKeys knownPublicKeys = serializer.Deserialize<KnownPublicKeys>(json);
            if (knownPublicKeys == null)
            {
                knownPublicKeys = new KnownPublicKeys();
            }
            knownPublicKeys._store = store;
            knownPublicKeys._serializer = serializer;
            return knownPublicKeys;
        }

        public bool AddOrReplace(IDataStore publicKeyStore)
        {
            UserPublicKey publicKey = null;
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
            AddOrReplace(publicKey);
            return true;
        }

        public void AddOrReplace(UserPublicKey publicKey)
        {
            if (publicKey == null)
            {
                throw new ArgumentNullException("publicKey");
            }

            for (int i = 0; i < _publicKeysWithStatus.Count; ++i)
            {
                if (_publicKeysWithStatus[i].PublicKey == publicKey)
                {
                    return;
                }
                if (_publicKeysWithStatus[i].PublicKey.Email == publicKey.Email)
                {
                    _publicKeysWithStatus[i].PublicKey = publicKey;
                    _publicKeysWithStatus[i].UpdateStatus = UserPublicKeyUpdateStatus.RecentlyUpdated;
                    return;
                }
            }
            _publicKeysWithStatus.Add(new UserPublicKeyWithStatus() {PublicKey = publicKey,UpdateStatus = UserPublicKeyUpdateStatus.RecentlyUpdated});
        }

        public void ClearRecentlyUpdated()
        {
            for (int i = 0; i < _publicKeysWithStatus.Count; ++i)
            {
                _publicKeysWithStatus[i].UpdateStatus = UserPublicKeyUpdateStatus.NotRecentlyUpdated;
            }
            UpdateDataStore();
        }

        public void UpdateDataStore()
        {
            string json = _serializer.Serialize(this);
            using (FileLock fileLock = New<FileLocker>().Acquire(_store))
            {
                using (StreamWriter writer = new StreamWriter(_store.OpenWrite(), Encoding.UTF8))
                {
                    writer.Write(json);
                }
            }
        }
    }
}