using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.Header;
using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Mono
{
    public class ProtectedDataImpl : IProtectedData
    {
        private static byte[] _axCryptGuid = AxCrypt1Guid.GetBytes();

        private DataProtectionScope _scope;

        public ProtectedDataImpl(DataProtectionScope scope)
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