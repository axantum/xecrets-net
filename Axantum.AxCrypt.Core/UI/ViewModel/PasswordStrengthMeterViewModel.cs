#region Coypright and License

/*
 * AxCrypt - Copyright 2016, Svante Seleborg, All Rights Reserved
 *
 * This file is part of AxCrypt.
 *
 * AxCrypt is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * AxCrypt is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with AxCrypt.  If not, see <http://www.gnu.org/licenses/>.
 *
 * The source is maintained at http://bitbucket.org/axantum/axcrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Core.UI.ViewModel
{
    public class PasswordStrengthMeterViewModel : ViewModelBase
    {
        private int _goodBits;

        public PasswordStrengthMeterViewModel(int goodBits)
        {
            if (goodBits < 64)
            {
                throw new ArgumentException("Level of good must be better than 64 bits.", nameof(goodBits));
            }

            _goodBits = goodBits;

            InitializePropertyValues();
            BindPropertyChangedEvents();
            SubscribeToModelEvents();
        }

        public string PasswordCandidate { get { return GetProperty<string>(nameof(PasswordCandidate)); } set { SetProperty(nameof(PasswordCandidate), value); } }

        public int EstimatedBits { get { return GetProperty<int>(nameof(EstimatedBits)); } set { SetProperty(nameof(EstimatedBits), value); } }

        public int PercentStrength { get { return GetProperty<int>(nameof(PercentStrength)); } set { SetProperty(nameof(PercentStrength), value); } }

        public PasswordStrength PasswordStrength { get { return GetProperty<PasswordStrength>(nameof(PasswordStrength)); } set { SetProperty(nameof(PasswordStrength), value); } }

        private static void InitializePropertyValues()
        {
        }

        private void BindPropertyChangedEvents()
        {
            BindPropertyChangedInternal(nameof(PasswordCandidate), (string pc) => { TestCandidate(pc); });
        }

        private static void SubscribeToModelEvents()
        {
        }

        private void TestCandidate(string candidate)
        {
            int estimatedBits = PasswordStrengthCalculator.Estimate(candidate);

            double fraction = (double)estimatedBits / (double)_goodBits;
            if (fraction > 1.0)
            {
                fraction = 1.0;
            }

            int percent = (int)Math.Round(fraction * 100);

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

            PercentStrength = percent;
            EstimatedBits = estimatedBits;
            PasswordStrength = strength;
        }
    }
}