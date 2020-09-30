using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using AxCrypt.Abstractions;
using AxCrypt.Core.Extensions;
using AxCrypt.Core.Runtime;
using AxCrypt.Core.Header;
using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Mono
{
    public class ProtectedDataImplementation : IProtectedData
    {
        private static byte[] _axCryptGuid = AxCrypt1Guid.GetBytes();

        private DataProtectionScope _scope;

        public ProtectedDataImplementation(DataProtectionScope scope)
        {
            _scope = scope;
        }

        #region IProtectedData Members

        public byte[] Protect(byte[] userData, byte[] optionalEntropy)
        {
            return ProtectedData.Protect(userData, optionalEntropy, _scope);
        }

        public byte[] Unprotect(byte[] encryptedData, byte[] optionalEntropy)
        {
            if (encryptedData.Locate(_axCryptGuid, 0, _axCryptGuid.Length) == 0)
            {
                return null;
            }
            try
            {
                byte[] bytes = ProtectedData.Unprotect(encryptedData, optionalEntropy, _scope);
                return bytes;
            }
            catch (CryptographicException cex)
            {
                New<IReport>().Exception(cex);
                return null;
            }
        }

        #endregion IProtectedData Members
    }
}