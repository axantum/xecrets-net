using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Crypto.Asymmetric;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Extensions
{
    public static class UserTypeExtensions
    {
        public static string ToFileString(this PublicKeyThumbprint thumbprint)
        {
            if (thumbprint == null)
            {
                throw new ArgumentNullException("thumbprint");
            }

            string base64 = Convert.ToBase64String(thumbprint.ToByteArray());
            string fileString = base64.Substring(0, base64.Length - 2).Replace('/', '-');

            return fileString;
        }

        public static PublicKeyThumbprint ToPublicKeyThumbprint(this string thumbprint)
        {
            if (thumbprint == null)
            {
                throw new ArgumentNullException("thumbprint");
            }
            if (thumbprint.Length != 22)
            {
                throw new ArgumentException("Length must be 128 bits base 64 without padding.", "thumbprint");
            }
            byte[] bytes;
            try
            {
                bytes = Convert.FromBase64String(thumbprint.Replace('-', '/') + "==");
            }
            catch (FormatException)
            {
                throw new ArgumentException("Incorrect base64 encoding.", "thumbprint");
            }

            return new PublicKeyThumbprint(bytes);
        }

        public static string ToUtf8Base64(this Passphrase passphrase)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(passphrase.Text));
        }
    }
}