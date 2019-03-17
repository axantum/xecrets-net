﻿using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.Header;
using Axantum.AxCrypt.Core.IO;
using System;
using System.IO;
using System.Linq;
using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.Session
{
    public class OpenFileProperties
    {
        public int KeyShareCount { get; private set; }

        public bool IsLegacyV1 { get; private set; }

        public bool IsShared { get; private set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public static OpenFileProperties Create(IDataStore dataStore)
        {
            if (dataStore == null)
            {
                throw new ArgumentNullException(nameof(dataStore));
            }

            try
            {
                if (!dataStore.IsAvailable)
                {
                    return new OpenFileProperties();
                }
                using (Stream stream = dataStore.OpenRead())
                {
                    return Create(stream);
                }
            }
            catch (FileNotFoundException fnfex)
            {
                New<IReport>().Exception(fnfex);
                return new OpenFileProperties();
            }
            catch (Exception ex)
            {
                ex.RethrowFileOperation(dataStore.FullName);
                return null;
            }
        }

        public static OpenFileProperties Create(Stream stream)
        {
            OpenFileProperties properties = new OpenFileProperties();
            Headers headers = New<AxCryptFactory>().Headers(stream);
            properties.Fill(headers);
            return properties;
        }

        private void Fill(Headers headers)
        {
            KeyShareCount = GetKeyShareCount(headers);
            IsLegacyV1 = headers.HeaderBlocks.Any(hb => hb.HeaderBlockType == HeaderBlockType.KeyWrap1);
            IsShared = KeyShareCount > 0;
        }

        private int GetKeyShareCount(Headers headers)
        {
            if (!Resolve.KnownIdentities.IsLoggedOn)
            {
                return 0;
            }

            LogOnIdentity identity = Resolve.KnownIdentities.DefaultEncryptionIdentity;
            V2AxCryptDocument v2Document = new V2AxCryptDocument();
            v2Document.Load(identity.Passphrase, Resolve.CryptoFactory.Preferred.CryptoId, headers);
            V2AsymmetricRecipientsEncryptedHeaderBlock headerBlock = v2Document.DocumentHeaders.Headers.FindHeaderBlock<V2AsymmetricRecipientsEncryptedHeaderBlock>();
            if (headerBlock == null)
            {
                return 0;
            }

            return headerBlock.Recipients.PublicKeys.Count(upk => upk.Email != identity.UserEmail);
        }
    }
}