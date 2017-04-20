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

        public LegacyConversionMode LegacyConversionMode { get { return GetProperty<LegacyConversionMode>(nameof(LegacyConversionMode)); } set { SetProperty(nameof(LegacyConversionMode), value); } }

        public string Title { get { return GetProperty<string>(nameof(Title)); } set { SetProperty(nameof(Title), value); } }

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

        public LicensePolicy License { get { return GetProperty<LicensePolicy>(nameof(License)); } set { SetProperty(nameof(License), value); } }

        public IAction RemoveRecentFiles { get; private set; }

        public IAction AddWatchedFolders { get; private set; }

        public IAction EncryptPendingFiles { get; private set; }

        public IAction ClearPassphraseMemory { get; private set; }

        public IAction RemoveWatchedFolders { get; private set; }

        public IAction OpenSelectedFolder { get; private set; }

        public IAction AxCryptUpdateCheck { get; private set; }

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
            DebugMode = _userSettings.DebugMode;
            Title = String.Empty;
            DownloadVersion = DownloadVersion.Empty;
            VersionUpdateStatus = DownloadVersion.CalculateStatus(New<IVersion>().Current, New<INow>().Utc, _userSettings.LastUpdateCheckUtc);
            License = New<LicensePolicy>();
            LegacyConversionMode = Resolve.UserSettings.LegacyConversionMode;

            AddWatchedFolders = new DelegateAction<IEnumerable<string>>((folders) => AddWatchedFoldersAction(folders), (folders) => LoggedOn);
            RemoveRecentFiles = new DelegateAction<IEnumerable<string>>((files) => RemoveRecentFilesAction(files));
            EncryptPendingFiles = new DelegateAction<object>((parameter) => EncryptPendingFilesAction());
            ClearPassphraseMemory = new DelegateAction<object>((parameter) => ClearPassphraseMemoryAction());
            RemoveWatchedFolders = new DelegateAction<IEnumerable<string>>((folders) => RemoveWatchedFoldersAction(folders), (folders) => LoggedOn);
            OpenSelectedFolder = new DelegateAction<string>((folder) => OpenSelectedFolderAction(folder));
            AxCryptUpdateCheck = new DelegateAction<DateTime>((utc) => AxCryptUpdateCheckAction(utc));
            LicenseUpdate = new DelegateAction<object>((o) => License = New<LicensePolicy>());

            DecryptFileEnabled = true;
            OpenEncryptedEnabled = true;
            RandomRenameEnabled = true;
        }

        private void BindPropertyChangedEvents()
        {
            BindPropertyChangedInternal(nameof(DragAndDropFiles), (IEnumerable<string> files) => { DragAndDropFilesTypes = DetermineFileTypes(files.Select(f => New<IDataItem>(f))); });
            BindPropertyChangedInternal(nameof(DragAndDropFiles), (IEnumerable<string> files) => { DroppableAsRecent = DetermineDroppableAsRecent(files.Select(f => New<IDataItem>(f))); });
            BindPropertyChangedInternal(nameof(DragAndDropFiles), (IEnumerable<string> files) => { DroppableAsWatchedFolder = DetermineDroppableAsWatchedFolder(files.Select(f => New<IDataItem>(f))); });
            BindPropertyChangedInternal(nameof(DebugMode), (bool enabled) => { UpdateDebugMode(enabled); });
            BindPropertyChangedInternal(nameof(RecentFilesComparer), (ActiveFileComparer comparer) => { SetRecentFilesComparer(); });
            BindPropertyChangedInternal(nameof(LoggedOn), (bool loggedOn) => LicenseUpdate.Execute(null));
            BindPropertyChangedInternal(nameof(LoggedOn), (bool loggedOn) => { if (loggedOn) AxCryptUpdateCheck.Execute(_userSettings.LastUpdateCheckUtc); });
            BindPropertyChangedInternal(nameof(License), async (LicensePolicy policy) => await SetWatchedFoldersAsync());
            BindPropertyChangedInternal(nameof(LegacyConversionMode), (LegacyConversionMode mode) => Resolve.UserSettings.LegacyConversionMode = mode);
        }

        private void SubscribeToModelEvents()
        {
            Resolve.SessionNotify.Notification += HandleSessionChangedAsync;
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

            if (!await License.HasAsync(LicenseCapability.KeySharing))
            {
                return false;
            }

            return true;
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

            if (!fileInfo.IsEncryptable())
            {
                return false;
            }

            return true;
        }

        private async void HandleSessionChangedAsync(object sender, SessionNotificationEventArgs e)
        {
            switch (e.Notification.NotificationType)
            {
                case SessionNotificationType.WatchedFolderAdded:
                    await SetWatchedFoldersAsync();
                    break;

                case SessionNotificationType.WatchedFolderRemoved:
                    await SetWatchedFoldersAsync();
                    break;

                case SessionNotificationType.LogOn:
                    SetLogOnState(Resolve.KnownIdentities.IsLoggedOn);
                    break;

                case SessionNotificationType.LogOff:
                    SetLogOnState(Resolve.KnownIdentities.IsLoggedOn);
                    break;

                case SessionNotificationType.WatchedFolderChange:
                    SetFilesArePending();
                    break;

                case SessionNotificationType.KnownKeyChange:
                    if (e.Notification.Identity == LogOnIdentity.Empty)
                    {
                        throw new InvalidOperationException("Attempt to add the empty identity as a known key.");
                    }
                    if (!_fileSystemState.KnownPassphrases.Any(p => p.Thumbprint == e.Notification.Identity.Passphrase.Thumbprint))
                    {
                        _fileSystemState.KnownPassphrases.Add(e.Notification.Identity.Passphrase);
                        _fileSystemState.Save();
                    }
                    break;

                case SessionNotificationType.SessionStart:
                case SessionNotificationType.ActiveFileChange:
                    SetFilesArePending();
                    SetRecentFiles();
                    break;

                case SessionNotificationType.LicensePolicyChange:
                    LicenseUpdate.Execute(null);
                    break;

                case SessionNotificationType.WorkFolderChange:
                case SessionNotificationType.ProcessExit:
                case SessionNotificationType.SessionChange:
                default:
                    break;
            }
        }

        private async Task SetWatchedFoldersAsync()
        {
            WatchedFoldersEnabled = await New<LicensePolicy>().HasAsync(LicenseCapability.SecureFolders);
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

        private void SetFilesArePending()
        {
            IList<ActiveFile> openFiles = _fileSystemState.DecryptedActiveFiles;
            FilesArePending = openFiles.Count > 0 || Resolve.KnownIdentities.LoggedOnWatchedFolders.SelectMany(wf => New<IDataContainer>(wf.Path).ListEncryptable()).Any();
        }

        private void SetLogOnState(bool isLoggedOn)
        {
            LoggedOn = isLoggedOn;
            EncryptFileEnabled = isLoggedOn;
        }

        private void ClearPassphraseMemoryAction()
        {
            IDataStore fileSystemStateInfo = Resolve.FileSystemState.PathInfo;
            New<AxCryptFile>().Wipe(fileSystemStateInfo, new ProgressContext());
            TypeMap.Register.Singleton<FileSystemState>(() => FileSystemState.Create(fileSystemStateInfo));
            TypeMap.Register.Singleton<KnownIdentities>(() => new KnownIdentities(_fileSystemState, Resolve.SessionNotify));
            Resolve.SessionNotify.Notify(new SessionNotification(SessionNotificationType.SessionStart));
        }

        private static void EncryptPendingFilesAction()
        {
            Resolve.SessionNotify.Notify(new SessionNotification(SessionNotificationType.EncryptPendingFiles));
        }

        private void RemoveRecentFilesAction(IEnumerable<string> files)
        {
            foreach (string file in files)
            {
                ActiveFile activeFile = _fileSystemState.FindActiveFileFromEncryptedPath(file);
                if (activeFile != null)
                {
                    _fileSystemState.RemoveActiveFile(activeFile);
                }
            }
            _fileSystemState.Save();
        }

        private void AddWatchedFoldersAction(IEnumerable<string> folders)
        {
            if (!folders.Any())
            {
                return;
            }
            foreach (string folder in folders)
            {
                _fileSystemState.AddWatchedFolder(new WatchedFolder(folder, Resolve.KnownIdentities.DefaultEncryptionIdentity.Tag));
            }
            _fileSystemState.Save();
        }

        private void RemoveWatchedFoldersAction(IEnumerable<string> folders)
        {
            if (!folders.Any())
            {
                return;
            }
            foreach (string watchedFolderPath in folders)
            {
                _fileSystemState.RemoveWatchedFolder(New<IDataContainer>(watchedFolderPath));
            }
            _fileSystemState.Save();
        }

        private static void OpenSelectedFolderAction(string folder)
        {
            using (ILauncher launcher = New<ILauncher>())
            {
                launcher.Launch(folder);
            }
        }

        private void AxCryptUpdateCheckAction(DateTime lastUpdateCheckUtc)
        {
            _axCryptUpdateCheck.CheckInBackground(lastUpdateCheckUtc, _userSettings.NewestKnownVersion, _userSettings.UpdateUrl, _userSettings.CultureName);
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
                Resolve.SessionNotify.Notification -= HandleSessionChangedAsync;

                _axCryptUpdateCheck.AxCryptUpdate -= Handle_VersionUpdate;
                _axCryptUpdateCheck.Dispose();
                _axCryptUpdateCheck = null;
            }
        }
    }
}