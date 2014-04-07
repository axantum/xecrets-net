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

using System;
using System.Linq;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.UI;

namespace Axantum.AxCrypt.Core.Session
{
    public class SessionNotificationHandler
    {
        private FileSystemState _fileSystemState;

        private KnownKeys _knownKeys;

        private ActiveFileAction _activeFileAction;

        private AxCryptFile _axCryptFile;

        private IStatusChecker _statusChecker;

        public SessionNotificationHandler(FileSystemState fileSystemState, KnownKeys knownKeys, ActiveFileAction activeFileAction, AxCryptFile axCryptFile, IStatusChecker statusChecker)
        {
            _fileSystemState = fileSystemState;
            _knownKeys = knownKeys;
            _activeFileAction = activeFileAction;
            _axCryptFile = axCryptFile;
            _statusChecker = statusChecker;
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
                    return new FileOperationContext(String.Empty, FileOperationStatus.Success);
                },
                (FileOperationContext status) =>
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
                    _axCryptFile.EncryptFoldersUniqueWithBackupAndWipe(new IRuntimeFileInfo[] { addedFolderInfo }, notification.Key, progress);
                    break;

                case SessionNotificationType.WatchedFolderRemoved:
                    IRuntimeFileInfo removedFolderInfo = Factory.New<IRuntimeFileInfo>(notification.FullName);
                    if (removedFolderInfo.IsExistingFolder)
                    {
                        _axCryptFile.DecryptFilesInsideFolderUniqueWithWipeOfOriginal(removedFolderInfo, notification.Key, _statusChecker, progress);
                    }
                    break;

                case SessionNotificationType.LogOn:
                case SessionNotificationType.LogOff:
                    _axCryptFile.EncryptFoldersUniqueWithBackupAndWipe(_fileSystemState.WatchedFolders.Where(wf => wf.Thumbprint == notification.Key.Thumbprint).Select(wf => Factory.New<IRuntimeFileInfo>(wf.Path)), notification.Key, progress);
                    break;

                case SessionNotificationType.SessionStart:
                    _activeFileAction.CheckActiveFiles(ChangedEventMode.RaiseAlways, progress);
                    break;

                case SessionNotificationType.EncryptPendingFiles:
                    _activeFileAction.PurgeActiveFiles(progress);
                    _axCryptFile.EncryptFoldersUniqueWithBackupAndWipe(_knownKeys.LoggedOnWatchedFolders.Select(wf => Factory.New<IRuntimeFileInfo>(wf.Path)), _knownKeys.DefaultEncryptionKey, progress);
                    break;

                case SessionNotificationType.WatchedFolderChange:
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