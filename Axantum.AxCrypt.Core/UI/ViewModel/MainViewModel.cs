#region Coypright and License

/*
 * AxCrypt - Copyright 2013, Svante Seleborg, All Rights Reserved
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
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

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

        public string LogOnName { get { return GetProperty<string>("LogOnName"); } set { SetProperty("LogOnName", value); } }

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

        public bool FilesAreOpen { get { return GetProperty<bool>("FilesAreOpen"); } set { SetProperty("FilesAreOpen", value); } }

        public Version CurrentVersion { get { return GetProperty<Version>("CurrentVersion"); } set { SetProperty("CurrentVersion", value); } }

        public Version UpdatedVersion { get { return GetProperty<Version>("UpdatedVersion"); } set { SetProperty("UpdatedVersion", value); } }

        public VersionUpdateStatus VersionUpdateStatus { get { return GetProperty<VersionUpdateStatus>("VersionUpdateStatus"); } set { SetProperty("VersionUpdateStatus", value); } }

        public bool DebugMode { get { return GetProperty<bool>("DebugMode"); } set { SetProperty("DebugMode", value); } }

        public IAction RemoveRecentFiles { get; private set; }

        public IAction AddWatchedFolders { get; private set; }

        public IAction PurgeActiveFiles { get; private set; }

        public IAction ClearPassphraseMemory { get; private set; }

        public IAction RemoveWatchedFolders { get; private set; }

        public IAction OpenSelectedFolder { get; private set; }

        public IAction DecryptFiles { get; private set; }

        public IAction EncryptFiles { get; private set; }

        public IAction OpenFiles { get; private set; }

        public IAction LogOnLogOff { get; private set; }

        public IAction DecryptFolders { get; private set; }

        public IAction UpdateCheck { get; private set; }

        public IAction WipeFiles { get; private set; }

        public IAction OpenFilesFromFolder { get; private set; }

        public IAction AddRecentFiles { get; private set; }

        public event EventHandler<FileSelectionEventArgs> SelectingFiles;

        public event EventHandler<LogOnEventArgs> LoggingOn;

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
            DebugMode = false;
            VersionUpdateStatus = UI.VersionUpdateStatus.Unknown;

            AddWatchedFolders = new DelegateAction<IEnumerable<string>>((folders) => AddWatchedFoldersAction(folders), (folders) => LoggedOn);
            RemoveRecentFiles = new DelegateAction<IEnumerable<string>>((files) => RemoveRecentFilesAction(files));
            PurgeActiveFiles = new DelegateAction<object>((parameter) => PurgeActiveFilesAction());
            ClearPassphraseMemory = new DelegateAction<object>((parameter) => ClearPassphraseMemoryAction());
            RemoveWatchedFolders = new DelegateAction<IEnumerable<string>>((files) => RemoveWatchedFoldersAction(files));
            OpenSelectedFolder = new DelegateAction<string>((folder) => OpenSelectedFolderAction(folder));
            DecryptFiles = new DelegateAction<IEnumerable<string>>((files) => DecryptFilesAction(files));
            EncryptFiles = new DelegateAction<IEnumerable<string>>((files) => EncryptFilesAction(files));
            OpenFiles = new DelegateAction<IEnumerable<string>>((files) => OpenFilesAction(files));
            LogOnLogOff = new DelegateAction<object>((parameter) => LogOnLogOffAction());
            DecryptFolders = new DelegateAction<IEnumerable<string>>((folders) => DecryptFoldersAction(folders));
            UpdateCheck = new DelegateAction<DateTime>((lastUpdateCheckUtc) => UpdateCheckAction(lastUpdateCheckUtc));
            WipeFiles = new DelegateAction<IEnumerable<string>>((files) => WipeFilesAction(files));
            OpenFilesFromFolder = new DelegateAction<string>((folder) => OpenFilesFromFolderAction(folder));
            AddRecentFiles = new DelegateAction<IEnumerable<string>>((files) => AddRecentFilesAction(files));
        }

        private void BindPropertyChangedEvents()
        {
            BindPropertyChanged("DragAndDropFiles", (IEnumerable<string> files) => { DragAndDropFilesTypes = DetermineFileTypes(files.Select(f => OS.Current.FileInfo(f))); });
            BindPropertyChanged("DragAndDropFiles", (IEnumerable<string> files) => { DroppableAsRecent = DetermineDroppableAsRecent(files.Select(f => OS.Current.FileInfo(f))); });
            BindPropertyChanged("DragAndDropFiles", (IEnumerable<string> files) => { DroppableAsWatchedFolder = DetermineDroppableAsWatchedFolder(files.Select(f => OS.Current.FileInfo(f))); });
            BindPropertyChanged("CurrentVersion", (Version cv) => { if (cv != null) UpdateUpdateCheck(cv); });
            BindPropertyChanged("DebugMode", (bool enabled) => { UpdateDebugMode(enabled); });
            BindPropertyChanged("RecentFilesComparer", (ActiveFileComparer comparer) => { SetRecentFiles(); });
        }

        private void SubscribeToModelEvents()
        {
            Instance.SessionNotification.Notification += HandleSessionChanged;
            _fileSystemState.ActiveFileChanged += HandleActiveFileChangedEvent;
        }

        protected virtual void OnSelectingFiles(FileSelectionEventArgs e)
        {
            EventHandler<FileSelectionEventArgs> handler = SelectingFiles;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnLoggingOn(LogOnEventArgs e)
        {
            EventHandler<LogOnEventArgs> handler = LoggingOn;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private static void UpdateDebugMode(bool enabled)
        {
            if (enabled)
            {
                Instance.Log.SetLevel(LogLevel.Debug);
                ServicePointManager.ServerCertificateValidationCallback = (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) =>
                {
                    return true;
                };
            }
            else
            {
                ServicePointManager.ServerCertificateValidationCallback = null;
                Instance.Log.SetLevel(LogLevel.Error);
            }
        }

        private void UpdateUpdateCheck(Version currentVersion)
        {
            DisposeUpdateCheck();
            _updateCheck = Factory.New<Version, UpdateCheck>(currentVersion);
            _updateCheck.VersionUpdate += Handle_VersionUpdate;
            UpdateCheckAction(Instance.UserSettings.LastUpdateCheckUtc);
        }

        private void Handle_VersionUpdate(object sender, VersionEventArgs e)
        {
            Instance.UserSettings.LastUpdateCheckUtc = OS.Current.UtcNow;
            Instance.UserSettings.NewestKnownVersion = e.Version.ToString();
            Instance.UserSettings.UpdateUrl = e.UpdateWebpageUrl;

            UpdatedVersion = e.Version;
            VersionUpdateStatus = e.VersionUpdateStatus;
        }

        private void HandleActiveFileChangedEvent(object sender, ActiveFileChangedEventArgs e)
        {
            Instance.UIThread.RunOnUIThread(() => SetFilesAreOpen());
            Instance.UIThread.RunOnUIThread(() => SetRecentFiles());
        }

        private static FileInfoTypes DetermineFileTypes(IEnumerable<IRuntimeFileInfo> files)
        {
            FileInfoTypes types = FileInfoTypes.None;
            FileInfoTypes typesToLookFor = FileInfoTypes.EncryptedFile | FileInfoTypes.EncryptableFile;
            foreach (IRuntimeFileInfo file in files)
            {
                types |= file.Type() & typesToLookFor;
                if ((types & typesToLookFor) == typesToLookFor)
                {
                    return types;
                }
            }
            return types;
        }

        private static bool DetermineDroppableAsRecent(IEnumerable<IRuntimeFileInfo> files)
        {
            return files.Any(fileInfo => fileInfo.Type() == FileInfoTypes.EncryptedFile || (Instance.KnownKeys.IsLoggedOn && fileInfo.Type() == FileInfoTypes.EncryptableFile));
        }

        private static bool DetermineDroppableAsWatchedFolder(IEnumerable<IRuntimeFileInfo> files)
        {
            if (files.Count() != 1)
            {
                return false;
            }

            IRuntimeFileInfo fileInfo = files.First();
            if (!fileInfo.IsFolder)
            {
                return false;
            }

            if (!fileInfo.NormalizeFolder().IsEncryptable())
            {
                return false;
            }

            return true;
        }

        private void HandleSessionChanged(object sender, SessionNotificationEventArgs e)
        {
            switch (e.Notification.NotificationType)
            {
                case SessionNotificationType.ActiveFileChange:
                    break;

                case SessionNotificationType.WatchedFolderAdded:
                    Instance.UIThread.RunOnUIThread(() => SetWatchedFolders());
                    break;

                case SessionNotificationType.WatchedFolderRemoved:
                    Instance.UIThread.RunOnUIThread(() => SetWatchedFolders());
                    break;

                case SessionNotificationType.LogOn:
                    Instance.UIThread.RunOnUIThread(() => SetLogOnState(Instance.KnownKeys.IsLoggedOn));
                    Instance.UIThread.RunOnUIThread(() => SetWatchedFolders());
                    break;

                case SessionNotificationType.LogOff:
                    Instance.UIThread.RunOnUIThread(() => SetLogOnState(Instance.KnownKeys.IsLoggedOn));
                    Instance.UIThread.RunOnUIThread(() => SetWatchedFolders());
                    break;

                case SessionNotificationType.ProcessExit:
                    break;

                case SessionNotificationType.SessionChange:
                    break;

                case SessionNotificationType.SessionStart:
                    break;

                case SessionNotificationType.KnownKeyChange:
                    break;

                case SessionNotificationType.WorkFolderChange:
                    break;

                default:
                    break;
            }
        }

        private void SetWatchedFolders()
        {
            WatchedFolders = Instance.KnownKeys.WatchedFolders.Select(wf => wf.Path).ToList();
        }

        private void SetRecentFiles()
        {
            List<ActiveFile> activeFiles = new List<ActiveFile>(_fileSystemState.ActiveFiles);
            if (RecentFilesComparer != null)
            {
                activeFiles.Sort(RecentFilesComparer);
            }
            RecentFiles = activeFiles;
            DecryptedFiles = _fileSystemState.DecryptedActiveFiles;
        }

        private void SetFilesAreOpen()
        {
            IList<ActiveFile> openFiles = _fileSystemState.DecryptedActiveFiles;
            FilesAreOpen = openFiles.Count > 0;
        }

        private void SetLogOnState(bool isLoggedOn)
        {
            string name = String.Empty;
            if (isLoggedOn)
            {
                PassphraseIdentity identity = _fileSystemState.Identities.First(i => i.Thumbprint == Instance.KnownKeys.DefaultEncryptionKey.Thumbprint);
                name = identity.Name;
            }
            LogOnName = name;
            LoggedOn = isLoggedOn;
            EncryptFileEnabled = isLoggedOn;
            DecryptFileEnabled = isLoggedOn;
            OpenEncryptedEnabled = isLoggedOn;
            WatchedFoldersEnabled = isLoggedOn;
        }

        private void LogOnLogOffAction()
        {
            if (Instance.KnownKeys.IsLoggedOn)
            {
                Instance.KnownKeys.Clear();
                return;
            }

            if (_fileSystemState.Identities.Any(identity => true))
            {
                TryLogOnToExistingIdentity();
                return;
            }

            string passphrase = AskForNewEncryptionPassphrase(String.Empty);
            if (String.IsNullOrEmpty(passphrase))
            {
                return;
            }

            Instance.KnownKeys.DefaultEncryptionKey = Passphrase.Derive(passphrase);
        }

        private void TryLogOnToExistingIdentity()
        {
            string passphrase = AskForLogOnPassphrase(PassphraseIdentity.Empty);
            if (String.IsNullOrEmpty(passphrase))
            {
                return;
            }
        }

        private void ClearPassphraseMemoryAction()
        {
            Factory.New<AxCryptFile>().Wipe(FileSystemState.DefaultPathInfo, new ProgressContext());
            Factory.Instance.Singleton<FileSystemState>(() => FileSystemState.Create(FileSystemState.DefaultPathInfo));
            Factory.Instance.Singleton<KnownKeys>(() => new KnownKeys(_fileSystemState, Instance.SessionNotification));
            Instance.SessionNotification.Notify(new SessionNotification(SessionNotificationType.SessionStart));
        }

        private void PurgeActiveFilesAction()
        {
            Instance.SessionNotification.Notify(new SessionNotification(SessionNotificationType.PurgeActiveFiles));
            Instance.SessionNotification.DoAllNow();
            SetRecentFiles();
        }

        private void RemoveRecentFilesAction(IEnumerable<string> files)
        {
            foreach (string file in files)
            {
                ActiveFile activeFile = _fileSystemState.FindEncryptedPath(file);
                if (activeFile != null)
                {
                    _fileSystemState.Remove(activeFile);
                }
            }
        }

        private void AddWatchedFoldersAction(IEnumerable<string> folders)
        {
            if (!folders.Any())
            {
                return;
            }
            foreach (string folder in folders)
            {
                _fileSystemState.AddWatchedFolder(new WatchedFolder(folder, Instance.KnownKeys.DefaultEncryptionKey.Thumbprint));
            }
            _fileSystemState.Save();
        }

        private void RemoveWatchedFoldersAction(IEnumerable<string> folders)
        {
            foreach (string watchedFolderPath in folders)
            {
                _fileSystemState.RemoveWatchedFolder(OS.Current.FileInfo(watchedFolderPath));
            }
            _fileSystemState.Save();
        }

        private static void OpenSelectedFolderAction(string folder)
        {
            OS.Current.Launch(folder);
        }

        private void DecryptFoldersAction(IEnumerable<string> folders)
        {
            Instance.ParallelBackground.DoFiles(folders.Select(f => OS.Current.FileInfo(f)).ToList(), DecryptFolder, (status) => { });
        }

        private void UpdateCheckAction(DateTime lastUpdateCheckUtc)
        {
            _updateCheck.CheckInBackground(lastUpdateCheckUtc, Instance.UserSettings.NewestKnownVersion, Instance.UserSettings.AxCrypt2VersionCheckUrl, Instance.UserSettings.UpdateUrl);
        }

        private FileOperationStatus DecryptFolder(IRuntimeFileInfo folder, IProgressContext progress)
        {
            Factory.New<AxCryptFile>().DecryptFilesUniqueWithWipeOfOriginal(folder, Instance.KnownKeys.DefaultEncryptionKey, progress);
            return FileOperationStatus.Success;
        }

        private void EncryptFilesAction(IEnumerable<string> files)
        {
            if (files == null)
            {
                FileSelectionEventArgs fileSelectionArgs = new FileSelectionEventArgs(new string[0])
                {
                    FileSelectionType = FileSelectionType.Encrypt,
                };
                OnSelectingFiles(fileSelectionArgs);
                if (fileSelectionArgs.Cancel)
                {
                    return;
                }
                files = fileSelectionArgs.SelectedFiles;
            }
            Instance.ParallelBackground.DoFiles(files.Select(f => OS.Current.FileInfo(f)).ToList(), EncryptFile, (status) => { });
        }

        private void DecryptFilesAction(IEnumerable<string> files)
        {
            if (files == null)
            {
                FileSelectionEventArgs fileSelectionArgs = new FileSelectionEventArgs(new string[0])
                {
                    FileSelectionType = FileSelectionType.Decrypt,
                };
                OnSelectingFiles(fileSelectionArgs);
                if (fileSelectionArgs.Cancel)
                {
                    return;
                }
                files = fileSelectionArgs.SelectedFiles;
            }
            Instance.ParallelBackground.DoFiles(files.Select(f => OS.Current.FileInfo(f)).ToList(), DecryptFile, (status) => { });
        }

        private FileOperationStatus DecryptFile(IRuntimeFileInfo file, IProgressContext progress)
        {
            FileOperationsController operationsController = new FileOperationsController(progress);

            operationsController.QueryDecryptionPassphrase += HandleQueryDecryptionPassphraseEvent;

            operationsController.QuerySaveFileAs += (object sender, FileOperationEventArgs e) =>
            {
                FileSelectionEventArgs fileSelectionArgs = new FileSelectionEventArgs(new string[] { e.SaveFileFullName })
                {
                    FileSelectionType = FileSelectionType.SaveAsDecrypted,
                };
                OnSelectingFiles(fileSelectionArgs);
                if (fileSelectionArgs.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
                e.SaveFileFullName = fileSelectionArgs.SelectedFiles[0];
            };

            operationsController.KnownKeyAdded += (object sender, FileOperationEventArgs e) =>
            {
                Instance.KnownKeys.Add(e.Key);
            };

            operationsController.Completed += (object sender, FileOperationEventArgs e) =>
            {
                if (Factory.Instance.Singleton<IStatusChecker>().CheckStatusAndShowMessage(e.Status, e.OpenFileFullName))
                {
                    Factory.New<ActiveFileAction>().RemoveRecentFiles(new IRuntimeFileInfo[] { OS.Current.FileInfo(e.OpenFileFullName) }, progress);
                }
            };

            return operationsController.DecryptFile(file);
        }

        private void WipeFilesAction(IEnumerable<string> files)
        {
            if (files == null)
            {
                FileSelectionEventArgs fileSelectionArgs = new FileSelectionEventArgs(new string[0])
                {
                    FileSelectionType = FileSelectionType.Wipe,
                };
                OnSelectingFiles(fileSelectionArgs);
                if (fileSelectionArgs.Cancel)
                {
                    return;
                }
                files = fileSelectionArgs.SelectedFiles;
            }
            Instance.ParallelBackground.DoFiles(files.Select(f => OS.Current.FileInfo(f)).ToList(), WipeFile, (status) => { });
        }

        private FileOperationStatus WipeFile(IRuntimeFileInfo file, IProgressContext progress)
        {
            FileOperationsController operationsController = new FileOperationsController(progress);

            operationsController.WipeQueryConfirmation += (object sender, FileOperationEventArgs e) =>
            {
                FileSelectionEventArgs fileSelectionArgs = new FileSelectionEventArgs(new string[] { file.FullName, })
                {
                    FileSelectionType = FileSelectionType.WipeConfirm,
                };
                OnSelectingFiles(fileSelectionArgs);
                e.Cancel = fileSelectionArgs.Cancel;
                e.Skip = fileSelectionArgs.Skip;
                e.ConfirmAll = fileSelectionArgs.ConfirmAll;
                e.SaveFileFullName = fileSelectionArgs.SelectedFiles.FirstOrDefault() ?? String.Empty;
            };

            operationsController.Completed += (object sender, FileOperationEventArgs e) =>
            {
                if (e.Skip)
                {
                    return;
                }
                if (Instance.StatusChecker.CheckStatusAndShowMessage(e.Status, e.OpenFileFullName))
                {
                    Factory.New<ActiveFileAction>().RemoveRecentFiles(new IRuntimeFileInfo[] { OS.Current.FileInfo(e.SaveFileFullName) }, progress);
                }
            };

            return operationsController.WipeFile(file);
        }

        private void OpenFilesFromFolderAction(string folder)
        {
            FileSelectionEventArgs fileSelectionArgs = new FileSelectionEventArgs(new string[] { folder })
            {
                FileSelectionType = FileSelectionType.Open,
            };
            OnSelectingFiles(fileSelectionArgs);
            if (fileSelectionArgs.Cancel)
            {
                return;
            }
            OpenFilesAction(fileSelectionArgs.SelectedFiles);
        }

        private void OpenFilesAction(IEnumerable<string> files)
        {
            Instance.ParallelBackground.DoFiles(files.Select(f => OS.Current.FileInfo(f)).ToList(), OpenEncrypted, (status) => { });
        }

        private FileOperationStatus OpenEncrypted(IRuntimeFileInfo file, IProgressContext progress)
        {
            FileOperationsController operationsController = new FileOperationsController(progress);

            operationsController.QueryDecryptionPassphrase += HandleQueryDecryptionPassphraseEvent;

            operationsController.KnownKeyAdded += (object sender, FileOperationEventArgs e) =>
            {
                Instance.KnownKeys.Add(e.Key);
            };

            operationsController.Completed += (object sender, FileOperationEventArgs e) =>
            {
                if (e.Status == FileOperationStatus.Canceled)
                {
                    return;
                }
                Factory.Instance.Singleton<IStatusChecker>().CheckStatusAndShowMessage(e.Status, e.OpenFileFullName);
            };

            return operationsController.DecryptAndLaunch(file);
        }

        private void HandleQueryDecryptionPassphraseEvent(object sender, FileOperationEventArgs e)
        {
            string passphraseText = AskForLogOnOrDecryptPassphrase(e.OpenFileFullName);
            if (String.IsNullOrEmpty(passphraseText))
            {
                e.Cancel = true;
                return;
            }
            e.Passphrase = passphraseText;
        }

        private string AskForLogOnOrDecryptPassphrase(string fullName)
        {
            ActiveFile openFile = _fileSystemState.FindEncryptedPath(fullName);
            if (openFile == null || openFile.Thumbprint == null)
            {
                return AskForLogOnPassphrase(PassphraseIdentity.Empty);
            }

            PassphraseIdentity identity = _fileSystemState.Identities.FirstOrDefault(i => i.Thumbprint == openFile.Thumbprint);
            if (identity == null)
            {
                return AskForLogOnPassphrase(PassphraseIdentity.Empty);
            }

            return AskForLogOnPassphrase(identity);
        }

        private FileOperationStatus EncryptFile(IRuntimeFileInfo file, IProgressContext progress)
        {
            FileOperationsController operationsController = new FileOperationsController(progress);

            operationsController.QuerySaveFileAs += (object sender, FileOperationEventArgs e) =>
            {
                FileSelectionEventArgs fileSelectionArgs = new FileSelectionEventArgs(new string[] { e.SaveFileFullName })
                {
                    FileSelectionType = FileSelectionType.SaveAsEncrypted,
                };
                OnSelectingFiles(fileSelectionArgs);
                if (fileSelectionArgs.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
                e.SaveFileFullName = fileSelectionArgs.SelectedFiles[0];
            };

            operationsController.QueryEncryptionPassphrase += (object sender, FileOperationEventArgs e) =>
            {
                string passphrase = AskForLogOnPassphrase(PassphraseIdentity.Empty);
                if (String.IsNullOrEmpty(passphrase))
                {
                    e.Cancel = true;
                    return;
                }
                e.Passphrase = passphrase;
            };

            operationsController.Completed += (object sender, FileOperationEventArgs e) =>
            {
                if (e.Status == FileOperationStatus.FileAlreadyEncrypted)
                {
                    e.Status = FileOperationStatus.Success;
                    return;
                }
                if (Factory.Instance.Singleton<IStatusChecker>().CheckStatusAndShowMessage(e.Status, e.OpenFileFullName))
                {
                    IRuntimeFileInfo encryptedInfo = OS.Current.FileInfo(e.SaveFileFullName);
                    IRuntimeFileInfo decryptedInfo = OS.Current.FileInfo(FileOperation.GetTemporaryDestinationName(e.OpenFileFullName));
                    ActiveFile activeFile = new ActiveFile(encryptedInfo, decryptedInfo, e.Key, ActiveFileStatus.NotDecrypted);
                    _fileSystemState.Add(activeFile);
                    _fileSystemState.Save();
                }
            };

            return operationsController.EncryptFile(file);
        }

        private void AddRecentFilesAction(IEnumerable<string> files)
        {
            IEnumerable<IRuntimeFileInfo> fileInfos = files.Select(f => OS.Current.FileInfo(f)).ToList();
            ProcessEncryptableFilesDroppedInRecentList(fileInfos.Where(fileInfo => Instance.KnownKeys.IsLoggedOn && fileInfo.Type() == FileInfoTypes.EncryptableFile));
            ProcessEncryptedFilesDroppedInRecentList(fileInfos.Where(fileInfo => fileInfo.Type() == FileInfoTypes.EncryptedFile));
        }

        private void ProcessEncryptedFilesDroppedInRecentList(IEnumerable<IRuntimeFileInfo> encryptedFiles)
        {
            Instance.ParallelBackground.DoFiles(encryptedFiles, VerifyAndAddActive, (status) => { });
        }

        private FileOperationStatus VerifyAndAddActive(IRuntimeFileInfo fullName, IProgressContext progress)
        {
            FileOperationsController operationsController = new FileOperationsController(progress);

            operationsController.QueryDecryptionPassphrase += HandleQueryDecryptionPassphraseEvent;

            operationsController.KnownKeyAdded += (object sender, FileOperationEventArgs e) =>
            {
                Instance.KnownKeys.Add(e.Key);
            };

            operationsController.Completed += (object sender, FileOperationEventArgs e) =>
            {
                if (e.Skip)
                {
                    return;
                }
                if (Factory.Instance.Singleton<IStatusChecker>().CheckStatusAndShowMessage(e.Status, e.OpenFileFullName))
                {
                    IRuntimeFileInfo encryptedInfo = OS.Current.FileInfo(e.OpenFileFullName);
                    IRuntimeFileInfo decryptedInfo = OS.Current.FileInfo(e.SaveFileFullName);
                    ActiveFile activeFile = new ActiveFile(encryptedInfo, decryptedInfo, e.Key, ActiveFileStatus.NotDecrypted);
                    _fileSystemState.Add(activeFile);
                    _fileSystemState.Save();
                }
            };

            return operationsController.VerifyEncrypted(fullName);
        }

        private void ProcessEncryptableFilesDroppedInRecentList(IEnumerable<IRuntimeFileInfo> encryptableFiles)
        {
            Instance.ParallelBackground.DoFiles(encryptableFiles, EncryptFileNonInteractive, (status) => { });
        }

        private FileOperationStatus EncryptFileNonInteractive(IRuntimeFileInfo fullName, IProgressContext progress)
        {
            FileOperationsController operationsController = new FileOperationsController(progress);

            operationsController.QuerySaveFileAs += (object sender, FileOperationEventArgs e) =>
            {
                e.SaveFileFullName = OS.Current.FileInfo(e.SaveFileFullName).FullName.CreateUniqueFile();
            };

            operationsController.Completed += (object sender, FileOperationEventArgs e) =>
            {
                if (Factory.Instance.Singleton<IStatusChecker>().CheckStatusAndShowMessage(e.Status, e.OpenFileFullName))
                {
                    IRuntimeFileInfo encryptedInfo = OS.Current.FileInfo(e.SaveFileFullName);
                    IRuntimeFileInfo decryptedInfo = OS.Current.FileInfo(FileOperation.GetTemporaryDestinationName(e.OpenFileFullName));
                    ActiveFile activeFile = new ActiveFile(encryptedInfo, decryptedInfo, e.Key, ActiveFileStatus.NotDecrypted);
                    _fileSystemState.Add(activeFile);
                    _fileSystemState.Save();
                }
            };

            return operationsController.EncryptFile(fullName);
        }

        private string AskForLogOnPassphrase(PassphraseIdentity identity)
        {
            string passphrase = AskForLogOnOrEncryptionPassphrase(identity);
            if (passphrase.Length == 0)
            {
                return String.Empty;
            }

            Instance.KnownKeys.DefaultEncryptionKey = Passphrase.Derive(passphrase);
            return passphrase;
        }

        private string AskForLogOnOrEncryptionPassphrase(PassphraseIdentity identity)
        {
            LogOnEventArgs logOnArgs = new LogOnEventArgs()
            {
                DisplayPassphrase = Instance.UserSettings.DisplayEncryptPassphrase,
                Identity = identity,
            };
            OnLoggingOn(logOnArgs);

            if (logOnArgs.CreateNew)
            {
                return AskForNewEncryptionPassphrase(logOnArgs.Passphrase);
            }

            if (logOnArgs.Cancel || logOnArgs.Passphrase.Length == 0)
            {
                return String.Empty;
            }

            Instance.UserSettings.DisplayEncryptPassphrase = logOnArgs.DisplayPassphrase;

            return logOnArgs.Passphrase;
        }

        private string AskForNewEncryptionPassphrase(string defaultPassphrase)
        {
            LogOnEventArgs logOnArgs = new LogOnEventArgs()
            {
                CreateNew = true,
                DisplayPassphrase = Instance.UserSettings.DisplayEncryptPassphrase,
                Passphrase = defaultPassphrase
            };
            OnLoggingOn(logOnArgs);

            if (logOnArgs.Cancel || logOnArgs.Passphrase.Length == 0)
            {
                return String.Empty;
            }

            Instance.UserSettings.DisplayEncryptPassphrase = logOnArgs.DisplayPassphrase;

            Passphrase passphrase = new Passphrase(logOnArgs.Passphrase);
            PassphraseIdentity identity = _fileSystemState.Identities.FirstOrDefault(i => i.Thumbprint == passphrase.DerivedPassphrase.Thumbprint);
            if (identity != null)
            {
                return logOnArgs.Passphrase;
            }

            identity = new PassphraseIdentity(logOnArgs.Name, passphrase.DerivedPassphrase);
            _fileSystemState.Identities.Add(identity);
            _fileSystemState.Save();

            return logOnArgs.Passphrase;
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