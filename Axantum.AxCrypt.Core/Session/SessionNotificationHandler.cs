#region Coypright and License

/*
 * AxCrypt - Copyright 2014, Svante Seleborg, All Rights Reserved
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
                    if (removedFolderInfo.IsFolder)
                    {
                        _axCryptFile.DecryptFilesInsideFolderUniqueWithWipeOfOriginal(removedFolderInfo, notification.Key, progress);
                    }
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