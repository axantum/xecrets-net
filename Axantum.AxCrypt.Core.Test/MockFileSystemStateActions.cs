using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            HandleSessionEventMock = (sessionEvent, progress) => { base.HandleSessionEvent(sessionEvent, progress); };
            HandleSessionEventsMock = (events, progress) => { throw new InvalidOperationException("Unexpected call to this method."); };
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

        public Func<ActiveFile, ProgressContext, ActiveFile> CheckActiveFileMock { get; set; }

        public override ActiveFile CheckActiveFile(ActiveFile activeFile, ProgressContext progress)
        {
            return CheckActiveFileMock(activeFile, progress);
        }

        public Action<ChangedEventMode, ProgressContext> CheckActiveFilesMock { get; set; }

        public override void CheckActiveFiles(ChangedEventMode mode, ProgressContext progress)
        {
            CheckActiveFilesMock(mode, progress);
        }

        public Action<AesKey, ProgressContext> EncryptFilesInWatchedFoldersMock { get; set; }

        public override void EncryptFilesInWatchedFolders(AesKey encryptionKey, ProgressContext progress)
        {
            EncryptFilesInWatchedFoldersMock(encryptionKey, progress);
        }

        public Action<SessionEvent, ProgressContext> HandleSessionEventMock { get; set; }

        public override void HandleSessionEvent(SessionEvent sessionEvent, ProgressContext progress)
        {
            HandleSessionEventMock(sessionEvent, progress);
        }

        public Action<IEnumerable<SessionEvent>, ProgressContext> HandleSessionEventsMock { get; set; }

        public override void HandleSessionEvents(IEnumerable<SessionEvent> events, ProgressContext progress)
        {
            HandleSessionEventsMock(events, progress);
        }

        public Func<IEnumerable<IRuntimeFileInfo>> ListEncryptableInWatchedFoldersMock { get; set; }

        public override IEnumerable<IRuntimeFileInfo> ListEncryptableInWatchedFolders()
        {
            return ListEncryptableInWatchedFoldersMock();
        }

        public Action<ProgressContext> PurgeActiveFilesMock { get; set; }

        public override void PurgeActiveFiles(ProgressContext progress)
        {
            PurgeActiveFilesMock(progress);
        }

        public Action<IEnumerable<string>, ProgressContext> RemoveRecentFilesMock { get; set; }

        public override void RemoveRecentFiles(IEnumerable<string> encryptedPaths, ProgressContext progress)
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