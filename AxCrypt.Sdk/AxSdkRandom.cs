using Axantum.AxCrypt.Abstractions.Algorithm;
using System;
using System.Collections.Generic;
using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Sdk
{
    /// <summary>
    /// Random helpers
    /// </summary>
    public class AxSdkRandom
    {
        private RandomNumberGenerator _rng;

        public AxSdkRandom()
        {
            _rng = New<RandomNumberGenerator>();
        }

        public string Password(int bits)
        {
            if (bits < 1)
            {
                throw new ArgumentException($"{nameof(bits)} must be greater than 0.");
            }
            int bytes = (bits - 1) / 8 + 1;

            byte[] data = new byte[bytes];
            _rng.GetBytes(data);

            string password = Convert.ToBase64String(data);
            while (password.EndsWith("="))
            {
                password = password.Substring(0, password.Length - 1);
            }

            return password;
        }
    }
}