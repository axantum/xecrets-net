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
 * The source is maintained at http://bitbucket.org/axantum/axcrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Core.Test
{
    internal class MockFileSystemStateActions : ActiveFileAction
    {
        public MockFileSystemStateActions()
            : base()
        {
            CheckActiveFileMock = (activeFile, progress) => { throw new InvalidOperationException("Unexpected call to this method."); };
            CheckActiveFilesMock = (progress) => Constant.CompletedTask;
            PurgeActiveFilesMock = (progress) => { throw new InvalidOperationException("Unexpected call to this method."); };
            RemoveRecentFilesMock = (encryptedPaths, progress) => { throw new InvalidOperationException("Unexpected call to this method."); };
            UpdateActiveFileWithKeyIfKeyMatchesThumbprintMock = (key) => { throw new InvalidOperationException("Unexpected call to this method."); };
        }

        public Func<ActiveFile, IProgressContext, ActiveFile> CheckActiveFileMock { get; set; }

        public override ActiveFile CheckActiveFile(ActiveFile activeFile, IProgressContext progress)
        {
            return CheckActiveFileMock(activeFile, progress);
        }

        public Func<IProgressContext, Task> CheckActiveFilesMock { get; set; }

        public override Task CheckActiveFiles(IProgressContext progress)
        {
            return CheckActiveFilesMock(progress);
        }

        public Func<IProgressContext, Task> PurgeActiveFilesMock { get; set; }

        public override Task PurgeActiveFiles(IProgressContext progress)
        {
            return PurgeActiveFilesMock(progress);
        }

        public Func<IEnumerable<IDataStore>, IProgressContext, Task> RemoveRecentFilesMock { get; set; }

        public override Task RemoveRecentFiles(IEnumerable<IDataStore> encryptedPaths, IProgressContext progress)
        {
            return RemoveRecentFilesMock(encryptedPaths, progress);
        }

        public Func<LogOnIdentity, Task<bool>> UpdateActiveFileWithKeyIfKeyMatchesThumbprintMock { get; set; }

        public override Task<bool> UpdateActiveFileWithKeyIfKeyMatchesThumbprint(LogOnIdentity key)
        {
            return UpdateActiveFileWithKeyIfKeyMatchesThumbprintMock(key);
        }
    }
}