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
using AxCrypt.Core.IO;
using AxCrypt.Core.UI;
using AxCrypt.Fake;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestParallelFileOperation
    {
        [SetUp]
        public static void Setup()
        {
            SetupAssembly.AssemblySetup(CryptoImplementation.WindowsDesktop);
        }

        [TearDown]
        public static void Teardown()
        {
            SetupAssembly.AssemblyTeardown();
        }

        [Test]
        public static async Task TestParallelFileOperationSimple()
        {
            IDataStore info1 = New<IDataStore>(@"c:\file1.txt");
            IDataStore info2 = New<IDataStore>(@"c:\file2.txt");
            ParallelFileOperation pfo = new ParallelFileOperation();
            int callCount = 0;
            await pfo.DoFilesAsync(new IDataStore[] { info1, info2 },
                (info, progress) =>
                {
                    ++callCount;
                    return Task.FromResult(new FileOperationContext(String.Empty, ErrorStatus.Success));
                },
                (status) =>
                {
                    return Constant.CompletedTask;
                });

            Assert.That(callCount, Is.EqualTo(2), "There are two files, so there should be two calls.");
        }

        [Test]
        public static async Task TestQuitAllOnError()
        {
            FakeUIThread fakeUIThread = new FakeUIThread();
            fakeUIThread.IsOn = true;
            TypeMap.Register.Singleton<IUIThread>(() => fakeUIThread);

            FakeRuntimeEnvironment.Instance.MaxConcurrency = 2;

            IDataStore info1 = New<IDataStore>(@"c:\file1.txt");
            IDataStore info2 = New<IDataStore>(@"c:\file2.txt");
            IDataStore info3 = New<IDataStore>(@"c:\file3.txt");
            IDataStore info4 = New<IDataStore>(@"c:\file4.txt");
            ParallelFileOperation pfo = new ParallelFileOperation();

            int callCount = 0;
            await pfo.DoFilesAsync(new IDataStore[] { info1, info2, info3, info4 },
                (info, progress) =>
                {
                    int result = Interlocked.Increment(ref callCount);
                    if (result == 1)
                    {
                        return Task.FromResult(new FileOperationContext(String.Empty, ErrorStatus.UnspecifiedError));
                    }
                    Thread.Sleep(1);
                    return Task.FromResult(new FileOperationContext(String.Empty, ErrorStatus.Success));
                },
                (status) => Constant.CompletedTask);
            Assert.That(callCount, Is.LessThanOrEqualTo(2), "There are several files, but max concurrency is two, so there could be up to two calls.");
        }
    }
}
