using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.UI.ViewModel
{
    public class WatchedFoldersViewModel : ViewModelBase
    {
        private FileSystemState _fileSystemState;

        public bool LoggedOn { get { return GetProperty<bool>(nameof(LoggedOn)); } set { SetProperty(nameof(LoggedOn), value); } }

        public IEnumerable<string> WatchedFolders { get { return GetProperty<IEnumerable<string>>(nameof(WatchedFolders)); } set { SetProperty(nameof(WatchedFolders), value.ToList()); } }

        public bool WatchedFoldersEnabled { get { return GetProperty<bool>(nameof(WatchedFoldersEnabled)); } set { SetProperty(nameof(WatchedFoldersEnabled), value); } }

        public IEnumerable<string> SelectedWatchedFolders { get { return GetProperty<IEnumerable<string>>(nameof(SelectedWatchedFolders)); } set { SetProperty(nameof(SelectedWatchedFolders), value.ToList()); } }

        public bool FilesArePending { get { return GetProperty<bool>(nameof(FilesArePending)); } set { SetProperty(nameof(FilesArePending), value); } }

        public bool DroppableAsWatchedFolder { get { return GetProperty<bool>(nameof(DroppableAsWatchedFolder)); } set { SetProperty(nameof(DroppableAsWatchedFolder), value); } }

        public IEnumerable<string> DragAndDropFiles { get { return GetProperty<IEnumerable<string>>(nameof(DragAndDropFiles)); } set { SetProperty(nameof(DragAndDropFiles), value.ToList()); } }

        public IAction AddWatchedFolders { get; private set; }

        public IAction RemoveWatchedFolders { get; private set; }

        public IAction OpenSelectedFolder { get; private set; }

        public WatchedFoldersViewModel(FileSystemState fileSystemState)
        {
            _fileSystemState = fileSystemState;

            InitializePropertyValues();
            BindPropertyChangedEvents();
            SubscribeToModelEvents();
            SetWatchedFolders();
            SetLogOnState(Resolve.KnownIdentities.IsLoggedOn);
        }

        private void InitializePropertyValues()
        {
            WatchedFoldersEnabled = false;
            WatchedFolders = new string[0];
            DragAndDropFiles = new string[0];
            SelectedWatchedFolders = new string[0];

            AddWatchedFolders = new DelegateAction<IEnumerable<string>>((folders) => AddWatchedFoldersAction(folders), (folders) => LoggedOn);
            RemoveWatchedFolders = new DelegateAction<IEnumerable<string>>((folders) => RemoveWatchedFoldersAction(folders), (folders) => LoggedOn);
            OpenSelectedFolder = new DelegateAction<string>((folder) => OpenSelectedFolderAction(folder));
        }

        private void BindPropertyChangedEvents()
        {
            BindPropertyChanged(nameof(DragAndDropFiles), (IEnumerable<string> files) => { DroppableAsWatchedFolder = DetermineDroppableAsWatchedFolder(files.Select(f => New<IDataItem>(f))); });
        }

        private void SubscribeToModelEvents()
        {
            Resolve.SessionNotify.Notification += HandleSessionChanged;
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
                case SessionNotificationType.WatchedFolderRemoved:
                    SetWatchedFolders();
                    break;

                case SessionNotificationType.LogOn:
                case SessionNotificationType.LogOff:
                    SetLogOnState(Resolve.KnownIdentities.IsLoggedOn);
                    SetWatchedFolders();
                    break;
            }
        }

        private void SetWatchedFolders()
        {
            WatchedFolders = Resolve.KnownIdentities.LoggedOnWatchedFolders.Select(wf => wf.Path).ToList();
        }

        private void SetLogOnState(bool isLoggedOn)
        {
            WatchedFoldersEnabled = isLoggedOn;
            LoggedOn = isLoggedOn;
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
            New<ILauncher>().Launch(folder);
        }
    }
}