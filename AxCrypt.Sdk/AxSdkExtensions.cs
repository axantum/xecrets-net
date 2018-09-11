using System;
using Axantum.AxCrypt.Abstractions.Algorithm;
using Axantum.AxCrypt.Core;
using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Sdk
{
    public static class AxSdkExtensions
    {
        /// <summary>
        /// Creates a random password with the strength as specified by the bits.
        /// </summary>
        /// <param name="bits">The bits.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">bits</exception>
        public static string RandomPassword(this int bits)
        {
            if (bits < 1)
            {
                throw new ArgumentException($"{nameof(bits)} must be greater than 0.");
            }
            int bytes = (bits - 1) / 8 + 1;

            byte[] data = new byte[bytes];
            New<RandomNumberGenerator>().GetBytes(data);

            string password = Convert.ToBase64String(data);
            while (password.EndsWith("="))
            {
                password = password.Substring(0, password.Length - 1);
            }

            return password;
        }

        public static string EncryptedFileName(this string fileName)
        {
            string encryptedFileName = AxCryptFile.MakeAxCryptFileName(fileName);
            return encryptedFileName;
        }
    }
}