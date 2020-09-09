using AxCrypt.Core.Crypto;
using System;
using System.Collections.Generic;

namespace AxCrypt.Sdk
{
    public class AxSdkConfiguration
    {
        public Uri ApiBaseUrl { get; set; } = new Uri("https://account.axcrypt.net/api/");

        public TimeSpan ApiTimeout { get; set; } = TimeSpan.FromSeconds(120);

        public Guid CryptoId { get; set; } = new V2Aes256CryptoFactory().CryptoId;

        public bool Copmress { get; set; } = true;
    }
}