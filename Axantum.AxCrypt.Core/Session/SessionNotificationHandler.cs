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

        public void HandleNotification(object sender, SessionNotificationArgs e)
        {
            HandleNotification(e.Notification, e.Progress);
        }

        public virtual void HandleNotification(SessionNotification notification, IProgressContext progress)
        {
            if (Instance.Log.IsInfoEnabled)
            {
                Instance.Log.LogInfo("Received notification type '{0}'.".InvariantFormat(notification.NotificationType));
            }
            switch (notification.NotificationType)
            {
                case SessionNotificationType.ActiveFileChange:
                    _activeFileAction.CheckActiveFiles(ChangedEventMode.RaiseOnlyOnModified, progress);
                    break;

                case SessionNotificationType.WatchedFolderAdded:
                    IRuntimeFileInfo addedFolderInfo = OS.Current.FileInfo(notification.FullName);
                    _axCryptFile.EncryptFilesUniqueWithBackupAndWipe(new IRuntimeFileInfo[] { addedFolderInfo }, notification.Key, progress);
                    break;

                case SessionNotificationType.WatchedFolderRemoved:
                    IRuntimeFileInfo removedFolderInfo = OS.Current.FileInfo(notification.FullName);
                    _axCryptFile.DecryptFilesUniqueWithWipeOfOriginal(removedFolderInfo, notification.Key, progress);
                    break;

                case SessionNotificationType.LogOn:
                case SessionNotificationType.LogOff:
                    _axCryptFile.EncryptFilesUniqueWithBackupAndWipe(_fileSystemState.WatchedFolders.Select((wf) => OS.Current.FileInfo(wf.Path)), notification.Key, progress);
                    break;

                case SessionNotificationType.SessionStart:
                    _activeFileAction.CheckActiveFiles(ChangedEventMode.RaiseAlways, progress);
                    break;

                case SessionNotificationType.PurgeActiveFiles:
                    _activeFileAction.PurgeActiveFiles(progress);
                    break;

                case SessionNotificationType.KnownKeyChange:
                case SessionNotificationType.ProcessExit:
                case SessionNotificationType.SessionChange:
                case SessionNotificationType.WorkFolderChange:
                    break;

                default:
                    throw new InvalidOperationException("Unhandled notification recieved");
            }
        }
    }
}