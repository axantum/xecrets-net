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

using AxCrypt.Core.Header;
using NUnit.Framework;
using System;
using System.Linq;

namespace AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestHeaderBlock
    {
        [SetUp]
        public static void Setup()
        {
        }

        [TearDown]
        public static void Teardown()
        {
        }

        private class TestingHeaderBlock : HeaderBlock
        {
            public TestingHeaderBlock(HeaderBlockType headerBlockType, byte[] dataBlock)
                : base(headerBlockType, dataBlock)
            {
            }

            public override object Clone()
            {
                throw new NotImplementedException();
            }
        }

        [Test]
        public static void TestEquals()
        {
            byte[] data = new byte[] { 5, 6, 7, 8, 9, 10 };
            byte[] other = new byte[] { 5, 6, 7, 8, 9, 11 };

            TestingHeaderBlock block1 = new TestingHeaderBlock((HeaderBlockType)33, data);
            TestingHeaderBlock block2 = new TestingHeaderBlock((HeaderBlockType)33, data);
            TestingHeaderBlock block3 = new TestingHeaderBlock((HeaderBlockType)33, other);
            TestingHeaderBlock block4 = new TestingHeaderBlock((HeaderBlockType)34, data);
            TestingHeaderBlock nullBlock = null;

            Assert.That(block1.Equals(block2));
            Assert.That(block2.Equals(block1));
            Assert.That(!block1.Equals(block3));
            Assert.That(!block1.Equals(block4));
            Assert.That(!block1.Equals(nullBlock));
        }

        [Test]
        public static void TestObjectEquals()
        {
            byte[] data = new byte[] { 5, 6, 7, 8, 9, 10 };
            byte[] other = new byte[] { 5, 6, 7, 8, 9, 11 };

            object block1 = new TestingHeaderBlock((HeaderBlockType)33, data);
            object block2 = new TestingHeaderBlock((HeaderBlockType)33, data);
            object block3 = new TestingHeaderBlock((HeaderBlockType)33, other);
            object block4 = new TestingHeaderBlock((HeaderBlockType)34, data);
            object nullBlock = null;

            Assert.That(block1.Equals(block2));
            Assert.That(block2.Equals(block1));
            Assert.That(!block1.Equals(block3));
            Assert.That(!block1.Equals(block4));
            Assert.That(!block1.Equals(nullBlock));
        }

        [Test]
        public static void TestGetHashCode()
        {
            byte[] data = new byte[] { 5, 6, 7, 8, 9, 10 };
            byte[] other = new byte[] { 5, 6, 7, 8, 9, 11 };

            TestingHeaderBlock block1 = new TestingHeaderBlock((HeaderBlockType)33, data);
            TestingHeaderBlock block2 = new TestingHeaderBlock((HeaderBlockType)33, data);
            TestingHeaderBlock block3 = new TestingHeaderBlock((HeaderBlockType)33, other);
            TestingHeaderBlock block4 = new TestingHeaderBlock((HeaderBlockType)34, data);

            Assert.That(block1.GetHashCode(), Is.EqualTo(block2.GetHashCode()));
            Assert.That(block1.GetHashCode(), Is.Not.EqualTo(block3.GetHashCode()));
            Assert.That(block1.GetHashCode(), Is.Not.EqualTo(block4.GetHashCode()));
        }

        [Test]
        public static void TestOperatorEquals()
        {
            byte[] data = new byte[] { 5, 6, 7, 8, 9, 10 };
            byte[] other = new byte[] { 5, 6, 7, 8, 9, 11 };

            TestingHeaderBlock block1 = new TestingHeaderBlock((HeaderBlockType)33, data);
            TestingHeaderBlock block1alias = block1;
            TestingHeaderBlock block2 = new TestingHeaderBlock((HeaderBlockType)33, data);
            TestingHeaderBlock block3 = new TestingHeaderBlock((HeaderBlockType)33, other);
            TestingHeaderBlock block4 = new TestingHeaderBlock((HeaderBlockType)34, data);
            TestingHeaderBlock nullBlock = null;
            TestingHeaderBlock nullBlockAlias = nullBlock;

            Assert.That(block1 == block1alias);
            Assert.That(block1 == block2);
            Assert.That(block2 == block1);
            Assert.That(block1 != block3);
            Assert.That(block1 != block4);
            Assert.That(block1 != nullBlock);
            Assert.That(nullBlock != block1);
            Assert.That(nullBlock == nullBlockAlias);
        }
    }
}