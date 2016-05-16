﻿using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Core.Header;
using Axantum.AxCrypt.Core.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.Session
{
    public class OpenFileProperties
    {
        public static OpenFileProperties Create(IDataStore dataStore)
        {
            if (dataStore == null)
            {
                throw new ArgumentNullException(nameof(dataStore));
            }

            try
            {
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
            KeyShareCount = headers.HeaderBlocks.Count(hb => hb.HeaderBlockType == HeaderBlockType.V2AsymmetricKeyWrap);
            IsLegacyV1 = headers.HeaderBlocks.Any(hb => hb.HeaderBlockType == HeaderBlockType.KeyWrap1);
        }

        public int KeyShareCount { get; private set; }

        public bool IsLegacyV1 { get; private set; }
    }
}