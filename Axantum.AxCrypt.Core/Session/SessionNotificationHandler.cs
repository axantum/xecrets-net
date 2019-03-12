#region Coypright and License

/*
 * AxCrypt - Copyright 2016, Svante Seleborg, All Rights Reserved
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
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Common;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.UI;
using System;
using System.Linq;
using System.Threading.Tasks;
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

        public virtual async Task HandleNotificationAsync(SessionNotification notification)
        {
            await New<IProgressBackground>().WorkAsync($"{nameof(HandleNotificationAsync)} {notification.NotificationType}",
                async (IProgressContext progress) =>
                {
                    progress.NotifyLevelStart();
                    try
                    {
                        await HandleNotificationInternalAsync(notification, progress).Free();
                    }
                    catch (AxCryptException acex)
                    {
                        return new FileOperationContext(acex.DisplayContext, acex.Messages(), ErrorStatus.Exception);
                    }
                    catch (Exception ex)
                    {
                        return new FileOperationContext(string.Empty, ex.Messages(), ErrorStatus.Exception);
                    }
                    finally
                    {
                        progress.NotifyLevelFinished();
                    }
                    return new FileOperationContext(String.Empty, ErrorStatus.Success);
                },
                async (FileOperationContext status) =>
                {
                    if (status.ErrorStatus == ErrorStatus.Success)
                    {
                        return;
                    }
                    _statusChecker.CheckStatusAndShowMessage(status.ErrorStatus, status.FullName, $"{status.InternalMessage} ({notification.NotificationType})");
                },
                new ProgressContext()).Free();
        }

        private async Task HandleNotificationInternalAsync(SessionNotification notification, IProgressContext progress)
        {
            if (Resolve.Log.IsInfoEnabled)
            {
                Resolve.Log.LogInfo("Received notification type '{0}'.".InvariantFormat(notification.NotificationType));
            }
            EncryptionParameters encryptionParameters;
            switch (notification.NotificationType)
            {
                case SessionNotificationType.WatchedFolderAdded:
                case SessionNotificationType.WatchedFolderOptionsChanged:
                    progress.NotifyLevelStart();
                    try
                    {
                        foreach (string fullName in notification.FullNames)
                        {
                            WatchedFolder watchedFolder = _fileSystemState.WatchedFolders.First(wf => wf.Path == fullName);

                            encryptionParameters = new EncryptionParameters(Resolve.CryptoFactory.Default(New<ICryptoPolicy>()).CryptoId, notification.Identity);
                            await encryptionParameters.AddAsync(await watchedFolder.KeyShares.ToKnownPublicKeysAsync(notification.Identity));

                            IDataContainer container = New<IDataContainer>(watchedFolder.Path);
                            progress.Display = container.Name;
                            IDataContainer[] dc = new IDataContainer[] { container };
                            _axCryptFile.ShowIgnoreFileWarning(dc);
                            await _axCryptFile.EncryptFoldersUniqueWithBackupAndWipeAsync(dc, encryptionParameters, progress);
                        }
                    }
                    finally
                    {
                        progress.NotifyLevelFinished();
                    }
                    progress.Totals.ShowNotification();
                    break;

                case SessionNotificationType.WatchedFolderRemoved:
                    foreach (string fullName in notification.FullNames)
                    {
                        IDataContainer removedFolderInfo = New<IDataContainer>(fullName);
                        progress.Display = removedFolderInfo.Name;
                        if (removedFolderInfo.IsAvailable)
                        {
                            await _axCryptFile.DecryptFilesInsideFolderUniqueWithWipeOfOriginalAsync(removedFolderInfo, notification.Identity, _statusChecker, progress).Free();
                        }
                    }
                    break;

                case SessionNotificationType.SignIn:
                    await EncryptWatchedFoldersIfSupportedAsync(notification.Identity, notification.Capabilities, progress);
                    break;

                case SessionNotificationType.SignOut:
                    New<IInternetState>().Clear();
                    New<ICache>().RemoveItem(CacheKey.RootKey);
                    New<UserPublicKeyUpdateStatus>().Clear();
                    break;

                case SessionNotificationType.EncryptPendingFiles:
                    await _activeFileAction.ClearExceptionState();
                    await _activeFileAction.PurgeActiveFiles(progress);
                    if (_knownIdentities.DefaultEncryptionIdentity != LogOnIdentity.Empty)
                    {
                        await EncryptWatchedFoldersIfSupportedAsync(_knownIdentities.DefaultEncryptionIdentity, notification.Capabilities, progress);
                    }
                    break;

                case SessionNotificationType.UpdateActiveFiles:
                    await _fileSystemState.UpdateActiveFiles(notification.FullNames);
                    break;

                case SessionNotificationType.ActiveFileChange:
                case SessionNotificationType.SessionStart:
                case SessionNotificationType.WatchedFolderChange:
                case SessionNotificationType.ProcessExit:
                case SessionNotificationType.KnownKeyChange:
                case SessionNotificationType.SessionChange:
                case SessionNotificationType.WorkFolderChange:
                    await _activeFileAction.CheckActiveFiles(progress);
                    break;

                case SessionNotificationType.LicensePolicyChanged:
                case SessionNotificationType.RefreshLicensePolicy:
                    break;

                default:
                    throw new InvalidOperationException("Unhandled notification received");
            }
        }

        private async Task EncryptWatchedFoldersIfSupportedAsync(LogOnIdentity identity, LicenseCapabilities capabilities, IProgressContext progress)
        {
            if (!capabilities.Has(LicenseCapability.SecureFolders))
            {
                return;
            }
            foreach (WatchedFolder watchedFolder in _fileSystemState.WatchedFolders.Where(wf => wf.Tag.Matches(identity.Tag)))
            {
                EncryptionParameters encryptionParameters = new EncryptionParameters(Resolve.CryptoFactory.Default(New<ICryptoPolicy>()).CryptoId, identity);
                await encryptionParameters.AddAsync(await watchedFolder.KeyShares.ToKnownPublicKeysAsync(identity));
                IDataContainer folder = New<IDataContainer>(watchedFolder.Path);
                progress.Display = folder.Name;
                await _axCryptFile.EncryptFoldersUniqueWithBackupAndWipeAsync(new IDataContainer[] { folder }, encryptionParameters, progress);
            }
        }
    }
}