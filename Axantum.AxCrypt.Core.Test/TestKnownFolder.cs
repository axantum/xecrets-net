#region Coypright and License

/*
 * AxCrypt - Copyright 2014, Svante Seleborg, All Rights Reserved
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

using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Portable;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.UI;
using Axantum.AxCrypt.Mono.Portable;
using NUnit.Framework;
using System;
using System.Drawing;
using System.Linq;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestKnownFolder
    {
        [SetUp]
        public static void Setup()
        {
            Factory.Instance.Register<string, IRuntimeFileInfo>((path) => new FakeRuntimeFileInfo(path));
            Factory.Instance.Singleton<IRuntimeEnvironment>(() => new FakeRuntimeEnvironment());
            Factory.Instance.Singleton<IPortableFactory>(() => new PortableFactory());
        }

        [TearDown]
        public static void Teardown()
        {
            Factory.Instance.Clear();
        }

        [Test]
        public static void TestNormalConstructor()
        {
            Bitmap image = new Bitmap(32, 32);
            Uri providerUrl = new Uri("http://localhost/AxCrypt/");

            KnownFolder kf = new KnownFolder(@"C:\Users\AxCrypt\My Documents", @"AxCrypt", image, providerUrl);
            Assert.That(kf.RootFullPath.FullName, Is.EqualTo(@"C:\Users\AxCrypt\My Documents".NormalizeFolderPath()));
            Assert.That(kf.MyFullPath.FullName, Is.EqualTo(@"C:\Users\AxCrypt\My Documents\AxCrypt".NormalizeFolderPath()));
            Assert.That(kf.Image, Is.EqualTo(image));
            Assert.That(kf.ProviderUrl, Is.EqualTo(providerUrl));
            Assert.That(kf.Enabled, Is.False);
        }

        [Test]
        public static void TestCopyConstructor()
        {
            Bitmap image = new Bitmap(32, 32);
            Uri providerUrl = new Uri("http://localhost/AxCrypt/");

            KnownFolder kf = new KnownFolder(@"C:\Users\AxCrypt\My Documents", @"AxCrypt", image, providerUrl);
            KnownFolder kfCopy = new KnownFolder(kf, true);

            Assert.That(kfCopy.RootFullPath.FullName, Is.EqualTo(@"C:\Users\AxCrypt\My Documents".NormalizeFolderPath()));
            Assert.That(kfCopy.MyFullPath.FullName, Is.EqualTo(@"C:\Users\AxCrypt\My Documents\AxCrypt".NormalizeFolderPath()));
            Assert.That(kfCopy.Image, Is.EqualTo(image));
            Assert.That(kfCopy.ProviderUrl, Is.EqualTo(providerUrl));
            Assert.That(kfCopy.Enabled, Is.True);
        }

        [Test]
        public static void TestBadArgumentsToConstructor()
        {
            Bitmap image = new Bitmap(32, 32);
            Uri providerUrl = new Uri("http://localhost/AxCrypt/");

            Bitmap nullImage = null;
            Uri nullUrl = null;
            string nullString = null;

            KnownFolder kf;

            Assert.Throws<ArgumentNullException>(() => kf = new KnownFolder(nullString, @"AxCrypt", image, providerUrl));
            Assert.Throws<ArgumentNullException>(() => kf = new KnownFolder(@"C:\Users\AxCrypt\My Documents", nullString, image, providerUrl));
            Assert.Throws<ArgumentNullException>(() => kf = new KnownFolder(@"C:\Users\AxCrypt\My Documents", @"AxCrypt", nullImage, providerUrl));
            Assert.DoesNotThrow(() => kf = new KnownFolder(@"C:\Users\AxCrypt\My Documents", @"AxCrypt", image, nullUrl));
            kf = null;
            Assert.Throws<ArgumentNullException>(() => kf = new KnownFolder(kf, true));
        }
    }
}