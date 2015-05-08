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

using Axantum.AxCrypt.Core.Crypto;
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
            Resolve.ProgressBackground.Work(
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
            if (Resolve.Log.IsInfoEnabled)
            {
                Resolve.Log.LogInfo("Received notification type '{0}'.".InvariantFormat(notification.NotificationType));
            }
            EncryptionParameters encryptionParameters;
            switch (notification.NotificationType)
            {
                case SessionNotificationType.WatchedFolderAdded:
                    IDataContainer addedFolderInfo = TypeMap.Resolve.New<IDataContainer>(notification.FullName);
                    encryptionParameters = new EncryptionParameters(Resolve.CryptoFactory.Default.Id, notification.Key.Passphrase);
                    _axCryptFile.EncryptFoldersUniqueWithBackupAndWipe(new IDataContainer[] { addedFolderInfo }, encryptionParameters, progress);
                    break;

                case SessionNotificationType.WatchedFolderRemoved:
                    IDataContainer removedFolderInfo = TypeMap.Resolve.New<IDataContainer>(notification.FullName);
                    if (removedFolderInfo.IsAvailable)
                    {
                        _axCryptFile.DecryptFilesInsideFolderUniqueWithWipeOfOriginal(removedFolderInfo, notification.Key, _statusChecker, progress);
                    }
                    break;

                case SessionNotificationType.LogOn:
                case SessionNotificationType.LogOff:
                    encryptionParameters = new EncryptionParameters(Resolve.CryptoFactory.Default.Id, notification.Key.Passphrase);
                    _axCryptFile.EncryptFoldersUniqueWithBackupAndWipe(_fileSystemState.WatchedFolders.Where(wf => wf.Thumbprint == notification.Key.Passphrase.Thumbprint).Select(wf => TypeMap.Resolve.New<IDataContainer>(wf.Path)), encryptionParameters, progress);
                    break;

                case SessionNotificationType.SessionStart:
                    _activeFileAction.CheckActiveFiles(ChangedEventMode.RaiseAlways, progress);
                    break;

                case SessionNotificationType.EncryptPendingFiles:
                    _activeFileAction.PurgeActiveFiles(progress);
                    Passphrase passphrase = _knownKeys.DefaultEncryptionKey == null ? null : _knownKeys.DefaultEncryptionKey.Passphrase;
                    encryptionParameters = new EncryptionParameters(Resolve.CryptoFactory.Default.Id, passphrase);
                    _axCryptFile.EncryptFoldersUniqueWithBackupAndWipe(_knownKeys.LoggedOnWatchedFolders.Select(wf => TypeMap.Resolve.New<IDataContainer>(wf.Path)), encryptionParameters, progress);
                    break;

                case SessionNotificationType.PurgeActiveFiles:
                    _fileSystemState.PurgeActiveFiles();
                    break;

                case SessionNotificationType.FileMove:
                    _fileSystemState.ChangeActiveFile(notification.OtherFullName, notification.FullName);
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