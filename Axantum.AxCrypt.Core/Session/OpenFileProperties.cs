using Axantum.AxCrypt.Abstractions;
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
            V2DocumentHeaders documentHeaders = LoadDocumentHeaders(headers, identity.Passphrase);

            V2AsymmetricRecipientsEncryptedHeaderBlock headerBlock = documentHeaders.Headers.FindHeaderBlock<V2AsymmetricRecipientsEncryptedHeaderBlock>();
            if (headerBlock == null)
            {
                return 0;
            }

            return headerBlock.Recipients.PublicKeys.Count(upk => upk.Email != identity.UserEmail);
        }

        private V2DocumentHeaders LoadDocumentHeaders(Headers headers, Passphrase passphrase)
        {
            ICryptoFactory cryptoFactory = Resolve.CryptoFactory.Create(Resolve.CryptoFactory.Preferred.CryptoId);
            V2KeyWrapHeaderBlock keyWrap = headers.FindHeaderBlock<V2KeyWrapHeaderBlock>();

            IDerivedKey key = cryptoFactory.RestoreDerivedKey(passphrase, keyWrap.DerivationSalt, keyWrap.DerivationIterations);
            keyWrap.SetDerivedKey(cryptoFactory, key);

            V2DocumentHeaders documentHeaders = new V2DocumentHeaders(keyWrap);
            documentHeaders.Load(headers);

            return documentHeaders;
        }
    }
}