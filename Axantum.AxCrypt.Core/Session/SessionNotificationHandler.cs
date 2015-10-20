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

using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.UI;
using System;
using System.Linq;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.Session
{
    public class SessionNotificationHandler
    {
        private FileSystemState _fileSystemState;

        private KnownIdentities _knownIdentities;

        private ActiveFileAction _activeFileAction;

        private AxCryptFile _axCryptFile;

        private IStatusChecker _statusChecker;

        public SessionNotificationHandler(FileSystemState fileSystemState, KnownIdentities knownIdentities, ActiveFileAction activeFileAction, AxCryptFile axCryptFile, IStatusChecker statusChecker)
        {
            _fileSystemState = fileSystemState;
            _knownIdentities = knownIdentities;
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
                    return new FileOperationContext(String.Empty, ErrorStatus.Success);
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
                    foreach (string fullName in notification.FullNames)
                    {
                        IDataContainer addedFolderInfo = New<IDataContainer>(fullName);
                        encryptionParameters = new EncryptionParameters(Resolve.CryptoFactory.Default.Id, notification.Identity);
                        _axCryptFile.EncryptFoldersUniqueWithBackupAndWipe(new IDataContainer[] { addedFolderInfo }, encryptionParameters, progress);
                    }
                    break;

                case SessionNotificationType.WatchedFolderRemoved:
                    foreach (string fullName in notification.FullNames)
                    {
                        IDataContainer removedFolderInfo = New<IDataContainer>(fullName);
                        if (removedFolderInfo.IsAvailable)
                        {
                            _axCryptFile.DecryptFilesInsideFolderUniqueWithWipeOfOriginal(removedFolderInfo, notification.Identity, _statusChecker, progress);
                        }
                    }
                    break;

                case SessionNotificationType.LogOn:
                case SessionNotificationType.LogOff:
                    encryptionParameters = new EncryptionParameters(Resolve.CryptoFactory.Default.Id, notification.Identity);
                    _axCryptFile.EncryptFoldersUniqueWithBackupAndWipe(_fileSystemState.WatchedFolders.Where(wf => wf.Tag.Matches(notification.Identity.Tag)).Select(wf => New<IDataContainer>(wf.Path)), encryptionParameters, progress);
                    break;

                case SessionNotificationType.SessionStart:
                    _activeFileAction.CheckActiveFiles(ChangedEventMode.RaiseAlways, progress);
                    break;

                case SessionNotificationType.EncryptPendingFiles:
                    _activeFileAction.PurgeActiveFiles(progress);
                    if (_knownIdentities.DefaultEncryptionIdentity != LogOnIdentity.Empty)
                    {
                        encryptionParameters = new EncryptionParameters(Resolve.CryptoFactory.Default.Id, _knownIdentities.DefaultEncryptionIdentity);
                        _axCryptFile.EncryptFoldersUniqueWithBackupAndWipe(_knownIdentities.LoggedOnWatchedFolders.Select(wf => New<IDataContainer>(wf.Path)), encryptionParameters, progress);
                    }
                    break;

                case SessionNotificationType.UpdateActiveFiles:
                    _fileSystemState.UpdateActiveFiles(notification.FullNames);
                    break;

                case SessionNotificationType.WatchedFolderChange:
                case SessionNotificationType.ProcessExit:
                case SessionNotificationType.ActiveFileChange:
                case SessionNotificationType.KnownKeyChange:
                case SessionNotificationType.SessionChange:
                case SessionNotificationType.WorkFolderChange:
                    break;

                default:
                    throw new InvalidOperationException("Unhandled notification received");
            }
        }
    }
}