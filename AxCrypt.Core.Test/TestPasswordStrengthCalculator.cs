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

using AxCrypt.Core.UI;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCrypt.Core.Test
{
    [TestFixture]
    public class TestPasswordStrengthCalculator
    {
        [Test]
        public void TestPasswordStrengthCalculatorInCommonList()
        {
            int estimate;

            estimate = PasswordStrengthCalculator.Estimate("password");
            Assert.That(estimate, Is.EqualTo(0), "password");
            estimate = PasswordStrengthCalculator.Estimate("PASSWORD");
            Assert.That(estimate, Is.EqualTo(0), "PASSWORD");
            estimate = PasswordStrengthCalculator.Estimate("passWord");
            Assert.That(estimate, Is.EqualTo(0), "passWord");
            estimate = PasswordStrengthCalculator.Estimate("Password");
            Assert.That(estimate, Is.EqualTo(0), "Password");
            estimate = PasswordStrengthCalculator.Estimate("secret");
            Assert.That(estimate, Is.EqualTo(0), "secret");
            estimate = PasswordStrengthCalculator.Estimate("xxxxxxxx");
            Assert.That(estimate, Is.EqualTo(0), "xxxxxxxx");
            estimate = PasswordStrengthCalculator.Estimate("psycho");
            Assert.That(estimate, Is.EqualTo(0), "psycho");
            estimate = PasswordStrengthCalculator.Estimate("2424");
            Assert.That(estimate, Is.EqualTo(0), "2424");
            estimate = PasswordStrengthCalculator.Estimate("Mexico");
            Assert.That(estimate, Is.EqualTo(0), "Mexico");
            estimate = PasswordStrengthCalculator.Estimate("4ever");
            Assert.That(estimate, Is.EqualTo(0), "4ever");
        }

        [Test]
        public void TestPasswordStrengthCalculatorTooShort()
        {
            int estimate;

            estimate = PasswordStrengthCalculator.Estimate("a");
            Assert.That(estimate, Is.EqualTo(0), "a");
            estimate = PasswordStrengthCalculator.Estimate("qx");
            Assert.That(estimate, Is.EqualTo(0), "qx");
            estimate = PasswordStrengthCalculator.Estimate("():-");
            Assert.That(estimate, Is.EqualTo(0), "():-");
        }

        [Test]
        public void TestPasswordStrengthCalculatorContainingOnListTooShort()
        {
            int estimate;

            estimate = PasswordStrengthCalculator.Estimate("psychoa");
            Assert.That(estimate, Is.EqualTo(0), "a");
            estimate = PasswordStrengthCalculator.Estimate("qxMexico");
            Assert.That(estimate, Is.EqualTo(0), "qx");
            estimate = PasswordStrengthCalculator.Estimate("()4ever:-");
            Assert.That(estimate, Is.EqualTo(0), "():-");
        }

        [Test]
        public void TestPasswordStrengthCalculatorSpacesAndUpperCase()
        {
            int estimate;
            int baseEstimate;

            baseEstimate = PasswordStrengthCalculator.Estimate("33doettlys{yessoney");
            estimate = PasswordStrengthCalculator.Estimate("33doettlys{yEssoney");
            Assert.That(estimate, Is.EqualTo(baseEstimate + 1), "33doettlys{yEssoney");

            baseEstimate = PasswordStrengthCalculator.Estimate("33doettlys{yessoney");
            estimate = PasswordStrengthCalculator.Estimate("33d Oettlys {yEssoNey");
            Assert.That(estimate, Is.EqualTo(baseEstimate + 5), "33d Oettlys {yEssoNey");
        }

        [Test]
        public void TestPasswordStrengthCalculatorCalibration()
        {
            int estimate;

            estimate = PasswordStrengthCalculator.Estimate("Yutedall");
            Assert.That(estimate, Is.EqualTo(31), "Yutedall");
            estimate = PasswordStrengthCalculator.Estimate("roWinory");
            Assert.That(estimate, Is.EqualTo(31), "roWinory");
            estimate = PasswordStrengthCalculator.Estimate("doDergot");
            Assert.That(estimate, Is.EqualTo(31), "doDergot");
            estimate = PasswordStrengthCalculator.Estimate("feLiedid");
            Assert.That(estimate, Is.EqualTo(31), "feLiedid");

            estimate = PasswordStrengthCalculator.Estimate("Boxe4Derver");
            Assert.That(estimate, Is.EqualTo(44), "Boxe4Derver");
            estimate = PasswordStrengthCalculator.Estimate("47FIngstsion");
            Assert.That(estimate, Is.EqualTo(38), "47FIngstsion");
            estimate = PasswordStrengthCalculator.Estimate("Ken3buSlive");
            Assert.That(estimate, Is.EqualTo(44), "Ken3buSlive");
            estimate = PasswordStrengthCalculator.Estimate("VickEtitin26");
            Assert.That(estimate, Is.EqualTo(50), "VickEtitin26");

            estimate = PasswordStrengthCalculator.Estimate("3pglaTere:cHeinst");
            Assert.That(estimate, Is.EqualTo(74), "3pglaTere:cHeinst");
            estimate = PasswordStrengthCalculator.Estimate("hYesonse&sKilath9");
            Assert.That(estimate, Is.EqualTo(86), "Yutedall");
            estimate = PasswordStrengthCalculator.Estimate("63Sablerst(aWarlie");
            Assert.That(estimate, Is.EqualTo(74), "63Sablerst(aWarlie");
            estimate = PasswordStrengthCalculator.Estimate("61Onwooked[nohoUlty");
            Assert.That(estimate, Is.EqualTo(98), "61Onwooked[nohoUlty");

            estimate = PasswordStrengthCalculator.Estimate("32F35E5D99A24731A7003BB42403F3D4".ToLower());
            Assert.That(estimate, Is.EqualTo(156), "32F35E5D99A24731A7003BB42403F3D4");
            estimate = PasswordStrengthCalculator.Estimate("1005B8C02821440EA36907C7BD21FA1F".ToLower());
            Assert.That(estimate, Is.EqualTo(144), "1005B8C02821440EA36907C7BD21FA1F");
            estimate = PasswordStrengthCalculator.Estimate("AA81862BDE3C48B2BC02517EE678AE2E".ToLower());
            Assert.That(estimate, Is.EqualTo(138), "AA81862BDE3C48B2BC02517EE678AE2E");
            estimate = PasswordStrengthCalculator.Estimate("D0CFB87A87D448568B28A13E8D2523CB".ToLower());
            Assert.That(estimate, Is.EqualTo(162), "D0CFB87A87D448568B28A13E8D2523CB");

            estimate = PasswordStrengthCalculator.Estimate("8A57C81A1E63478E9E6A0E23004C621B1FA95549A9E840B7BD71C2E01F42EC09".ToLower());
            Assert.That(estimate, Is.EqualTo(330), "8A57C81A1E63478E9E6A0E23004C621B1FA95549A9E840B7BD71C2E01F42EC09");
            estimate = PasswordStrengthCalculator.Estimate("5812FA9880604AEAA929A1FD4B02F86E3D7F45BD0FCB4BA7A227885FBA7A5CB4".ToLower());
            Assert.That(estimate, Is.EqualTo(312), "5812FA9880604AEAA929A1FD4B02F86E3D7F45BD0FCB4BA7A227885FBA7A5CB4");
            estimate = PasswordStrengthCalculator.Estimate("FFA226D1BFDE428BB2B48FFBBF14571C3A19C742AD474320A8BC81E98C083B94".ToLower());
            Assert.That(estimate, Is.EqualTo(318), "FFA226D1BFDE428BB2B48FFBBF14571C3A19C742AD474320A8BC81E98C083B94");
            estimate = PasswordStrengthCalculator.Estimate("31E1252B068F42B68AD3F8F3E89953B5DBD400883E644F539B72E977F3620340".ToLower());
            Assert.That(estimate, Is.EqualTo(324), "31E1252B068F42B68AD3F8F3E89953B5DBD400883E644F539B72E977F3620340");
        }
    }
}