using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCrypt.Api.Extension
{
    public static class CipherTypeExtension
    {
        public static string GetCipherBytes(this byte[] cipher)
        {
            return Convert.ToBase64String(cipher);
        }

        public static byte[] GetCipherString(this string cipher)
        {
            return Convert.FromBase64String(cipher);
        }
    }
}
