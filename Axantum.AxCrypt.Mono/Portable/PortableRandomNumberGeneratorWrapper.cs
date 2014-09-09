using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Mono.Portable
{
    internal class PortableRandomNumberGeneratorWrapper : Axantum.AxCrypt.Core.Portable.RandomNumberGenerator
    {
        private System.Security.Cryptography.RandomNumberGenerator _rng;

        public PortableRandomNumberGeneratorWrapper(System.Security.Cryptography.RandomNumberGenerator rng)
        {
            _rng = rng;
        }

        public override void GetBytes(byte[] data)
        {
            _rng.GetBytes(data);
        }

        public override void GetNonZeroBytes(byte[] data)
        {
            _rng.GetNonZeroBytes(data);
        }

        public override void Dispose()
        {
            _rng.Dispose();
        }
    }
}