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

using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.UI.ViewModel
{
    public class KnownFoldersViewModel : ViewModelBase
    {
        private FileSystemState _fileSystemState;

        private SessionNotify _sessionNotify;

        private KnownIdentities _knownIdentities;

        public KnownFoldersViewModel(FileSystemState fileSystemState, SessionNotify sessionNotify, KnownIdentities knownIdentities)
        {
            _fileSystemState = fileSystemState;
            _sessionNotify = sessionNotify;
            _knownIdentities = knownIdentities;

            InitializePropertyValues();
            SubscribeToModelEvents();
        }

        private void SubscribeToModelEvents()
        {
            _sessionNotify.AddCommand(HandleKnownFolderAffectingEventsAsync);
        }

        public IAsyncAction UpdateState { get; private set; }

        public IEnumerable<KnownFolder> KnownFolders { get { return GetProperty<IEnumerable<KnownFolder>>(nameof(KnownFolders)); } set { SetProperty(nameof(KnownFolders), value.ToList()); } }

        private void InitializePropertyValues()
        {
            KnownFolders = new KnownFolder[0];
            UpdateState = new AsyncDelegateAction<object>(async (object o) => KnownFolders = await UpdateEnabledStateAsync(KnownFolders));
        }

        private async Task EnsureKnownFoldersWatched(IEnumerable<KnownFolder> folders)
        {
            foreach (KnownFolder knownFolder in folders)
            {
                if (_fileSystemState.WatchedFolders.Any((wf) => wf.Path == knownFolder.My.FullName))
                {
                    continue;
                }
                if (knownFolder.My.IsFile)
                {
                    continue;
                }
                if (!_fileSystemState.KnownCloudFolders.Any(kcf => kcf.Name == knownFolder.DisplayName && kcf.IsMyAxCryptFolderCreated))
                {
                    knownFolder.Folder.CreateFolder(knownFolder.My.Name);
                    await _fileSystemState.AddKnownFolderAsync(new KnownCloudFolder() { Name = knownFolder.DisplayName, IsMyAxCryptFolderCreated = true });
                }

                if (knownFolder.My.IsAvailable)
                {
                    await _fileSystemState.AddWatchedFolderAsync(new WatchedFolder(knownFolder.My.FullName, _knownIdentities.DefaultEncryptionIdentity.Tag));
                }
            }
            await _fileSystemState.Save();
        }

        private async Task<IEnumerable<KnownFolder>> UpdateEnabledStateAsync(IEnumerable<KnownFolder> knownFolders)
        {
            List<KnownFolder> updatedFolders = new List<KnownFolder>();
            bool hasCloudStorageAwareness = New<LicensePolicy>().Capabilities.Has(LicenseCapability.CloudStorageAwareness);
            foreach (KnownFolder folder in knownFolders)
            {
                KnownFolder updated = new KnownFolder(folder, hasCloudStorageAwareness && _knownIdentities.LoggedOnWatchedFolders.Any(f => f.Path == folder.My.FullName));
                updatedFolders.Add(updated);
            }
            return updatedFolders;
        }

        private async Task HandleKnownFolderAffectingEventsAsync(SessionNotification notification)
        {
            switch (notification.NotificationType)
            {
                case SessionNotificationType.LicensePolicyChanged:
                case SessionNotificationType.KnownKeyChange:
                case SessionNotificationType.SignOut:
                    if (notification.Capabilities.Has(LicenseCapability.SecureFolders))
                    {
                        await EnsureKnownFoldersWatched(KnownFolders);
                    }
                    KnownFolders = await UpdateEnabledStateAsync(KnownFolders);
                    break;
            }
        }
    }
}