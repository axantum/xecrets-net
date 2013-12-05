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

using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Axantum.AxCrypt.Core.UI.ViewModel
{
    public class MainViewModel : ViewModelBase, IDisposable
    {
        private UpdateCheck _updateCheck;

        public MainViewModel()
        {
            InitializePropertyValues();
            BindPropertyChangedEvents();
            SubscribeToModelEvents();
        }

        private void InitializePropertyValues()
        {
            LogonEnabled = true;
            WatchedFoldersEnabled = false;
            WatchedFolders = new string[0];
            DragAndDropFiles = new string[0];
            RecentFiles = new ActiveFile[0];
            VersionUpdateStatus = UI.VersionUpdateStatus.Unknown;
        }

        private void BindPropertyChangedEvents()
        {
            BindPropertyChanged("DragAndDropFiles", (IEnumerable<string> files) => { DragAndDropFilesTypes = DetermineFileTypes(files.Select(f => OS.Current.FileInfo(f))); });
            BindPropertyChanged("DragAndDropFiles", (IEnumerable<string> files) => { DroppableAsRecent = DetermineDroppableAsRecent(files.Select(f => OS.Current.FileInfo(f))); });
            BindPropertyChanged("DragAndDropFiles", (IEnumerable<string> files) => { DroppableAsWatchedFolder = DetermineDroppableAsWatchedFolder(files.Select(f => OS.Current.FileInfo(f))); });
            BindPropertyChanged("CurrentVersion", (Version cv) => { if (cv != null) UpdateUpdateCheck(cv); });
        }

        private void UpdateUpdateCheck(Version currentVersion)
        {
            DisposeUpdateCheck();
            _updateCheck = new UpdateCheck(currentVersion);
            _updateCheck.VersionUpdate += Handle_VersionUpdate;
            UpdateCheck(Instance.UserSettings.LastUpdateCheckUtc);
        }

        private void Handle_VersionUpdate(object sender, VersionEventArgs e)
        {
            Instance.UserSettings.LastUpdateCheckUtc = OS.Current.UtcNow;
            Instance.UserSettings.NewestKnownVersion = e.Version.ToString();
            Instance.UserSettings.UpdateUrl = e.UpdateWebpageUrl;

            UpdatedVersion = e.Version;
            VersionUpdateStatus = e.VersionUpdateStatus;
        }

        private void SubscribeToModelEvents()
        {
            OS.Current.SessionChanged += HandleSessionChanged;
            Instance.FileSystemState.ActiveFileChanged += HandleActiveFileChangedEvent;
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
                types |= file.Type();
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

        private void HandleSessionChanged(object sender, SessionEventArgs e)
        {
            foreach (SessionEvent sessionEvent in e.SessionEvents)
            {
                switch (sessionEvent.SessionEventType)
                {
                    case SessionEventType.ActiveFileChange:
                        break;

                    case SessionEventType.WatchedFolderAdded:
                        Instance.UIThread.RunOnUIThread(() => SetWatchedFolders());
                        break;

                    case SessionEventType.WatchedFolderRemoved:
                        Instance.UIThread.RunOnUIThread(() => SetWatchedFolders());
                        break;

                    case SessionEventType.LogOn:
                        Instance.UIThread.RunOnUIThread(() => SetLogOnState(Instance.KnownKeys.IsLoggedOn));
                        Instance.UIThread.RunOnUIThread(() => SetWatchedFolders());
                        break;

                    case SessionEventType.LogOff:
                        Instance.UIThread.RunOnUIThread(() => SetLogOnState(Instance.KnownKeys.IsLoggedOn));
                        Instance.UIThread.RunOnUIThread(() => SetWatchedFolders());
                        break;

                    case SessionEventType.ProcessExit:
                        break;

                    case SessionEventType.SessionChange:
                        break;

                    case SessionEventType.SessionStart:
                        break;

                    case SessionEventType.KnownKeyChange:
                        break;

                    case SessionEventType.WorkFolderChange:
                        break;

                    default:
                        break;
                }
            }
        }

        private void SetWatchedFolders()
        {
            WatchedFolders = Instance.KnownKeys.WatchedFolders.Select(wf => wf.Path).ToList();
        }

        private void SetRecentFiles()
        {
            RecentFiles = Instance.FileSystemState.ActiveFiles;
        }

        private void SetFilesAreOpen()
        {
            IList<ActiveFile> openFiles = Instance.FileSystemState.DecryptedActiveFiles;
            FilesAreOpen = openFiles.Count > 0;
        }

        public bool LogonEnabled { get { return GetProperty<bool>("LogonEnabled"); } set { SetProperty("LogonEnabled", value); } }

        public bool EncryptFileEnabled { get { return GetProperty<bool>("EncryptFileEnabled"); } set { SetProperty("EncryptFileEnabled", value); } }

        public bool DecryptFileEnabled { get { return GetProperty<bool>("DecryptFileEnabled"); } set { SetProperty("DecryptFileEnabled", value); } }

        public bool OpenEncryptedEnabled { get { return GetProperty<bool>("OpenEncryptedEnabled"); } set { SetProperty("OpenEncryptedEnabled", value); } }

        public bool WatchedFoldersEnabled { get { return GetProperty<bool>("WatchedFoldersEnabled"); } set { SetProperty("WatchedFoldersEnabled", value); } }

        public string LogOnName { get { return GetProperty<string>("LogOnName"); } set { SetProperty("LogOnName", value); } }

        public IEnumerable<string> WatchedFolders { get { return GetProperty<IEnumerable<string>>("WatchedFolders"); } set { SetProperty("WatchedFolders", value.ToList()); } }

        public IEnumerable<ActiveFile> RecentFiles { get { return GetProperty<IEnumerable<ActiveFile>>("RecentFiles"); } set { SetProperty("RecentFiles", value.ToList()); } }

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

        public void LogOnLogOff()
        {
            LogOnLogOffInternal();
        }

        private void SetLogOnState(bool isLoggedOn)
        {
            string name = String.Empty;
            if (isLoggedOn)
            {
                PassphraseIdentity identity = Instance.FileSystemState.Identities.First(i => i.Thumbprint == Instance.KnownKeys.DefaultEncryptionKey.Thumbprint);
                name = identity.Name;
            }
            LogOnName = name;
            LogonEnabled = !isLoggedOn;
            EncryptFileEnabled = isLoggedOn;
            DecryptFileEnabled = isLoggedOn;
            OpenEncryptedEnabled = isLoggedOn;
            WatchedFoldersEnabled = isLoggedOn;
        }

        private void LogOnLogOffInternal()
        {
            if (Instance.KnownKeys.IsLoggedOn)
            {
                Instance.KnownKeys.Clear();
                return;
            }

            if (Instance.FileSystemState.Identities.Any(identity => true))
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

        public void ClearPassphraseMemory()
        {
            AxCryptFile.Wipe(FileSystemState.DefaultPathInfo, new ProgressContext());
            FactoryRegistry.Instance.Singleton<FileSystemState>(FileSystemState.Create(FileSystemState.DefaultPathInfo));
            FactoryRegistry.Instance.Singleton<KnownKeys>(new KnownKeys());
        }

        public void RemoveRecentFiles(IEnumerable<string> files)
        {
            foreach (string file in files)
            {
                ActiveFile activeFile = Instance.FileSystemState.FindEncryptedPath(file);
                if (activeFile != null)
                {
                    Instance.FileSystemState.Remove(activeFile);
                }
            }
        }

        public void AddWatchedFolders(IEnumerable<string> folders)
        {
            foreach (string folder in folders)
            {
                Instance.FileSystemState.AddWatchedFolder(new WatchedFolder(folder, Instance.KnownKeys.DefaultEncryptionKey.Thumbprint));
            }
            Instance.FileSystemState.Save();
        }

        public void RemoveWatchedFolders(IEnumerable<string> folders)
        {
            foreach (string watchedFolderPath in folders)
            {
                Instance.FileSystemState.RemoveWatchedFolder(OS.Current.FileInfo(watchedFolderPath));
            }
            Instance.FileSystemState.Save();
        }

        public void OpenSelectedFolder(string folder)
        {
            OS.Current.Launch(folder);
        }

        public void DecryptWatchedFolders(IEnumerable<string> folders)
        {
            Instance.ParallelBackground.DoFiles(folders.Select(f => OS.Current.FileInfo(f)).ToList(), DecryptWatchedFolder, (status) => { });
        }

        public void UpdateCheck(DateTime lastUpdateCheckUtc)
        {
            _updateCheck.CheckInBackground(lastUpdateCheckUtc, Instance.UserSettings.NewestKnownVersion, Instance.UserSettings.AxCrypt2VersionCheckUrl, Instance.UserSettings.UpdateUrl); 
        }

        private FileOperationStatus DecryptWatchedFolder(IRuntimeFileInfo folder, IProgressContext progress)
        {
            Factory.AxCryptFile.DecryptFilesUniqueWithWipeOfOriginal(folder, Instance.KnownKeys.DefaultEncryptionKey, progress);
            return FileOperationStatus.Success;
        }

        public void EncryptFiles()
        {
            FileSelectionEventArgs fileSelectionArgs = new FileSelectionEventArgs()
            {
                FileSelectionType = FileSelectionType.Encrypt,
            };
            OnSelectingFiles(fileSelectionArgs);
            if (fileSelectionArgs.Cancel)
            {
                return;
            }
            EncryptFiles(fileSelectionArgs.SelectedFiles);
        }

        public void EncryptFiles(IEnumerable<string> files)
        {
            Instance.ParallelBackground.DoFiles(files.Select(f => OS.Current.FileInfo(f)).ToList(), EncryptFile, (status) => { });
        }

        public void DecryptFiles()
        {
            FileSelectionEventArgs fileSelectionArgs = new FileSelectionEventArgs()
            {
                FileSelectionType = FileSelectionType.Decrypt,
            };
            OnSelectingFiles(fileSelectionArgs);
            if (fileSelectionArgs.Cancel)
            {
                return;
            }
            DecryptFiles(fileSelectionArgs.SelectedFiles);
        }

        public void DecryptFiles(IEnumerable<string> files)
        {
            Instance.ParallelBackground.DoFiles(files.Select(f => OS.Current.FileInfo(f)).ToList(), DecryptFile, (status) => { });
        }

        public FileOperationStatus DecryptFile(IRuntimeFileInfo file, IProgressContext progress)
        {
            FileOperationsController operationsController = new FileOperationsController(progress);

            operationsController.QueryDecryptionPassphrase += HandleQueryDecryptionPassphraseEvent;

            operationsController.QuerySaveFileAs += (object sender, FileOperationEventArgs e) =>
            {
                FileSelectionEventArgs fileSelectionArgs = new FileSelectionEventArgs()
                {
                    FileSelectionType = FileSelectionType.SaveAsDecrypted,
                    SelectedFiles = new string[] { e.SaveFileFullName },
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
                if (FactoryRegistry.Instance.Singleton<IStatusChecker>().CheckStatusAndShowMessage(e.Status, e.OpenFileFullName))
                {
                    Instance.FileSystemState.Actions.RemoveRecentFiles(new IRuntimeFileInfo[] { OS.Current.FileInfo(e.OpenFileFullName) }, progress);
                }
            };

            return operationsController.DecryptFile(file);
        }

        public void WipeFiles()
        {
            FileSelectionEventArgs fileSelectionArgs = new FileSelectionEventArgs()
            {
                FileSelectionType = FileSelectionType.Wipe,
            };
            OnSelectingFiles(fileSelectionArgs);
            if (fileSelectionArgs.Cancel)
            {
                return;
            }
            WipeFiles(fileSelectionArgs.SelectedFiles);
        }

        public void WipeFiles(IEnumerable<string> files)
        {
            Instance.ParallelBackground.DoFiles(files.Select(f => OS.Current.FileInfo(f)).ToList(), WipeFile, (status) => { });
        }

        private FileOperationStatus WipeFile(IRuntimeFileInfo file, IProgressContext progress)
        {
            FileOperationsController operationsController = new FileOperationsController(progress);

            operationsController.WipeQueryConfirmation += (object sender, FileOperationEventArgs e) =>
            {
                FileSelectionEventArgs fileSelectionArgs = new FileSelectionEventArgs()
                {
                    FileSelectionType = FileSelectionType.WipeConfirm,
                    SelectedFiles = new string[] { file.FullName, },
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
                    Instance.FileSystemState.Actions.RemoveRecentFiles(new IRuntimeFileInfo[] { OS.Current.FileInfo(e.SaveFileFullName) }, progress);
                }
            };

            return operationsController.WipeFile(file);
        }

        public void OpenFileFromFolder(string folder)
        {
            FileSelectionEventArgs fileSelectionArgs = new FileSelectionEventArgs()
            {
                FileSelectionType = FileSelectionType.Open,
                SelectedFiles = new string[] { folder },
            };
            OnSelectingFiles(fileSelectionArgs);
            if (fileSelectionArgs.Cancel)
            {
                return;
            }
            OpenFiles(fileSelectionArgs.SelectedFiles);
        }

        public void OpenFiles(IEnumerable<string> files)
        {
            Instance.ParallelBackground.DoFiles(files.Select(f => OS.Current.FileInfo(f)).ToList(), OpenEncrypted, (status) => { });
        }

        public FileOperationStatus OpenEncrypted(IRuntimeFileInfo file, IProgressContext progress)
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
                FactoryRegistry.Instance.Singleton<IStatusChecker>().CheckStatusAndShowMessage(e.Status, e.OpenFileFullName);
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
            ActiveFile openFile = Instance.FileSystemState.FindEncryptedPath(fullName);
            if (openFile == null || openFile.Thumbprint == null)
            {
                return AskForLogOnPassphrase(PassphraseIdentity.Empty);
            }

            PassphraseIdentity identity = Instance.FileSystemState.Identities.FirstOrDefault(i => i.Thumbprint == openFile.Thumbprint);
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
                FileSelectionEventArgs fileSelectionArgs = new FileSelectionEventArgs()
                {
                    FileSelectionType = FileSelectionType.SaveAsEncrypted,
                    SelectedFiles = new string[] { e.SaveFileFullName },
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
                if (FactoryRegistry.Instance.Singleton<IStatusChecker>().CheckStatusAndShowMessage(e.Status, e.OpenFileFullName))
                {
                    IRuntimeFileInfo encryptedInfo = OS.Current.FileInfo(e.SaveFileFullName);
                    IRuntimeFileInfo decryptedInfo = OS.Current.FileInfo(FileOperation.GetTemporaryDestinationName(e.OpenFileFullName));
                    ActiveFile activeFile = new ActiveFile(encryptedInfo, decryptedInfo, e.Key, ActiveFileStatus.NotDecrypted);
                    Instance.FileSystemState.Add(activeFile);
                    Instance.FileSystemState.Save();
                }
            };

            return operationsController.EncryptFile(file);
        }

        public void DragAndDroppedRecentFiles()
        {
            IEnumerable<IRuntimeFileInfo> files = DragAndDropFiles.Select(f => OS.Current.FileInfo(f)).ToList();
            ProcessEncryptableFilesDroppedInRecentList(files.Where(fileInfo => Instance.KnownKeys.IsLoggedOn && fileInfo.Type() == FileInfoTypes.EncryptableFile));
            ProcessEncryptedFilesDroppedInRecentList(files.Where(fileInfo => fileInfo.Type() == FileInfoTypes.EncryptedFile));
        }

        private void ProcessEncryptedFilesDroppedInRecentList(IEnumerable<IRuntimeFileInfo> encryptedFiles)
        {
            Instance.ParallelBackground.DoFiles(encryptedFiles, VerifyAndAddActive, (status) => { });
        }

        public FileOperationStatus VerifyAndAddActive(IRuntimeFileInfo fullName, IProgressContext progress)
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
                if (FactoryRegistry.Instance.Singleton<IStatusChecker>().CheckStatusAndShowMessage(e.Status, e.OpenFileFullName))
                {
                    IRuntimeFileInfo encryptedInfo = OS.Current.FileInfo(e.OpenFileFullName);
                    IRuntimeFileInfo decryptedInfo = OS.Current.FileInfo(e.SaveFileFullName);
                    ActiveFile activeFile = new ActiveFile(encryptedInfo, decryptedInfo, e.Key, ActiveFileStatus.NotDecrypted);
                    Instance.FileSystemState.Add(activeFile);
                    Instance.FileSystemState.Save();
                }
            };

            return operationsController.VerifyEncrypted(fullName);
        }

        private void ProcessEncryptableFilesDroppedInRecentList(IEnumerable<IRuntimeFileInfo> encryptableFiles)
        {
            Instance.ParallelBackground.DoFiles(encryptableFiles, EncryptFileNonInteractive, (status) => { });
        }

        public FileOperationStatus EncryptFileNonInteractive(IRuntimeFileInfo fullName, IProgressContext progress)
        {
            FileOperationsController operationsController = new FileOperationsController(progress);

            operationsController.QuerySaveFileAs += (object sender, FileOperationEventArgs e) =>
            {
                e.SaveFileFullName = OS.Current.FileInfo(e.SaveFileFullName).FullName.CreateUniqueFile();
            };

            operationsController.Completed += (object sender, FileOperationEventArgs e) =>
            {
                if (FactoryRegistry.Instance.Singleton<IStatusChecker>().CheckStatusAndShowMessage(e.Status, e.OpenFileFullName))
                {
                    IRuntimeFileInfo encryptedInfo = OS.Current.FileInfo(e.SaveFileFullName);
                    IRuntimeFileInfo decryptedInfo = OS.Current.FileInfo(FileOperation.GetTemporaryDestinationName(e.OpenFileFullName));
                    ActiveFile activeFile = new ActiveFile(encryptedInfo, decryptedInfo, e.Key, ActiveFileStatus.NotDecrypted);
                    Instance.FileSystemState.Add(activeFile);
                    Instance.FileSystemState.Save();
                }
            };

            return operationsController.EncryptFile(fullName);
        }

        public event EventHandler<FileSelectionEventArgs> SelectingFiles;

        protected virtual void OnSelectingFiles(FileSelectionEventArgs e)
        {
            EventHandler<FileSelectionEventArgs> handler = SelectingFiles;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public event EventHandler<LogOnEventArgs> LoggingOn;

        protected virtual void OnLogggingOn(LogOnEventArgs e)
        {
            EventHandler<LogOnEventArgs> handler = LoggingOn;
            if (handler != null)
            {
                handler(this, e);
            }
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
            OnLogggingOn(logOnArgs);

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
            OnLogggingOn(logOnArgs);

            if (logOnArgs.Cancel || logOnArgs.Passphrase.Length == 0)
            {
                return String.Empty;
            }

            Instance.UserSettings.DisplayEncryptPassphrase = logOnArgs.DisplayPassphrase;

            Passphrase passphrase = new Passphrase(logOnArgs.Passphrase);
            PassphraseIdentity identity = Instance.FileSystemState.Identities.FirstOrDefault(i => i.Thumbprint == passphrase.DerivedPassphrase.Thumbprint);
            if (identity != null)
            {
                return logOnArgs.Passphrase;
            }

            identity = new PassphraseIdentity(logOnArgs.Passphrase, passphrase.DerivedPassphrase);
            Instance.FileSystemState.Identities.Add(identity);
            Instance.FileSystemState.Save();

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