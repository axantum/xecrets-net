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
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading;
using NUnit.Framework;
using CoreResources = Axantum.AxCrypt.Core.Properties.Resources;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestResources
    {
        [Test]
        public static void TestIt()
        {
            ResourceManager manager = CoreResources.ResourceManager;
            Assert.That(manager, Is.Not.Null, "There should be a ResourceManager in Core.");

            CultureInfo culture = CoreResources.Culture;
            Assert.That(culture, Is.Null, "The culture property should not be set.");

            CoreResources.Culture = CultureInfo.CreateSpecificCulture("sv-SE");
            Assert.That(CoreResources.Culture, Is.Not.Null, "The default culture should now be set.");
            Assert.That(CoreResources.Culture, Is.EqualTo(CultureInfo.CreateSpecificCulture("sv-SE")), "The culture should now be the Sweden-Swedish culture.");

            CoreResources.Culture = culture;
            Assert.That(CoreResources.Culture, Is.Null, "The culture should again be null.");
        }
    }
}