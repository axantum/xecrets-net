#region Xecrets Cli Copyright and GPL License notice

/*
 * Xecrets Cli - Changes and additions Copyright © 2022-2025, Svante Seleborg, All Rights Reserved.
 *
 * This code file is part of Xecrets Cli, but is derived from AxCrypt as licensed under GPL v3 or later.
 * 
 * The changes and additions are separately copyrighted and only licensed under GPL v3 or later as detailed below,
 * unless explicitly licensed otherwise. If you use any part of these changes and additions in your software,
 * please see https://www.gnu.org/licenses/ for details of what this means for you.
 * 
 * Warning: If you are using the original AxCrypt code under a non-GPL v3 or later license, these changes and additions
 * are not included in that license. If you use these changes under those circumstances, all your code becomes subject to
 * the GPL v3 or later license, according to the principle of strong copyleft as applied to GPL v3 or later.
 *
 * Xecrets Cli is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * Xecrets Cli is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied
 * warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License along with Xecrets Cli. If not, see
 * https://www.gnu.org/licenses/.
 *
 * The source repository can be found at https://github.com/axantum/xecrets-net please go there for more information,
 * suggestions and contributions, as well for commit history detailing changes and additions that fall under the strong
 * copyleft provisions mentioned above. You may also visit https://www.axantum.com for more information about the author.
*/

#endregion Xecrets Cli Copyright and GPL License notice
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
 * The source is maintained at http://bitbucket.org/AxCrypt.Desktop.Window-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using AxCrypt.Abstractions;
using AxCrypt.Core.UI;
using AxCrypt.Core.UI.ViewModel;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Core.Test
{
    [TestFixture]
    public class TestPasswordStrengthMeterViewModel
    {
        [SetUp]
        public void Setup()
        {
            SetupAssembly.AssemblySetup(Fake.CryptoImplementation.WindowsDesktop);
            TypeMap.Register.Singleton<PasswordStrengthEvaluator>(() => new PasswordStrengthEvaluator(100, 0));
        }

        [TearDown]
        public void Teardown()
        {
            TypeMap.Register.Clear();
        }

        [Test]
        public void TestPasswordStrengthMeterViewModelSimple()
        {
            PasswordStrengthMeterViewModel viewModel = new PasswordStrengthMeterViewModel();

            viewModel.PasswordCandidate = "`Peandled7laSterty";
            Assert.That(viewModel.EstimatedBits, Is.EqualTo(74), nameof(viewModel.EstimatedBits));
            Assert.That(viewModel.PercentStrength, Is.EqualTo(74), nameof(viewModel.PercentStrength));
            Assert.That(viewModel.PasswordStrength, Is.EqualTo(PasswordStrength.Weak), nameof(viewModel.PasswordStrength));
        }

        [Test]
        public void TestPasswordStrengthMeterViewModelUnacceptable()
        {
            PasswordStrengthMeterViewModel viewModel = new PasswordStrengthMeterViewModel();

            viewModel.PasswordCandidate = "Password";
            Assert.That(viewModel.EstimatedBits, Is.EqualTo(0), nameof(viewModel.EstimatedBits));
            Assert.That(viewModel.PercentStrength, Is.EqualTo(0), nameof(viewModel.PercentStrength));
            Assert.That(viewModel.PasswordStrength, Is.EqualTo(PasswordStrength.Unacceptable), nameof(viewModel.PasswordStrength));
        }

        [Test]
        public void TestPasswordStrengthMeterViewModelBad()
        {
            PasswordStrengthMeterViewModel viewModel = new PasswordStrengthMeterViewModel();

            viewModel.PasswordCandidate = "wrOundst";
            Assert.That(viewModel.EstimatedBits, Is.EqualTo(31), nameof(viewModel.EstimatedBits));
            Assert.That(viewModel.PercentStrength, Is.EqualTo(31), nameof(viewModel.PercentStrength));
            Assert.That(viewModel.PasswordStrength, Is.EqualTo(PasswordStrength.Bad), nameof(viewModel.PasswordStrength));
        }

        [Test]
        public void TestPasswordStrengthMeterViewModelStrong()
        {
            PasswordStrengthMeterViewModel viewModel = new PasswordStrengthMeterViewModel();

            viewModel.PasswordCandidate = @"ciStried""Negaist9";
            Assert.That(viewModel.EstimatedBits, Is.EqualTo(80), nameof(viewModel.EstimatedBits));
            Assert.That(viewModel.PercentStrength, Is.EqualTo(80), nameof(viewModel.PercentStrength));
            Assert.That(viewModel.PasswordStrength, Is.EqualTo(PasswordStrength.Strong), nameof(viewModel.PasswordStrength));
        }

        [Test]
        public void TestPasswordStrengthMeterViewModelReallyStrong()
        {
            PasswordStrengthMeterViewModel viewModel = new PasswordStrengthMeterViewModel();

            viewModel.PasswordCandidate = @"ciStried""Negaist9 lAtte86Losed";
            Assert.That(viewModel.EstimatedBits, Is.EqualTo(131), nameof(viewModel.EstimatedBits));
            Assert.That(viewModel.PercentStrength, Is.EqualTo(100), nameof(viewModel.PercentStrength));
            Assert.That(viewModel.PasswordStrength, Is.EqualTo(PasswordStrength.Strong), nameof(viewModel.PasswordStrength));
        }
    }
}
