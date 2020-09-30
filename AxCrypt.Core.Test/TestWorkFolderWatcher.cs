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
using AxCrypt.Core.Runtime;
using AxCrypt.Core.Session;
using AxCrypt.Fake;
using Moq;
using NUnit.Framework;
using System;
using System.Linq;

using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestWorkFolderWatcher
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

        [Test]
        public static void TestWorkFolderWatcherSimple()
        {
            TypeMap.Register.Singleton<SessionNotify>(() => new Mock<SessionNotify>().Object);

            using (WorkFolderWatcher wfw = new WorkFolderWatcher())
            {
                string fileName = Resolve.WorkFolder.FileInfo.FileItemInfo("New File.txt").FullName;
                FakeDataStore.AddFile(fileName, null);

                Mock.Get(Resolve.SessionNotify).Verify(s => s.NotifyAsync(It.Is<SessionNotification>(n => n.NotificationType == SessionNotificationType.WorkFolderChange && n.FullNames.SequenceEqual(new string[] { fileName }))), Times.Once);

                New<IDataStore>(fileName).Delete();
                Mock.Get(Resolve.SessionNotify).Verify(s => s.NotifyAsync(It.Is<SessionNotification>(n => n.NotificationType == SessionNotificationType.WorkFolderChange && n.FullNames.SequenceEqual(new string[] { fileName }))), Times.Exactly(2));

                IDataStore fileSystemStateInfo = Resolve.WorkFolder.FileInfo.FileItemInfo("FileSystemState.txt");
                fileSystemStateInfo.Delete();
                Mock.Get(Resolve.SessionNotify).Verify(s => s.NotifyAsync(It.Is<SessionNotification>(n => n.NotificationType == SessionNotificationType.WorkFolderChange && n.FullNames.SequenceEqual(new string[] { fileSystemStateInfo.FullName }))), Times.Never);
            }
        }
    }
}