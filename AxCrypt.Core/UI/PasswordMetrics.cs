using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCrypt.Core.UI
{
    public class PasswordMetrics
    {
        public PasswordMetrics(PasswordStrength strength, int bits, int percent)
        {
            Strength = strength;
            Bits = bits;
            Percent = percent;
        }

        public int Bits { get; private set; }

        public int Percent { get; private set; }

        public PasswordStrength Strength { get; private set; }
    }
}