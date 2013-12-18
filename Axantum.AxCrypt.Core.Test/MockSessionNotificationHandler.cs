using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.UI;
using System;
using System.Linq;

namespace Axantum.AxCrypt.Core.Test
{
    internal class MockSessionNotificationHandler : SessionNotificationHandler
    {
        public MockSessionNotificationHandler(FileSystemState fileSystemState, ActiveFileAction activeFileAction, AxCryptFile axCryptFile)
            : base(fileSystemState, activeFileAction, axCryptFile)
        {
            HandleSessionEventMock = (sessionEvent, progress) => { base.HandleNotification(sessionEvent, progress); };
        }

        public Action<SessionNotification, IProgressContext> HandleSessionEventMock { get; set; }

        public override void HandleNotification(SessionNotification sessionEvent, IProgressContext progress)
        {
            HandleSessionEventMock(sessionEvent, progress);
        }
    }
}