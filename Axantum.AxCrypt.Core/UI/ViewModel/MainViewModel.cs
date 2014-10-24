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
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Axantum.AxCrypt.Core.UI.ViewModel
{
    public class MainViewModel : ViewModelBase, IDisposable
    {
        private FileSystemState _fileSystemState;

        private UpdateCheck _updateCheck;

        public bool LoggedOn { get { return GetProperty<bool>("LoggedOn"); } set { SetProperty("LoggedOn", value); } }

        public bool EncryptFileEnabled { get { return GetProperty<bool>("EncryptFileEnabled"); } set { SetProperty("EncryptFileEnabled", value); } }

        public bool DecryptFileEnabled { get { return GetProperty<bool>("DecryptFileEnabled"); } set { SetProperty("DecryptFileEnabled", value); } }

        public bool OpenEncryptedEnabled { get { return GetProperty<bool>("OpenEncryptedEnabled"); } set { SetProperty("OpenEncryptedEnabled", value); } }

        public bool WatchedFoldersEnabled { get { return GetProperty<bool>("WatchedFoldersEnabled"); } set { SetProperty("WatchedFoldersEnabled", value); } }

        public string Title { get { return GetProperty<string>("Title"); } set { SetProperty("Title", value); } }

        public PassphraseIdentity Identity { get { return GetProperty<PassphraseIdentity>("Identity"); } set { SetProperty("Identity", value); } }

        public IEnumerable<string> WatchedFolders { get { return GetProperty<IEnumerable<string>>("WatchedFolders"); } set { SetProperty("WatchedFolders", value.ToList()); } }

        public IEnumerable<ActiveFile> RecentFiles { get { return GetProperty<IEnumerable<ActiveFile>>("RecentFiles"); } set { SetProperty("RecentFiles", value.ToList()); } }

        public IEnumerable<ActiveFile> DecryptedFiles { get { return GetProperty<IEnumerable<ActiveFile>>("DecryptedFiles"); } set { SetProperty("DecryptedFiles", value.ToList()); } }

        public ActiveFileComparer RecentFilesComparer { get { return GetProperty<ActiveFileComparer>("RecentFilesComparer"); } set { SetProperty("RecentFilesComparer", value); } }

        public IEnumerable<string> SelectedWatchedFolders { get { return GetProperty<IEnumerable<string>>("SelectedWatchedFolders"); } set { SetProperty("SelectedWatchedFolders", value.ToList()); } }

        public IEnumerable<string> SelectedRecentFiles { get { return GetProperty<IEnumerable<string>>("SelectedRecentFiles"); } set { SetProperty("SelectedRecentFiles", value.ToList()); } }

        public IEnumerable<string> DragAndDropFiles { get { return GetProperty<IEnumerable<string>>("DragAndDropFiles"); } set { SetProperty("DragAndDropFiles", value.ToList()); } }

        public FileInfoTypes DragAndDropFilesTypes { get { return GetProperty<FileInfoTypes>("DragAndDropFilesTypes"); } set { SetProperty("DragAndDropFilesTypes", value); } }

        public bool DroppableAsRecent { get { return GetProperty<bool>("DroppableAsRecent"); } set { SetProperty("DroppableAsRecent", value); } }

        public bool DroppableAsWatchedFolder { get { return GetProperty<bool>("DroppableAsWatchedFolder"); } set { SetProperty("DroppableAsWatchedFolder", value); } }

        public bool FilesArePending { get { return GetProperty<bool>("FilesArePending"); } set { SetProperty("FilesArePending", value); } }

        public Version CurrentVersion { get { return GetProperty<Version>("CurrentVersion"); } set { SetProperty("CurrentVersion", value); } }

        public Version UpdatedVersion { get { return GetProperty<Version>("UpdatedVersion"); } set { SetProperty("UpdatedVersion", value); } }

        public VersionUpdateStatus VersionUpdateStatus { get { return GetProperty<VersionUpdateStatus>("VersionUpdateStatus"); } set { SetProperty("VersionUpdateStatus", value); } }

        public bool DebugMode { get { return GetProperty<bool>("DebugMode"); } set { SetProperty("DebugMode", value); } }

        public bool Working { get { return GetProperty<bool>("Working"); } set { SetProperty("Working", value); } }

        public IAction RemoveRecentFiles { get; private set; }

        public IAction AddWatchedFolders { get; private set; }

        public IAction EncryptPendingFiles { get; private set; }

        public IAction ClearPassphraseMemory { get; private set; }

        public IAction RemoveWatchedFolders { get; private set; }

        public IAction OpenSelectedFolder { get; private set; }

        public IAction UpdateCheck { get; private set; }

        public MainViewModel(FileSystemState fileSystemState)
        {
            _fileSystemState = fileSystemState;

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
            DebugMode = false;
            Title = String.Empty;
            VersionUpdateStatus = UI.VersionUpdateStatus.Unknown;

            AddWatchedFolders = new DelegateAction<IEnumerable<string>>((folders) => AddWatchedFoldersAction(folders), (folders) => LoggedOn);
            RemoveRecentFiles = new DelegateAction<IEnumerable<string>>((files) => RemoveRecentFilesAction(files));
            EncryptPendingFiles = new DelegateAction<object>((parameter) => EncryptPendingFilesAction());
            ClearPassphraseMemory = new DelegateAction<object>((parameter) => ClearPassphraseMemoryAction());
            RemoveWatchedFolders = new DelegateAction<IEnumerable<string>>((folders) => RemoveWatchedFoldersAction(folders), (folders) => LoggedOn);
            OpenSelectedFolder = new DelegateAction<string>((folder) => OpenSelectedFolderAction(folder));
            UpdateCheck = new DelegateAction<DateTime>((utc) => UpdateCheckAction(utc), (utc) => _updateCheck != null);
        }

        private void BindPropertyChangedEvents()
        {
            BindPropertyChangedInternal("DragAndDropFiles", (IEnumerable<string> files) => { DragAndDropFilesTypes = DetermineFileTypes(files.Select(f => TypeMap.Resolve.New<IDataItem>(f))); });
            BindPropertyChangedInternal("DragAndDropFiles", (IEnumerable<string> files) => { DroppableAsRecent = DetermineDroppableAsRecent(files.Select(f => TypeMap.Resolve.New<IDataItem>(f))); });
            BindPropertyChangedInternal("DragAndDropFiles", (IEnumerable<string> files) => { DroppableAsWatchedFolder = DetermineDroppableAsWatchedFolder(files.Select(f => TypeMap.Resolve.New<IDataContainer>(f))); });
            BindPropertyChangedInternal("CurrentVersion", (Version cv) => { if (cv != null) UpdateUpdateCheck(cv); });
            BindPropertyChangedInternal("DebugMode", (bool enabled) => { UpdateDebugMode(enabled); });
            BindPropertyChangedInternal("RecentFilesComparer", (ActiveFileComparer comparer) => { SetRecentFiles(); });
        }

        private void SubscribeToModelEvents()
        {
            Resolve.SessionNotify.Notification += HandleSessionChanged;
            Resolve.ProgressBackground.WorkStatusChanged += (sender, e) =>
                {
                    Working = Resolve.ProgressBackground.Busy;
                };
            _fileSystemState.ActiveFileChanged += HandleActiveFileChangedEvent;
        }

        private static void UpdateDebugMode(bool enabled)
        {
            Resolve.Log.SetLevel(enabled ? LogLevel.Debug : LogLevel.Error);
            OS.Current.DebugMode(enabled);
        }

        private void UpdateUpdateCheck(Version currentVersion)
        {
            DisposeUpdateCheck();
            _updateCheck = TypeMap.Resolve.New<Version, UpdateCheck>(currentVersion);
            _updateCheck.VersionUpdate += Handle_VersionUpdate;
            UpdateCheckAction(Resolve.UserSettings.LastUpdateCheckUtc);
        }

        private void Handle_VersionUpdate(object sender, VersionEventArgs e)
        {
            Resolve.UserSettings.LastUpdateCheckUtc = OS.Current.UtcNow;
            Resolve.UserSettings.NewestKnownVersion = e.Version.ToString();
            Resolve.UserSettings.UpdateUrl = e.UpdateWebpageUrl;

            UpdatedVersion = e.Version;
            VersionUpdateStatus = e.VersionUpdateStatus;
        }

        private void HandleActiveFileChangedEvent(object sender, ActiveFileChangedEventArgs e)
        {
            SetFilesArePending();
            SetRecentFiles();
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
            return files.Any(fileInfo => fileInfo.Type() == FileInfoTypes.EncryptedFile || (Resolve.KnownKeys.IsLoggedOn && fileInfo.Type() == FileInfoTypes.EncryptableFile));
        }

        private static bool DetermineDroppableAsWatchedFolder(IEnumerable<IDataContainer> files)
        {
            if (files.Count() != 1)
            {
                return false;
            }

            IDataContainer fileInfo = files.First();
            if (!fileInfo.IsAvailable)
            {
                return false;
            }

            if (!fileInfo.IsEncryptable())
            {
                return false;
            }

            return true;
        }

        private void HandleSessionChanged(object sender, SessionNotificationEventArgs e)
        {
            switch (e.Notification.NotificationType)
            {
                case SessionNotificationType.WatchedFolderAdded:
                    SetWatchedFolders();
                    break;

                case SessionNotificationType.WatchedFolderRemoved:
                    SetWatchedFolders();
                    break;

                case SessionNotificationType.LogOn:
                    SetLogOnState(Resolve.KnownKeys.IsLoggedOn);
                    SetWatchedFolders();
                    break;

                case SessionNotificationType.LogOff:
                    SetLogOnState(Resolve.KnownKeys.IsLoggedOn);
                    SetWatchedFolders();
                    break;

                case SessionNotificationType.WatchedFolderChange:
                    SetFilesArePending();
                    break;

                case SessionNotificationType.WorkFolderChange:
                case SessionNotificationType.ProcessExit:
                case SessionNotificationType.SessionChange:
                case SessionNotificationType.SessionStart:
                case SessionNotificationType.KnownKeyChange:
                case SessionNotificationType.ActiveFileChange:
                default:
                    break;
            }
        }

        private void SetWatchedFolders()
        {
            WatchedFolders = Resolve.KnownKeys.LoggedOnWatchedFolders.Select(wf => wf.Path).ToList();
        }

        private void SetRecentFiles()
        {
            List<ActiveFile> activeFiles = new List<ActiveFile>(_fileSystemState.ActiveFiles).Where(af => !af.Status.HasFlag(ActiveFileStatus.Inactive)).ToList();
            if (RecentFilesComparer != null)
            {
                activeFiles.Sort(RecentFilesComparer);
            }
            RecentFiles = activeFiles;
            DecryptedFiles = _fileSystemState.DecryptedActiveFiles;
        }

        private void SetFilesArePending()
        {
            IList<ActiveFile> openFiles = _fileSystemState.DecryptedActiveFiles;
            FilesArePending = openFiles.Count > 0 || Resolve.KnownKeys.LoggedOnWatchedFolders.SelectMany(wf => TypeMap.Resolve.New<IDataContainer>(wf.Path).ListEncryptable()).Any();
        }

        private void SetLogOnState(bool isLoggedOn)
        {
            PassphraseIdentity identity = null;
            if (isLoggedOn)
            {
                identity = _fileSystemState.Identities.FirstOrDefault(i => i.Thumbprint == Resolve.KnownKeys.DefaultEncryptionKey.Thumbprint);
                if (identity == null)
                {
                    throw new InvalidOperationException("Attempt to log on without a matching identity being defined.");
                }
            }
            Identity = identity;
            LoggedOn = isLoggedOn;
            EncryptFileEnabled = isLoggedOn;
            DecryptFileEnabled = isLoggedOn;
            OpenEncryptedEnabled = isLoggedOn;
            WatchedFoldersEnabled = isLoggedOn;
        }

        private void ClearPassphraseMemoryAction()
        {
            IDataStore fileSystemStateInfo = Resolve.FileSystemState.PathInfo;
            TypeMap.Resolve.New<AxCryptFile>().Wipe(fileSystemStateInfo, new ProgressContext());
            TypeMap.Register.Singleton<FileSystemState>(() => FileSystemState.Create(fileSystemStateInfo));
            TypeMap.Register.Singleton<KnownKeys>(() => new KnownKeys(_fileSystemState, Resolve.SessionNotify));
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
                if (activeFile != null && !activeFile.Status.HasFlag(ActiveFileStatus.Inactive))
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
                _fileSystemState.AddWatchedFolder(new WatchedFolder(folder, Resolve.KnownKeys.DefaultEncryptionKey.Thumbprint));
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
                _fileSystemState.RemoveWatchedFolder(TypeMap.Resolve.New<IDataContainer>(watchedFolderPath));
            }
            _fileSystemState.Save();
        }

        private static void OpenSelectedFolderAction(string folder)
        {
            OS.Current.Launch(folder);
        }

        private void UpdateCheckAction(DateTime lastUpdateCheckUtc)
        {
            _updateCheck.CheckInBackground(lastUpdateCheckUtc, Resolve.UserSettings.NewestKnownVersion, Resolve.UserSettings.AxCrypt2VersionCheckUrl, Resolve.UserSettings.UpdateUrl);
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
            DisposeUpdateCheck();
        }

        private void DisposeUpdateCheck()
        {
            if (_updateCheck != null)
            {
                _updateCheck.VersionUpdate -= Handle_VersionUpdate;
                _updateCheck.Dispose();
            }
        }
    }
}