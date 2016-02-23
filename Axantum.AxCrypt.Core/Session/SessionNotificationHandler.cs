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
using Axantum.AxCrypt.Common;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Crypto.Asymmetric;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
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
                    progress.NotifyLevelStart();
                    try
                    {
                        foreach (string fullName in notification.FullNames)
                        {
                            WatchedFolder watchedFolder = _fileSystemState.WatchedFolders.First(wf => wf.Path == fullName);
                            encryptionParameters = WatchedFolderEncryptionParameters(watchedFolder, notification.Identity);
                            IDataContainer[] dc = new IDataContainer[] { New<IDataContainer>(watchedFolder.Path) };
                            _axCryptFile.EncryptFoldersUniqueWithBackupAndWipe(dc, encryptionParameters, progress);
                            _axCryptFile.ChangeEncryption(dc, notification.Identity, encryptionParameters, progress);
                        }
                    }
                    finally
                    {
                        progress.NotifyLevelFinished();
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
                    EncryptWatchedFolders(notification.Identity, progress);
                    break;

                case SessionNotificationType.LogOff:
                    EncryptWatchedFolders(notification.Identity, progress);
                    New<ICache>().RemoveItem(CacheKey.RootKey);
                    break;

                case SessionNotificationType.SessionStart:
                    _activeFileAction.CheckActiveFiles(ChangedEventMode.RaiseAlways, progress);
                    break;

                case SessionNotificationType.EncryptPendingFiles:
                    _activeFileAction.PurgeActiveFiles(progress);
                    if (_knownIdentities.DefaultEncryptionIdentity != LogOnIdentity.Empty)
                    {
                        EncryptWatchedFolders(_knownIdentities.DefaultEncryptionIdentity, progress);
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

        private void EncryptWatchedFolders(LogOnIdentity identity, IProgressContext progress)
        {
            foreach (WatchedFolder watchedFolder in _fileSystemState.WatchedFolders.Where(wf => wf.Tag.Matches(identity.Tag)))
            {
                EncryptionParameters encryptionParameters = WatchedFolderEncryptionParameters(watchedFolder, identity);
                _axCryptFile.EncryptFoldersUniqueWithBackupAndWipe(new IDataContainer[] { New<IDataContainer>(watchedFolder.Path) }, encryptionParameters, progress);
            }
        }

        private static EncryptionParameters WatchedFolderEncryptionParameters(WatchedFolder watchedFolder, LogOnIdentity identity)
        {
            EncryptionParameters encryptionParameters = new EncryptionParameters(Resolve.CryptoFactory.Default(New<LogOnIdentity, ICryptoPolicy>(identity)).Id, identity);

            IEnumerable<EmailAddress> keySharesEmails = watchedFolder.KeyShares;
            using (KnownPublicKeys knownPublicKeys = New<KnownPublicKeys>())
            {
                IEnumerable<UserPublicKey> keyShares = knownPublicKeys.PublicKeys.Where(pk => keySharesEmails.Contains(pk.Email));
                encryptionParameters.Add(keyShares);
            }

            return encryptionParameters;
        }
    }
}