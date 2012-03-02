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
 * The source is maintained at http://AxCrypt.codeplex.com/ please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axantum.com for more information about the author.
*/

#endregion Coypright and License

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Axantum.AxCrypt.Core.Reader;
using Axantum.AxCrypt.Core.Test.Properties;
using NUnit.Framework;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public class TestAxCryptDocument
    {
        [Test]
        public static void TestFileNameFromSimpleFile()
        {
            using (Stream testStream = new MemoryStream(Resources.HelloWorld_Key_a_txt))
            {
                AxCryptDocument document = new AxCryptDocument();
                AxCryptReaderSettings settings = new AxCryptReaderSettings("a");
                using (AxCryptReader axCryptReader = AxCryptReader.Create(testStream, settings))
                {
                    document.Load(axCryptReader);
                    string fileName = document.FileName;
                    Assert.That(fileName, Is.EqualTo("HelloWorld-Key-a.txt"), "Wrong file name");
                }
            }
        }
    }
}