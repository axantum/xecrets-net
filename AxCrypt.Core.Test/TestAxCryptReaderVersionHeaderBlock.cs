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
using AxCrypt.Core.IO;
using AxCrypt.Core.Reader;
using Xecrets.File.Core.Test.Properties;
using AxCrypt.Fake;
using NUnit.Framework;
using System.IO;

namespace AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestAxCryptReaderVersionHeaderBlock
    {
        [SetUp]
        public static void Setup()
        {
            SetupAssembly.AssemblySetup();
        }

        [TearDown]
        public static void Teardown()
        {
            SetupAssembly.AssemblyTeardown();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times"), Test]
        public static void TestFindVersionHeaderBlockFromSimpleFile()
        {
            using (Stream testStream = FakeDataStore.ExpandableMemoryStream(Resources.helloworld_key_a_txt))
            {
                using (V1AxCryptReader axCryptReader = new V1AxCryptReader(new LookAheadStream(testStream)))
                {
                    bool blockFound = false;
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
                                if (axCryptReader.CurrentHeaderBlock.HeaderBlockType == HeaderBlockType.Version)
                                {
                                    Assert.That(blockFound, Is.False, "We should only find one single VersionHeaderBlock");
                                    blockFound = true;
                                }
                                ++headers;
                                break;

                            case AxCryptItemType.Data:
                                break;

                            case AxCryptItemType.EndOfStream:
                                break;

                            default:
                                break;
                        }
                    }
                    Assert.That(blockFound, Is.True, "We're expecting a VersionHeaderBlock to be found!");
                }
            }
        }
    }
}
