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
    public class KnownPublicKeys : IDisposable
    {
        private IDataStore _store;

        private IStringSerializer _serializer;

        private bool _dirty;

        private List<UserPublicKey> _publicKeys;

        protected KnownPublicKeys()
        {
            _publicKeys = new List<UserPublicKey>();
        }

        public void Delete()
        {
            using (FileLockReleaser fileLock = FileLock.Lock(_store))
            {
                _store.Delete();
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Used by Json.NET serializer.")]
        [JsonProperty("publickeys")]
        public IEnumerable<UserPublicKey> PublicKeys
        {
            get
            {
                return _publicKeys;
            }

            private set
            {
                _publicKeys = new List<UserPublicKey>(value);
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
            using (FileLockReleaser fileLock = FileLock.Lock(store))
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
                using (FileLockReleaser fileLock = FileLock.Lock(_store))
                {
                    using (StreamWriter writer = new StreamWriter(_store.OpenWrite(), Encoding.UTF8))
                    {
                        writer.Write(json);
                    }
                }
            }
            _dirty = false;
            _store = null;
        }
    }
}