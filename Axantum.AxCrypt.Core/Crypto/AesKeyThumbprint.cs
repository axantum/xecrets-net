using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Axantum.AxCrypt.Core.Crypto
{
    public class AesKeyThumbprint
    {
        private byte[] _salt;
        private AesKey _key;

        public AesKeyThumbprint(AesKey key, byte[] salt)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (salt == null)
            {
                throw new ArgumentNullException("salt");
            }
            _key = key;
            _salt = salt;
        }

        public byte[] GetThumbprintBytes()
        {
            HashAlgorithm hash = HashAlgorithm.Create("SHA256");
            hash.TransformBlock(_salt, 0, _salt.Length, null, 0);
            hash.TransformFinalBlock(_key.GetBytes(), 0, _key.Length);

            return hash.Hash;
        }
    }
}