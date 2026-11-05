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

using System.Collections.Generic;
using System.Linq;
using AxCrypt.Core.IO;
using AxCrypt.Fake;
using NUnit.Framework;

using System;

using System.IO;
using System.Text.RegularExpressions;

namespace AxCrypt.Core.Test
{
    [TestFixture]
    public class TestFileFilter
    {
        [SetUp]
        public void Setup()
        {
            SetupAssembly.AssemblySetup(CryptoImplementation.WindowsDesktop);
        }

        [TearDown]
        public void Teardown()
        {
            SetupAssembly.AssemblyTeardown();
        }

        [Test]
        public void FileFilterEmptyFilter()
        {
            FileFilter filter = new FileFilter();

            Assert.That(filter.IsEncryptable(new FakeDataStore(@"test.axx")), Is.False);
            Assert.That(filter.IsEncryptable(new FakeDataStore(@"test.txt")), Is.True);
        }

        [Test]
        public void FileFilterThrowsArgumentNull()
        {
            FileFilter filter = new FileFilter();

            Assert.Throws<ArgumentNullException>(() => filter.AddUnencryptable(null));
            Assert.Throws<ArgumentNullException>(() => filter.AddUnencryptableExtension(null));
            Assert.Throws<ArgumentNullException>(() => filter.IsEncryptable(null));
        }

        [Test]
        public void FileFilterTestUnencryptablePatterns()
        {
            FileFilter filter = new FileFilter();

            string s = $"\\{Path.DirectorySeparatorChar}";
            filter.AddUnencryptable(new Regex(@"\\\.dropbox$".Replace(@"\\", s)));
            filter.AddUnencryptable(new Regex(@"\\desktop\.ini$".Replace(@"\\", s)));
            filter.AddUnencryptable(new Regex(@".*\.tmp$"));

            Assert.That(filter.IsEncryptable(new FakeDataStore(@"anywhere\.dropbox".Replace('\\', Path.DirectorySeparatorChar))), Is.False);
            Assert.That(filter.IsEncryptable(new FakeDataStore(@"C:\Somewhere\file.dropbox".Replace('\\', Path.DirectorySeparatorChar))), Is.True);
            Assert.That(filter.IsEncryptable(new FakeDataStore(@".dropboxx".Replace('\\', Path.DirectorySeparatorChar))), Is.True);
            Assert.That(filter.IsEncryptable(new FakeDataStore(@".dropbo".Replace('\\', Path.DirectorySeparatorChar))), Is.True);
            Assert.That(filter.IsEncryptable(new FakeDataStore(@"folder\desktop.ini".Replace('\\', Path.DirectorySeparatorChar))), Is.False);
            Assert.That(filter.IsEncryptable(new FakeDataStore(@"file-desktop.ini".Replace('\\', Path.DirectorySeparatorChar))), Is.True);
            Assert.That(filter.IsEncryptable(new FakeDataStore(@"anything.tmp".Replace('\\', Path.DirectorySeparatorChar))), Is.False);
            Assert.That(filter.IsEncryptable(new FakeDataStore(@"C:\anywhere\anything.tmp.tmp".Replace('\\', Path.DirectorySeparatorChar))), Is.False);
            Assert.That(filter.IsEncryptable(new FakeDataStore(@"C:\anywhere\anything.tmp.tmpx".Replace('\\', Path.DirectorySeparatorChar))), Is.True);
        }

        [Test]
        public void FileFilterTestOfficeTemporaryFiles()
        {
            FileFilter filter = new FileFilter();

            string s = $"\\{Path.DirectorySeparatorChar}";
            filter.AddUnencryptable(new Regex(@"^.*\\~\$[^\\]*$".Replace(@"\\", s)));

            Assert.That(filter.IsEncryptable(new FakeDataStore(@"C:\Folder\~$".Replace('\\', Path.DirectorySeparatorChar))), Is.False);
            Assert.That(filter.IsEncryptable(new FakeDataStore(@"C:\Folder\~$\".Replace('\\', Path.DirectorySeparatorChar))), Is.True);
            Assert.That(filter.IsEncryptable(new FakeDataStore(@"C:\Folder\~$\~$".Replace('\\', Path.DirectorySeparatorChar))), Is.False);
            Assert.That(filter.IsEncryptable(new FakeDataStore(@"C:\Folder\~$Temp".Replace('\\', Path.DirectorySeparatorChar))), Is.False);
            Assert.That(filter.IsEncryptable(new FakeDataStore(@"C:\Folder\~$Temp.docx".Replace('\\', Path.DirectorySeparatorChar))), Is.False);
            Assert.That(filter.IsEncryptable(new FakeDataStore(@"C:\Folder\~$Temp.docx\The-file.txt".Replace('\\', Path.DirectorySeparatorChar))), Is.True);
        }

        [Test]
        public void FileFilterTestUnencryptableExtension()
        {
            FileFilter filter = new FileFilter();

            filter.AddUnencryptableExtension("gdoc");

            Assert.That(filter.IsEncryptable(new FakeDataStore(@"file.gdoc")), Is.False);
            Assert.That(filter.IsEncryptable(new FakeDataStore(@"file..gdoc")), Is.False);
            Assert.That(filter.IsEncryptable(new FakeDataStore(@"anywhere\file.gdoc")), Is.False);
            Assert.That(filter.IsEncryptable(new FakeDataStore(@"file.gdoc\file.txt")), Is.True);
            Assert.That(filter.IsEncryptable(new FakeDataStore(@"filegdoc")), Is.True);
            Assert.That(filter.IsEncryptable(new FakeDataStore(@"filegdoc.txt")), Is.True);
        }

        [Test]
        public void FileFilterTestForbiddenFolder()
        {
            FileFilter filter = new FileFilter();

            Assert.Throws<ArgumentNullException>(() => filter.AddForbiddenFolderFilters(null));
            Assert.Throws<ArgumentNullException>(() => filter.IsForbiddenFolder(null));

            filter.AddForbiddenFolderFilters(@"c:\programdata\".Replace('\\', Path.DirectorySeparatorChar));
            filter.AddForbiddenFolderFilters(@"c:\program files (x86)\".Replace('\\', Path.DirectorySeparatorChar));
            filter.AddForbiddenFolderFilters(@"c:\program files\".Replace('\\', Path.DirectorySeparatorChar));
            filter.AddForbiddenFolderFilters(@"c:\windows\".Replace('\\', Path.DirectorySeparatorChar));

            Assert.That(filter.IsForbiddenFolder(@"C:\ProgramData".Replace('\\', Path.DirectorySeparatorChar)), Is.True);
            Assert.That(filter.IsForbiddenFolder(@"C:\Program Files (x86)".Replace('\\', Path.DirectorySeparatorChar)), Is.True);
            Assert.That(filter.IsForbiddenFolder(@"C:\WINDOWS".Replace('\\', Path.DirectorySeparatorChar)), Is.True);
            Assert.That(filter.IsForbiddenFolder(@"C:\Program Files".Replace('\\', Path.DirectorySeparatorChar)), Is.True);
            Assert.That(filter.IsForbiddenFolder(@"C:\Temp".Replace('\\', Path.DirectorySeparatorChar)), Is.False);
            Assert.That(filter.IsForbiddenFolder(@"C:\Windows\Temp".Replace('\\', Path.DirectorySeparatorChar)), Is.True);
        }
    }
}
