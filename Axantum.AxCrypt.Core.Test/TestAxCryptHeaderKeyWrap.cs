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

using System.Diagnostics.CodeAnalysis;
using System.IO;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Reader;
using Axantum.AxCrypt.Core.Test.Properties;
using NUnit.Framework;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public class TestAxCryptHeaderKeyWrap
    {
        [Test]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "This is test, readability and coding ease is a concern, not performance.")]
        public void TestDefaultIterations()
        {
            long defaultIterations = KeyWrap1HeaderBlock.DefaultIterations;

            Assert.That(defaultIterations, Is.GreaterThan(1000), "It really should be larger than 1000...");
        }

        [Test]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "This is test, readability and coding ease is a concern, not performance.")]
        public void TestUnwrapFromSimpleFile()
        {
            using (Stream testStream = new MemoryStream(Resources.helloworld_key_a_txt))
            {
                KeyWrap1HeaderBlock keyWrapHeaderBlock = null;
                using (AxCryptReader axCryptReader = AxCryptReader.Create(testStream))
                {
                    int headers = 0;
                    while (axCryptReader.Read())
                    {
                        switch (axCryptReader.CurrentItemType)
                        {
                            case AxCryptItemType.None:
                                break;

                            case AxCryptItemType.MagicGuid:
                                break;

                            case AxCryptItemType.HeaderBlock:
                                if (axCryptReader.CurrentHeaderBlock.HeaderBlockType == HeaderBlockType.KeyWrap1)
                                {
                                    keyWrapHeaderBlock = (KeyWrap1HeaderBlock)axCryptReader.CurrentHeaderBlock;
                                    ++headers;
                                }
                                break;

                            case AxCryptItemType.Data:
                                break;

                            case AxCryptItemType.EndOfStream:
                                break;
                            default:
                                break;
                        }
                    }
                    Assert.That(headers, Is.EqualTo(1), "We're expecting exactly one KeyWrap1 block to be found!");
                    byte[] wrapped = keyWrapHeaderBlock.GetKeyData();
                    using (KeyWrap keyWrap = new KeyWrap(new Passphrase("a").DerivedPassphrase, keyWrapHeaderBlock.Salt, keyWrapHeaderBlock.Iterations, KeyWrapMode.AxCrypt))
                    {
                        byte[] unwrapped = keyWrap.Unwrap(wrapped);
                        Assert.That(unwrapped.Length, Is.Not.EqualTo(0), "An unwrapped key is invalid if it is returned as a zero-length array.");
                    }
                }
            }
        }
    }
}