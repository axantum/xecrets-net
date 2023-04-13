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
using Microsoft.AspNetCore.DataProtection;

namespace AxCrypt.Mono
{
    public class ProtectedDataImplementation : IProtectedData
    {
        private static readonly byte[] _axCryptGuid = AxCrypt1Guid.GetBytes();

        private readonly IDataProtector _protector;

        public ProtectedDataImplementation(string application)
        {
            IDataProtectionProvider provider = DataProtectionProvider.Create(application);

            _protector = provider.CreateProtector("Xecrets.File.ProtectedData.v2");
        }

        #region IProtectedData Members

        public byte[] Protect(byte[] userData, byte[]? optionalEntropy)
        {
            ArgumentNullException.ThrowIfNull(userData, nameof(userData));
            if (optionalEntropy != null && optionalEntropy.Length > 0)
            {
                throw new NotImplementedException("Use of optional entropy is not yet implemented.");
            }

            return _protector.Protect(userData);
        }

        public byte[]? Unprotect(byte[] encryptedData, byte[]? optionalEntropy)
        {
            ArgumentNullException.ThrowIfNull(encryptedData, nameof(encryptedData));
            if (optionalEntropy != null && optionalEntropy.Length > 0)
            {
                throw new NotImplementedException("Use of optional entropy is not yet implemented.");
            }

            if (encryptedData.Locate(_axCryptGuid, 0, _axCryptGuid.Length) == 0)
            {
                return null;
            }
            try
            {
                byte[] bytes = _protector.Unprotect(encryptedData);
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
