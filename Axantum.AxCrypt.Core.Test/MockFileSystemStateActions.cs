#region Coypright and License

/*
 * AxCrypt - Copyright 2013, Svante Seleborg, All Rights Reserved
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

using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Axantum.AxCrypt.Core.Test
{
    internal class MockFileSystemStateActions : FileSystemStateActions
    {
        public MockFileSystemStateActions(FileSystemState fileSystemState)
            : base(fileSystemState)
        {
            CheckActiveFileMock = (activeFile, progress) => { throw new InvalidOperationException("Unexpected call to this method."); };
            CheckActiveFilesMock = (mode, progress) => { throw new InvalidOperationException("Unexpected call to this method."); };
            EncryptFilesInWatchedFoldersMock = (encryptionKey, progress) => { throw new InvalidOperationException("Unexpected call to this method."); };
            HandleSessionEventMock = (sessionEvent, progress) => { base.HandleNotification(sessionEvent, progress); };
            ListEncryptableInWatchedFoldersMock = () => { throw new InvalidOperationException("Unexpected call to this method."); };
            PurgeActiveFilesMock = (progress) => { throw new InvalidOperationException("Unexpected call to this method."); };
            RemoveRecentFilesMock = (encryptedPaths, progress) => { throw new InvalidOperationException("Unexpected call to this method."); };
            TryFindDecryptionKeyMock = DefaultTryFindDecryptionKeyMock;
            UpdateActiveFileWithKeyIfKeyMatchesThumbprintMock = (key) => { throw new InvalidOperationException("Unexpected call to this method."); };
        }

        private bool DefaultTryFindDecryptionKeyMock(string fullname, out AesKey key)
        {
            throw new InvalidOperationException("Unexpected call to this method.");
        }

        public Func<ActiveFile, IProgressContext, ActiveFile> CheckActiveFileMock { get; set; }

        public override ActiveFile CheckActiveFile(ActiveFile activeFile, IProgressContext progress)
        {
            return CheckActiveFileMock(activeFile, progress);
        }

        public Action<ChangedEventMode, IProgressContext> CheckActiveFilesMock { get; set; }

        public override void CheckActiveFiles(ChangedEventMode mode, IProgressContext progress)
        {
            CheckActiveFilesMock(mode, progress);
        }

        public Action<AesKey, IProgressContext> EncryptFilesInWatchedFoldersMock { get; set; }

        public override void EncryptFilesInWatchedFolders(AesKey encryptionKey, IProgressContext progress)
        {
            EncryptFilesInWatchedFoldersMock(encryptionKey, progress);
        }

        public Action<SessionNotification, IProgressContext> HandleSessionEventMock { get; set; }

        public override void HandleNotification(SessionNotification sessionEvent, IProgressContext progress)
        {
            HandleSessionEventMock(sessionEvent, progress);
        }

        public Func<IEnumerable<IRuntimeFileInfo>> ListEncryptableInWatchedFoldersMock { get; set; }

        public override IEnumerable<IRuntimeFileInfo> ListEncryptableInWatchedFolders()
        {
            return ListEncryptableInWatchedFoldersMock();
        }

        public Action<IProgressContext> PurgeActiveFilesMock { get; set; }

        public override void PurgeActiveFiles(IProgressContext progress)
        {
            PurgeActiveFilesMock(progress);
        }

        public Action<IEnumerable<IRuntimeFileInfo>, IProgressContext> RemoveRecentFilesMock { get; set; }

        public override void RemoveRecentFiles(IEnumerable<IRuntimeFileInfo> encryptedPaths, IProgressContext progress)
        {
            RemoveRecentFilesMock(encryptedPaths, progress);
        }

        public delegate TResult OutFunc<T1, T2, TResult>(T1 arg1, out T2 arg2);

        public OutFunc<string, AesKey, bool> TryFindDecryptionKeyMock { get; set; }

        public override bool TryFindDecryptionKey(string fullName, out AesKey key)
        {
            return TryFindDecryptionKeyMock(fullName, out key);
        }

        public Func<AesKey, bool> UpdateActiveFileWithKeyIfKeyMatchesThumbprintMock { get; set; }

        public override bool UpdateActiveFileWithKeyIfKeyMatchesThumbprint(AesKey key)
        {
            return UpdateActiveFileWithKeyIfKeyMatchesThumbprintMock(key);
        }
    }
}