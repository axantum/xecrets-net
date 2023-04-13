using AxCrypt.Core.Extensions;
using AxCrypt.Core.Header;
using AxCrypt.Core.IO;
using System;
using System.IO;
using System.Linq;
using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Core.Session
{
    public class OpenFileProperties
    {
        public bool IsLegacyV1 { get; private set; }

        public int V2AsymetricKeyWrapCount { get; private set; }

        public bool V2AsymetricMasterKey { get; private set; }

        private OpenFileProperties()
        {
        }

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
                using Stream stream = dataStore.OpenRead();
                return Create(stream);
            }
            catch (Exception ex)
            {
                ex.RethrowFileOperation(dataStore.FullName);
                return null!; // This is never reached, the exception is rethrown
            }
        }

        private static OpenFileProperties Create(Stream stream)
        {
            OpenFileProperties properties = new OpenFileProperties();
            properties.Fill(stream);
            return properties;
        }

        private void Fill(Stream stream)
        {
            Headers headers = New<AxCryptFactory>().Headers(stream);

            IsLegacyV1 = headers.HeaderBlocks.Any(hb => hb.HeaderBlockType == HeaderBlockType.KeyWrap1);
            V2AsymetricKeyWrapCount = headers.HeaderBlocks.Count(hb => hb.HeaderBlockType == HeaderBlockType.V2AsymmetricKeyWrap);
            V2AsymetricMasterKey = headers.HeaderBlocks.Any(hb => hb.HeaderBlockType == HeaderBlockType.AsymmetricMasterKey);
        }
    }
}
