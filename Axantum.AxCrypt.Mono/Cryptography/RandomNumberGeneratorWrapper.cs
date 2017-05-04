using Axantum.AxCrypt.Abstractions.Algorithm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Mono.Cryptography
{
    internal class RandomNumberGeneratorWrapper : RandomNumberGenerator
    {
        private System.Security.Cryptography.RandomNumberGenerator _rng;

        public RandomNumberGeneratorWrapper(System.Security.Cryptography.RandomNumberGenerator rng)
        {
            _rng = rng;
        }

        public override void GetBytes(byte[] data)
        {
            _rng.GetBytes(data);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _rng.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}