using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Core.Runtime;

namespace Axantum.AxCrypt.Mono
{
    public class ProtectedDataImpl : IProtectedData
    {
        #region IProtectedData Members

        public byte[] Protect(byte[] userData, byte[] optionalEntropy)
        {
            try
            {
                return ProtectedData.Protect(userData, optionalEntropy, System.Security.Cryptography.DataProtectionScope.LocalMachine);
            }
            catch (CryptographicException ce)
            {
                throw new CryptoException("Could not protect the data", ce);
            }
        }

        public byte[] Unprotect(byte[] encryptedData, byte[] optionalEntropy)
        {
            try
            {
                return ProtectedData.Unprotect(encryptedData, optionalEntropy, System.Security.Cryptography.DataProtectionScope.LocalMachine);
            }
            catch (CryptographicException ce)
            {
                throw new CryptoException("Could not unprotect the data", ce);
            }
        }

        #endregion IProtectedData Members
    }
}