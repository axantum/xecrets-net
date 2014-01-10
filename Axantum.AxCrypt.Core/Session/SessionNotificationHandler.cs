using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.UI;
using System;
using System.Linq;

namespace Axantum.AxCrypt.Core.Session
{
    public class SessionNotificationHandler
    {
        private FileSystemState _fileSystemState;

        private ActiveFileAction _activeFileAction;

        private AxCryptFile _axCryptFile;

        public SessionNotificationHandler(FileSystemState fileSystemState, ActiveFileAction activeFileAction, AxCryptFile axCryptFile)
        {
            _fileSystemState = fileSystemState;
            _activeFileAction = activeFileAction;
            _axCryptFile = axCryptFile;
        }

        public virtual void HandleNotification(SessionNotification notification)
        {
            Instance.ProgressBackground.Work(
                (IProgressContext progress) =>
                {
                    progress.NotifyLevelStart();
                    try
                    {
                        HandleNotificationInternal(notification, progress);
                        _activeFileAction.CheckActiveFiles(ChangedEventMode.RaiseOnlyOnModified, progress);
                    }
                    finally
                    {
                        progress.NotifyLevelFinished();
                    }
                    return FileOperationStatus.Success;
                },
                (FileOperationStatus status) =>
                {
                });
        }

        private void HandleNotificationInternal(SessionNotification notification, IProgressContext progress)
        {
            if (Instance.Log.IsInfoEnabled)
            {
                Instance.Log.LogInfo("Received notification type '{0}'.".InvariantFormat(notification.NotificationType));
            }
            switch (notification.NotificationType)
            {
                case SessionNotificationType.WatchedFolderAdded:
                    IRuntimeFileInfo addedFolderInfo = Factory.New<IRuntimeFileInfo>(notification.FullName);
                    _axCryptFile.EncryptFilesUniqueWithBackupAndWipe(new IRuntimeFileInfo[] { addedFolderInfo }, notification.Key, progress);
                    break;

                case SessionNotificationType.WatchedFolderRemoved:
                    IRuntimeFileInfo removedFolderInfo = Factory.New<IRuntimeFileInfo>(notification.FullName);
                    _axCryptFile.DecryptFilesInFolderUniqueWithWipeOfOriginal(removedFolderInfo, notification.Key, progress);
                    break;

                case SessionNotificationType.LogOn:
                case SessionNotificationType.LogOff:
                    _axCryptFile.EncryptFilesUniqueWithBackupAndWipe(_fileSystemState.WatchedFolders.Select((wf) => Factory.New<IRuntimeFileInfo>(wf.Path)), notification.Key, progress);
                    break;

                case SessionNotificationType.SessionStart:
                    _activeFileAction.CheckActiveFiles(ChangedEventMode.RaiseAlways, progress);
                    break;

                case SessionNotificationType.PurgeActiveFiles:
                    _activeFileAction.PurgeActiveFiles(progress);
                    break;

                case SessionNotificationType.ProcessExit:
                case SessionNotificationType.ActiveFileChange:
                case SessionNotificationType.KnownKeyChange:
                case SessionNotificationType.SessionChange:
                case SessionNotificationType.WorkFolderChange:
                    break;

                default:
                    throw new InvalidOperationException("Unhandled notification recieved");
            }
        }
    }
}