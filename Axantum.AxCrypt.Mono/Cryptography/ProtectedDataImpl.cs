using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using Axantum.AxCrypt.Abstractions;

namespace Axantum.AxCrypt.Mono
{
    public class ProtectedDataImpl : IProtectedData
    {
        #region IProtectedData Members

        public byte[] Protect(byte[] userData, byte[] optionalEntropy)
        {
            return ProtectedData.Protect(userData, optionalEntropy, System.Security.Cryptography.DataProtectionScope.LocalMachine);
        }

        public byte[] Unprotect(byte[] encryptedData, byte[] optionalEntropy)
        {
            return ProtectedData.Unprotect(encryptedData, optionalEntropy, System.Security.Cryptography.DataProtectionScope.LocalMachine);
        }

        #endregion IProtectedData Members
    }
}