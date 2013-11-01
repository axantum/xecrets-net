using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Axantum.AxCrypt
{
    internal class WatchedFoldersCore
    {
        private IMainView _mainView;

        public WatchedFoldersCore(IMainView mainView)
        {
            _mainView = mainView;
            ShowOrHideWatchedFoldersTabPage();
        }

        private TabPage WatchedFoldersTabPage
        {
            get { return _mainView.Tabs.TabPages["watchedFoldersTabPage"]; }
        }

        public void UpdateListView()
        {
            AddRemoveWatchedFolders();
            ShowOrHideWatchedFoldersTabPage();
        }

        public void OpenSelectedFolder()
        {
            string folder = _mainView.WatchedFolders.SelectedItems[0].Text;
            OS.Current.Launch(folder);
        }

        public static void StartDragAndDrop(DragEventArgs e)
        {
            IRuntimeFileInfo droppedFolder = GetDroppedFolderIfAny(e.Data);
            if (droppedFolder == null)
            {
                return;
            }
            e.Effect = DragDropEffects.Link;
        }

        public void DropDragAndDrop(DragEventArgs e)
        {
            IRuntimeFileInfo droppedFolder = GetDroppedFolderIfAny(e.Data);
            if (droppedFolder == null)
            {
                return;
            }
            _mainView.FileSystemState.AddWatchedFolder(new WatchedFolder(droppedFolder.FullName));
            _mainView.FileSystemState.Save();
        }

        public void DecryptSelectedFolder(string folder, ProgressContext progress)
        {
            AxCryptFile.DecryptFilesUniqueWithWipeOfOriginal(OS.Current.FileInfo(folder), _mainView.FileSystemState.KnownKeys.DefaultEncryptionKey, progress);
        }

        private void AddRemoveWatchedFolders()
        {
            _mainView.WatchedFolders.BeginUpdate();
            foreach (WatchedFolder folder in _mainView.FileSystemState.WatchedFolders)
            {
                ListViewItem item = _mainView.WatchedFolders.Items[folder.Path];
                if (item == null)
                {
                    string text = folder.Path;
                    item = _mainView.WatchedFolders.Items.Add(text);
                    item.Name = text;
                }
            }
            for (int i = 0; i < _mainView.WatchedFolders.Items.Count; ++i)
            {
                if (!_mainView.FileSystemState.WatchedFolders.Contains(new WatchedFolder(_mainView.WatchedFolders.Items[i].Text)))
                {
                    _mainView.WatchedFolders.Items.RemoveAt(i);
                    --i;
                }
            }
            _mainView.WatchedFolders.EndUpdate();
        }

        private static IRuntimeFileInfo GetDroppedFolderIfAny(IDataObject dataObject)
        {
            IList<string> dropped = dataObject.GetData(DataFormats.FileDrop) as IList<string>;
            if (dropped == null)
            {
                return null;
            }
            if (dropped.Count != 1)
            {
                return null;
            }
            IRuntimeFileInfo fileInfo = OS.Current.FileInfo(dropped[0]);
            if (!fileInfo.IsFolder)
            {
                return null;
            }
            return fileInfo;
        }

        public void RemoveSelectedWatchedFolders()
        {
            foreach (string watchedFolderPath in SelectedWatchedFoldersItems())
            {
                _mainView.FileSystemState.RemoveWatchedFolder(new WatchedFolder(watchedFolderPath));
            }
            _mainView.FileSystemState.Save();
        }

        private IEnumerable<string> SelectedWatchedFoldersItems()
        {
            IEnumerable<string> selected = _mainView.WatchedFolders.SelectedItems.Cast<ListViewItem>().Select((ListViewItem item) => { return item.Text; }).ToArray();
            return selected;
        }

        private TabPage _watchedFoldersTabPage;

        private void ShowOrHideWatchedFoldersTabPage()
        {
            if (_mainView.FileSystemState.KnownKeys.DefaultEncryptionKey != null && WatchedFoldersTabPage == null)
            {
                _mainView.Tabs.TabPages.Add(_watchedFoldersTabPage);
                _watchedFoldersTabPage = null;
                return;
            }
            if (_mainView.FileSystemState.KnownKeys.DefaultEncryptionKey == null && WatchedFoldersTabPage != null)
            {
                _watchedFoldersTabPage = WatchedFoldersTabPage;
                _mainView.Tabs.TabPages.Remove(WatchedFoldersTabPage);
                return;
            }
        }
    }
}