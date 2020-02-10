using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.UI
{
    public class PasswordStrengthEvaluator
    {
        private int _goodBits;

        private int _minimumEffectiveLength;

        public PasswordStrengthEvaluator(int goodBits, int minimumEffectiveLength)
        {
            if (goodBits < 64)
            {
                throw new ArgumentException("Level of good must be better than 64 bits.", nameof(goodBits));
            }
            if (minimumEffectiveLength < 0)
            {
                throw new ArgumentException("Minimum length must be positive or zero.", nameof(minimumEffectiveLength));
            }

            _goodBits = goodBits;
            _minimumEffectiveLength = minimumEffectiveLength;
        }

        public PasswordMetrics Evaluate(string candidate)
        {
            int estimatedBits;
            if (PasswordStrengthCalculator.Effective(candidate).Length < _minimumEffectiveLength)
            {
                estimatedBits = 0;
            }
            else
            {
                estimatedBits = PasswordStrengthCalculator.Estimate(candidate);
            }

            double fraction = (double)estimatedBits / (double)_goodBits;
            if (fraction > 1.0)
            {
                fraction = 1.0;
            }

            int percent = (int)Math.Round(fraction * 100);

            if (candidate.Equals(New<UserSettings>().UserEmail))
            {
                return new PasswordMetrics(PasswordStrength.Unacceptable, estimatedBits, percent);
            }

            PasswordStrength strength = PasswordStrength.Bad;
            if (percent == 0)
            {
                strength = PasswordStrength.Unacceptable;
            }
            if (percent > 50 && percent < 75)
            {
                strength = PasswordStrength.Weak;
            }
            if (percent >= 75)
            {
                strength = PasswordStrength.Strong;
            }

            return new PasswordMetrics(strength, estimatedBits, percent);
        }
    }
}