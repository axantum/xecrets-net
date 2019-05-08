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
using Axantum.AxCrypt.Core.Session;
using AxCrypt.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.UI.ViewModel
{
    public class MainViewModel : ViewModelBase, IDisposable
    {
        private FileSystemState _fileSystemState;

        private UserSettings _userSettings;

        private AxCryptUpdateCheck _axCryptUpdateCheck;

        public bool LoggedOn { get { return GetProperty<bool>(nameof(LoggedOn)); } set { SetProperty(nameof(LoggedOn), value); } }

        public bool EncryptFileEnabled { get { return GetProperty<bool>(nameof(EncryptFileEnabled)); } set { SetProperty(nameof(EncryptFileEnabled), value); } }

        public bool DecryptFileEnabled { get { return GetProperty<bool>(nameof(DecryptFileEnabled)); } set { SetProperty(nameof(DecryptFileEnabled), value); } }

        public bool OpenEncryptedEnabled { get { return GetProperty<bool>(nameof(OpenEncryptedEnabled)); } set { SetProperty(nameof(OpenEncryptedEnabled), value); } }

        public bool RandomRenameEnabled { get { return GetProperty<bool>(nameof(RandomRenameEnabled)); } set { SetProperty(nameof(RandomRenameEnabled), value); } }

        public bool WatchedFoldersEnabled { get { return GetProperty<bool>(nameof(WatchedFoldersEnabled)); } set { SetProperty(nameof(WatchedFoldersEnabled), value); } }

        public EncryptionUpgradeMode EncryptionUpgradeMode { get { return GetProperty<EncryptionUpgradeMode>(nameof(EncryptionUpgradeMode)); } set { SetProperty(nameof(EncryptionUpgradeMode), value); } }

        public IEnumerable<string> WatchedFolders { get { return GetProperty<IEnumerable<string>>(nameof(WatchedFolders)); } set { SetProperty(nameof(WatchedFolders), value.ToList()); } }

        public IEnumerable<ActiveFile> RecentFiles { get { return GetProperty<IEnumerable<ActiveFile>>(nameof(RecentFiles)); } set { SetProperty(nameof(RecentFiles), value.ToList()); } }

        public IEnumerable<ActiveFile> DecryptedFiles { get { return GetProperty<IEnumerable<ActiveFile>>(nameof(DecryptedFiles)); } set { SetProperty(nameof(DecryptedFiles), value.ToList()); } }

        public ActiveFileComparer RecentFilesComparer { get { return GetProperty<ActiveFileComparer>(nameof(RecentFilesComparer)); } set { SetProperty(nameof(RecentFilesComparer), value); } }

        public IEnumerable<string> SelectedWatchedFolders { get { return GetProperty<IEnumerable<string>>(nameof(SelectedWatchedFolders)); } set { SetProperty(nameof(SelectedWatchedFolders), value.ToList()); } }

        public IEnumerable<string> SelectedRecentFiles { get { return GetProperty<IEnumerable<string>>(nameof(SelectedRecentFiles)); } set { SetProperty(nameof(SelectedRecentFiles), value.ToList()); } }

        public IEnumerable<string> DragAndDropFiles { get { return GetProperty<IEnumerable<string>>(nameof(DragAndDropFiles)); } set { SetProperty(nameof(DragAndDropFiles), value.ToList()); } }

        public FileInfoTypes DragAndDropFilesTypes { get { return GetProperty<FileInfoTypes>(nameof(DragAndDropFilesTypes)); } set { SetProperty(nameof(DragAndDropFilesTypes), value); } }

        public bool DroppableAsRecent { get { return GetProperty<bool>(nameof(DroppableAsRecent)); } set { SetProperty(nameof(DroppableAsRecent), value); } }

        public bool DroppableAsWatchedFolder { get { return GetProperty<bool>(nameof(DroppableAsWatchedFolder)); } set { SetProperty(nameof(DroppableAsWatchedFolder), value); } }

        public bool FilesArePending { get { return GetProperty<bool>(nameof(FilesArePending)); } set { SetProperty(nameof(FilesArePending), value); } }

        public DownloadVersion DownloadVersion { get { return GetProperty<DownloadVersion>(nameof(DownloadVersion)); } set { SetProperty(nameof(DownloadVersion), value); } }

        public VersionUpdateStatus VersionUpdateStatus { get { return GetProperty<VersionUpdateStatus>(nameof(VersionUpdateStatus)); } set { SetProperty(nameof(VersionUpdateStatus), value); } }

        public bool DebugMode { get { return GetProperty<bool>(nameof(DebugMode)); } set { SetProperty(nameof(DebugMode), value); } }

        public bool TryBrokenFile { get { return GetProperty<bool>(nameof(TryBrokenFile)); } set { SetProperty(nameof(TryBrokenFile), value); } }

        public FolderOperationMode FolderOperationMode { get { return GetProperty<FolderOperationMode>(nameof(FolderOperationMode)); } set { SetProperty(nameof(FolderOperationMode), value); } }

        public LicenseCapabilities License { get { return GetProperty<LicenseCapabilities>(nameof(License)); } set { SetProperty(nameof(License), value); } }

        public IAsyncAction RemoveRecentFiles { get; private set; }

        public IAsyncAction AddWatchedFolders { get; private set; }

        public IAsyncAction EncryptPendingFiles { get; private set; }

        public IAsyncAction ClearPassphraseMemory { get; private set; }

        public IAsyncAction RemoveWatchedFolders { get; private set; }

        public IAction OpenSelectedFolder { get; private set; }

        public IAsyncAction AxCryptUpdateCheck { get; private set; }

        public IAction LicenseUpdate { get; private set; }

        public MainViewModel(FileSystemState fileSystemState, UserSettings userSettings)
        {
            _fileSystemState = fileSystemState;
            _userSettings = userSettings;

            _axCryptUpdateCheck = New<AxCryptUpdateCheck>();
            _axCryptUpdateCheck.AxCryptUpdate += Handle_VersionUpdate;

            InitializePropertyValues();
            BindPropertyChangedEvents();
            SubscribeToModelEvents();
        }

        private void InitializePropertyValues()
        {
            WatchedFoldersEnabled = false;
            WatchedFolders = new string[0];
            DragAndDropFiles = new string[0];
            RecentFiles = new ActiveFile[0];
            SelectedRecentFiles = new string[0];
            SelectedWatchedFolders = new string[0];
            DecryptedFiles = new ActiveFile[0];
            DebugMode = _userSettings.DebugMode;
            FolderOperationMode = _userSettings.FolderOperationMode;
            DownloadVersion = DownloadVersion.Empty;
            VersionUpdateStatus = DownloadVersion.CalculateStatus(New<IVersion>().Current, New<INow>().Utc, _userSettings.LastUpdateCheckUtc);
            License = New<LicensePolicy>().Capabilities;
            EncryptionUpgradeMode = _userSettings.EncryptionUpgradeMode;
            AddWatchedFolders = new AsyncDelegateAction<IEnumerable<string>>((folders) => AddWatchedFoldersActionAsync(folders), (folders) => Task.FromResult(LoggedOn));
            RemoveRecentFiles = new AsyncDelegateAction<IEnumerable<string>>((files) => RemoveRecentFilesAction(files));
            EncryptPendingFiles = new AsyncDelegateAction<object>((parameter) => EncryptPendingFilesAction());
            ClearPassphraseMemory = new AsyncDelegateAction<object>((parameter) => ClearPassphraseMemoryAction());
            RemoveWatchedFolders = new AsyncDelegateAction<IEnumerable<string>>((folders) => RemoveWatchedFoldersAction(folders), (folders) => Task.FromResult(LoggedOn));
            OpenSelectedFolder = new DelegateAction<string>((folder) => OpenSelectedFolderAction(folder));
            AxCryptUpdateCheck = new AsyncDelegateAction<DateTime>((utc) => AxCryptUpdateCheckAction(utc));
            LicenseUpdate = new DelegateAction<object>((o) => License = New<LicensePolicy>().Capabilities);

            DecryptFileEnabled = true;
            OpenEncryptedEnabled = true;
            RandomRenameEnabled = true;
        }

        private void BindPropertyChangedEvents()
        {
            BindPropertyChangedInternal(nameof(DragAndDropFiles), (IEnumerable<string> files) => { DragAndDropFilesTypes = DetermineFileTypes(files.Select(f => New<IDataItem>(f))); });
            BindPropertyChangedInternal(nameof(DragAndDropFiles), (IEnumerable<string> files) => { DroppableAsRecent = DetermineDroppableAsRecent(files.Select(f => New<IDataItem>(f))); });
            BindPropertyChangedInternal(nameof(DragAndDropFiles), (IEnumerable<string> files) => { DroppableAsWatchedFolder = DetermineDroppableAsWatchedFolder(files.Select(f => New<IDataItem>(f))); });
            BindPropertyChangedInternal(nameof(RecentFilesComparer), (ActiveFileComparer comparer) => { SetRecentFilesComparer(); });
            BindPropertyChangedInternal(nameof(LoggedOn), (bool loggedOn) => LicenseUpdate.Execute(null));
            BindPropertyChangedInternal(nameof(LoggedOn), async (bool loggedOn) => { if (loggedOn) await AxCryptUpdateCheck.ExecuteAsync(_userSettings.LastUpdateCheckUtc); });

            BindPropertyChanged(nameof(DebugMode), (bool enabled) => { UpdateDebugMode(enabled); });
            BindPropertyChanged(nameof(LoggedOn), (bool loggedOn) => EncryptFileEnabled = loggedOn || !License.Has(LicenseCapability.EncryptNewFiles));
            BindPropertyChanged(nameof(License), async (LicenseCapabilities policy) => await SetWatchedFoldersAsync());
            BindPropertyChanged(nameof(EncryptionUpgradeMode), (EncryptionUpgradeMode mode) => Resolve.UserSettings.EncryptionUpgradeMode = mode);
            BindPropertyChanged(nameof(FolderOperationMode), async (FolderOperationMode mode) => await SetFolderOperationMode(mode));
        }

        private void SubscribeToModelEvents()
        {
            Resolve.SessionNotify.AddCommand(HandleSessionChangedAsync);
        }

        public async Task<bool> CanShareAsync(IEnumerable<IDataStore> items)
        {
            if (!items.Any())
            {
                return false;
            }

            if (!LoggedOn)
            {
                return false;
            }

            return true;
        }

        public IEnumerable<ActiveFile> SelectedActiveFiles()
        {
            return SelectedRecentFiles.Select(f => _fileSystemState.FindActiveFileFromEncryptedPath(f)).Where(af => af != null);
        }

        private void UpdateDebugMode(bool enabled)
        {
            Resolve.Log.SetLevel(enabled ? LogLevel.Debug : LogLevel.Error);
            OS.Current.DebugMode(enabled);
            _userSettings.DebugMode = enabled;
        }

        private void Handle_VersionUpdate(object sender, VersionEventArgs e)
        {
            _userSettings.LastUpdateCheckUtc = New<INow>().Utc;
            _userSettings.NewestKnownVersion = e.DownloadVersion.Version.ToString();
            _userSettings.UpdateUrl = e.DownloadVersion.Url;
            _userSettings.UpdateLevel = e.DownloadVersion.Level;

            VersionUpdateStatus = e.DownloadVersion.CalculateStatus(New<IVersion>().Current, New<INow>().Utc, e.LastUpdateCheck);
            DownloadVersion = e.DownloadVersion;
        }

        private static FileInfoTypes DetermineFileTypes(IEnumerable<IDataItem> files)
        {
            FileInfoTypes types = FileInfoTypes.None;
            FileInfoTypes typesToLookFor = FileInfoTypes.EncryptedFile | FileInfoTypes.EncryptableFile;
            foreach (IDataItem file in files)
            {
                types |= file.Type() & typesToLookFor;
                if ((types & typesToLookFor) == typesToLookFor)
                {
                    return types;
                }
            }
            return types;
        }

        private static bool DetermineDroppableAsRecent(IEnumerable<IDataItem> files)
        {
            return files.Any(fileInfo => fileInfo.Type() == FileInfoTypes.EncryptedFile || (Resolve.KnownIdentities.IsLoggedOn && fileInfo.Type() == FileInfoTypes.EncryptableFile));
        }

        private static bool DetermineDroppableAsWatchedFolder(IEnumerable<IDataItem> files)
        {
            if (files.Count() != 1)
            {
                return false;
            }

            IDataItem fileInfo = files.First();
            if (!fileInfo.IsAvailable)
            {
                return false;
            }

            if (!fileInfo.IsFolder)
            {
                return false;
            }

            if (!New<FileFilter>().IsEncryptable(fileInfo))
            {
                return false;
            }

            return true;
        }

        private async Task HandleSessionChangedAsync(SessionNotification notification)
        {
            try
            {
                await HandleSessionChangedInternalAsync(notification);
            }
            catch (Exception ex)
            {
                ex.ReportAndDisplay();
            }
        }

        private async Task HandleSessionChangedInternalAsync(SessionNotification notification)
        {
            switch (notification.NotificationType)
            {
                case SessionNotificationType.WatchedFolderAdded:
                    await SetWatchedFoldersAsync();
                    break;

                case SessionNotificationType.WatchedFolderRemoved:
                    await SetWatchedFoldersAsync();
                    break;

                case SessionNotificationType.SignIn:
                case SessionNotificationType.SignOut:
                    LoggedOn = Resolve.KnownIdentities.IsLoggedOn;
                    break;

                case SessionNotificationType.WatchedFolderChange:
                    FilesArePending = AreFilesPending();
                    break;

                case SessionNotificationType.KnownKeyChange:
                    if (notification.Identity == LogOnIdentity.Empty)
                    {
                        throw new InvalidOperationException("Attempt to add the empty identity as a known key.");
                    }
                    if (!_fileSystemState.KnownPassphrases.Any(p => p.Thumbprint == notification.Identity.Passphrase.Thumbprint))
                    {
                        _fileSystemState.KnownPassphrases.Add(notification.Identity.Passphrase);
                        await _fileSystemState.Save();
                    }
                    break;

                case SessionNotificationType.SessionStart:
                case SessionNotificationType.ActiveFileChange:
                    FilesArePending = AreFilesPending();
                    SetRecentFiles();
                    break;

                case SessionNotificationType.LicensePolicyChanged:
                    LicenseUpdate.Execute(null);
                    break;

                case SessionNotificationType.ProcessExit:
                    if (New<UserSettings>().DisableRecentFiles)
                    {
                        FilesArePending = AreFilesPending();
                    }
                    break;

                case SessionNotificationType.WorkFolderChange:
                case SessionNotificationType.SessionChange:
                default:
                    break;
            }
        }

        private async Task SetWatchedFoldersAsync()
        {
            WatchedFoldersEnabled = License.Has(LicenseCapability.SecureFolders);
            if (!WatchedFoldersEnabled)
            {
                WatchedFolders = new string[0];
                return;
            }
            WatchedFolders = Resolve.KnownIdentities.LoggedOnWatchedFolders.Select(wf => wf.Path).ToList();
        }

        private void SetRecentFiles()
        {
            List<ActiveFile> activeFiles = new List<ActiveFile>(_fileSystemState.ActiveFiles).ToList();
            if (RecentFilesComparer != null)
            {
                activeFiles.Sort(RecentFilesComparer);
            }
            if (activeFiles.IsDisplayEquivalentTo(RecentFiles.ToList()))
            {
                return;
            }
            RecentFiles = activeFiles;
            DecryptedFiles = _fileSystemState.DecryptedActiveFiles;
        }

        private void SetRecentFilesComparer()
        {
            if (RecentFilesComparer == null)
            {
                return;
            }
            List<ActiveFile> recentFiles = RecentFiles.ToList();
            if (recentFiles.Count < 2)
            {
                return;
            }
            recentFiles.Sort(RecentFilesComparer);
            RecentFiles = recentFiles;
        }

        private bool AreFilesPending()
        {
            IList<ActiveFile> openFiles = _fileSystemState.DecryptedActiveFiles;
            if (openFiles.Count > 0)
            {
                return true;
            }

            List<IDataStore> files = new List<IDataStore>();
            foreach (IDataContainer container in Resolve.KnownIdentities.LoggedOnWatchedFolders.Select(wf => New<IDataContainer>(wf.Path)))
            {
                files.AddRange(container.ListOfFiles(_fileSystemState.WatchedFolders.Select(x => New<IDataContainer>(x.Path)), New<UserSettings>().FolderOperationMode.Policy()));
            }
            if (!New<UserSettings>().DoNotShowAgain.HasFlag(DoNotShowAgainOptions.IgnoreFileWarning))
            {
                return files.Where(ds => !ds.IsEncrypted()).Any();
            }

            return files.Where(ds => ds.IsEncryptable()).Any();
        }

        private async Task ClearPassphraseMemoryAction()
        {
            IDataStore fileSystemStateInfo = Resolve.FileSystemState.PathInfo;
            using (FileLock fileSystemStateFileLock = New<FileLocker>().Acquire(fileSystemStateInfo))
            {
                New<AxCryptFile>().Wipe(fileSystemStateFileLock, new ProgressContext());
            }
            TypeMap.Register.Singleton<FileSystemState>(() => FileSystemState.Create(fileSystemStateInfo));
            TypeMap.Register.Singleton<KnownIdentities>(() => new KnownIdentities(_fileSystemState, Resolve.SessionNotify));
            await Resolve.SessionNotify.NotifyAsync(new SessionNotification(SessionNotificationType.SessionStart));
        }

        private static async Task EncryptPendingFilesAction()
        {
            await Resolve.SessionNotify.NotifyAsync(new SessionNotification(SessionNotificationType.EncryptPendingFiles));
        }

        private async Task RemoveRecentFilesAction(IEnumerable<string> files)
        {
            foreach (string file in files)
            {
                ActiveFile activeFile = _fileSystemState.FindActiveFileFromEncryptedPath(file);
                if (activeFile != null)
                {
                    _fileSystemState.RemoveActiveFile(activeFile);
                }
            }
            await _fileSystemState.Save();
        }

        private async Task AddWatchedFoldersActionAsync(IEnumerable<string> folders)
        {
            if (!folders.Any())
            {
                return;
            }
            foreach (string folder in folders)
            {
                if (New<FileFilter>().IsForbiddenFolder(folder))
                {
                    await New<IPopup>().ShowAsync(PopupButtons.Ok, Texts.WarningTitle, Texts.SystemFolderForbiddenText.InvariantFormat(folder));
                    continue;
                }
                await _fileSystemState.AddWatchedFolderAsync(new WatchedFolder(folder, Resolve.KnownIdentities.DefaultEncryptionIdentity.Tag));
            }
            await _fileSystemState.Save();
        }

        private async Task RemoveWatchedFoldersAction(IEnumerable<string> folders)
        {
            if (!folders.Any())
            {
                return;
            }
            foreach (string watchedFolderPath in folders)
            {
                await _fileSystemState.RemoveWatchedFolder(New<IDataContainer>(watchedFolderPath));
            }
            await _fileSystemState.Save();
        }

        private static void OpenSelectedFolderAction(string folder)
        {
            using (ILauncher launcher = New<ILauncher>())
            {
                launcher.Launch(folder);
            }
        }

        private Task AxCryptUpdateCheckAction(DateTime lastUpdateCheckUtc)
        {
            return _axCryptUpdateCheck.CheckInBackgroundAsync(lastUpdateCheckUtc, _userSettings.NewestKnownVersion, _userSettings.UpdateUrl, _userSettings.CultureName);
        }

        private async Task SetFolderOperationMode(FolderOperationMode folderOperationMode)
        {
            _userSettings.FolderOperationMode = folderOperationMode;
            if (folderOperationMode != FolderOperationMode.IncludeSubfolders)
            {
                return;
            }

            await Resolve.SessionNotify.NotifyAsync(new SessionNotification(SessionNotificationType.WatchedFolderOptionsChanged, Resolve.KnownIdentities.DefaultEncryptionIdentity, New<FileSystemState>().WatchedFolders.Select(wf => wf.Path)));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeInternal();
            }
        }

        private void DisposeInternal()
        {
            if (_axCryptUpdateCheck != null)
            {
                Resolve.SessionNotify.RemoveCommand(HandleSessionChangedAsync);

                _axCryptUpdateCheck.AxCryptUpdate -= Handle_VersionUpdate;
                _axCryptUpdateCheck = null;
            }
        }
    }
}