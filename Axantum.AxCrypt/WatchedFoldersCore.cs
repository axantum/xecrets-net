using Axantum.AxCrypt.Core.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Core.UI;

namespace Axantum.AxCrypt
{
    internal class WatchedFoldersCore
    {
        private IMainView _mainView;

        public WatchedFoldersCore(IMainView mainView)
        {
            _mainView = mainView;
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

        public void EncryptFiles(ProgressContext progress)
        {
            _mainView.FileSystemState.EncryptFilesInWatchedFolders(progress);
        }

        private void AddRemoveWatchedFolders()
        {
            _mainView.WatchedFolders.ListViewItemSorter = null;
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
            _mainView.WatchedFolders.ListViewItemSorter = StringComparer.CurrentCulture;
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