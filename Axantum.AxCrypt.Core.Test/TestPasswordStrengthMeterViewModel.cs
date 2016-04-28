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

using Axantum.AxCrypt.Core.UI.ViewModel;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public class TestPasswordStrengthMeterViewModel
    {
        [Test]
        public void TestPasswordStrengthMeterViewModelSimple()
        {
            PasswordStrengthMeterViewModel viewModel = new PasswordStrengthMeterViewModel(100);

            viewModel.PasswordCandidate = "`Peandled7laSterty";
            Assert.That(viewModel.EstimatedBits, Is.EqualTo(74), nameof(viewModel.EstimatedBits));
            Assert.That(viewModel.PercentStrength, Is.EqualTo(74), nameof(viewModel.PercentStrength));
            Assert.That(viewModel.PasswordStrength, Is.EqualTo(PasswordStrength.Weak), nameof(viewModel.PasswordStrength));
        }

        [Test]
        public void TestPasswordStrengthMeterViewModelUnacceptable()
        {
            PasswordStrengthMeterViewModel viewModel = new PasswordStrengthMeterViewModel(100);

            viewModel.PasswordCandidate = "Password";
            Assert.That(viewModel.EstimatedBits, Is.EqualTo(0), nameof(viewModel.EstimatedBits));
            Assert.That(viewModel.PercentStrength, Is.EqualTo(0), nameof(viewModel.PercentStrength));
            Assert.That(viewModel.PasswordStrength, Is.EqualTo(PasswordStrength.Unacceptable), nameof(viewModel.PasswordStrength));
        }

        [Test]
        public void TestPasswordStrengthMeterViewModelBad()
        {
            PasswordStrengthMeterViewModel viewModel = new PasswordStrengthMeterViewModel(100);

            viewModel.PasswordCandidate = "wrOundst";
            Assert.That(viewModel.EstimatedBits, Is.EqualTo(31), nameof(viewModel.EstimatedBits));
            Assert.That(viewModel.PercentStrength, Is.EqualTo(31), nameof(viewModel.PercentStrength));
            Assert.That(viewModel.PasswordStrength, Is.EqualTo(PasswordStrength.Bad), nameof(viewModel.PasswordStrength));
        }

        [Test]
        public void TestPasswordStrengthMeterViewModelStrong()
        {
            PasswordStrengthMeterViewModel viewModel = new PasswordStrengthMeterViewModel(100);

            viewModel.PasswordCandidate = @"ciStried""Negaist9";
            Assert.That(viewModel.EstimatedBits, Is.EqualTo(80), nameof(viewModel.EstimatedBits));
            Assert.That(viewModel.PercentStrength, Is.EqualTo(80), nameof(viewModel.PercentStrength));
            Assert.That(viewModel.PasswordStrength, Is.EqualTo(PasswordStrength.Strong), nameof(viewModel.PasswordStrength));
        }

        [Test]
        public void TestPasswordStrengthMeterViewModelReallyStrong()
        {
            PasswordStrengthMeterViewModel viewModel = new PasswordStrengthMeterViewModel(100);

            viewModel.PasswordCandidate = @"ciStried""Negaist9 lAtte86Losed";
            Assert.That(viewModel.EstimatedBits, Is.EqualTo(131), nameof(viewModel.EstimatedBits));
            Assert.That(viewModel.PercentStrength, Is.EqualTo(100), nameof(viewModel.PercentStrength));
            Assert.That(viewModel.PasswordStrength, Is.EqualTo(PasswordStrength.Strong), nameof(viewModel.PasswordStrength));
        }
    }
}