using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Axantum.AxCrypt.Core.Reader
{
    public class AxCryptReaderSettings
    {
        private string _passphrase;

        public AxCryptReaderSettings()
        {
        }

        public AxCryptReaderSettings(string passphrase)
        {
            _passphrase = passphrase;
        }

        public byte[] GetDerivedPassphrase()
        {
            if (_passphrase == null)
            {
                return new byte[0];
            }

            HashAlgorithm hashAlgorithm = new SHA1Managed();
            byte[] ansiBytes = Encoding.GetEncoding(1252).GetBytes(_passphrase);
            byte[] hash = hashAlgorithm.ComputeHash(ansiBytes);
            byte[] derivedPassphrase = new byte[16];
            Array.Copy(hash, derivedPassphrase, derivedPassphrase.Length);

            return derivedPassphrase;
        }
    }
}