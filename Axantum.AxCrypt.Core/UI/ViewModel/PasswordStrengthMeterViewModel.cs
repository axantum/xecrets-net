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

using AxCrypt.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.UI.ViewModel
{
    public class PasswordStrengthMeterViewModel : ViewModelBase
    {
        public PasswordStrengthMeterViewModel()
        {
            InitializePropertyValues();
            BindPropertyChangedEvents();
            SubscribeToModelEvents();
        }

        public string PasswordCandidate { get { return GetProperty<string>(nameof(PasswordCandidate)); } set { SetProperty(nameof(PasswordCandidate), value); } }

        public int EstimatedBits { get { return GetProperty<int>(nameof(EstimatedBits)); } set { SetProperty(nameof(EstimatedBits), value); } }

        public int PercentStrength { get { return GetProperty<int>(nameof(PercentStrength)); } set { SetProperty(nameof(PercentStrength), value); } }

        public PasswordStrength PasswordStrength { get { return GetProperty<PasswordStrength>(nameof(PasswordStrength)); } set { SetProperty(nameof(PasswordStrength), value); } }

        public string StrengthTip
        {
            get
            {
                switch (PasswordStrength)
                {
                    case PasswordStrength.Unacceptable:
                        return Texts.PasswordStrengthUnacceptableTip;

                    case PasswordStrength.Bad:
                        return Texts.PasswordStrengthBadTip;

                    case PasswordStrength.Weak:
                        return Texts.PasswordStrengthWeakTip;

                    case PasswordStrength.Strong:
                        return Texts.PasswordStrengthStrongTip;

                    default:
                        return string.Empty;
                }
            }
        }

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
            PasswordMetrics metrics = New<PasswordStrengthEvaluator>().Evaluate(candidate);

            PercentStrength = metrics.Percent;
            EstimatedBits = metrics.Bits;
            PasswordStrength = metrics.Strength;
        }
    }
}