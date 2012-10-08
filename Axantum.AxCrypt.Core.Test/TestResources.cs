#region Coypright and License

/*
 * AxCrypt - Copyright 2012, Svante Seleborg, All Rights Reserved
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
 * http://www.axantum.com for more information about the author.
*/

#endregion Coypright and License

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading;
using Axantum.AxCrypt.Core.UI;
using NUnit.Framework;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestResources
    {
        [Test]
        public static void TestIt()
        {
            CultureInfo culture = PublicResources.Culture;
            Assert.That(culture, Is.Null, "There should be no explicitly culture set.");

            PublicResources.Culture = CultureInfo.CreateSpecificCulture("sv-SE");
            Assert.That(PublicResources.Culture, Is.EqualTo(CultureInfo.CreateSpecificCulture("sv-SE")), "The culture should be Swedish now.");

            PublicResources.Culture = culture;
            Assert.That(PublicResources.Culture, Is.Null, "There should be no explicitly culture set again.");

            string license = PublicResources.BouncycastleLicense;
            Assert.That(license, Is.Not.Null, "Just checking that there is a Bouncy Castle License Text.");

            Icon icon = new Icon(PublicResources.AxCryptIcon);
            Assert.That(icon, Is.Not.Null, "Just checking that there is an icon.");
        }
    }
}