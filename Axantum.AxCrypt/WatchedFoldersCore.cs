using Axantum.AxCrypt.Core.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Axantum.AxCrypt
{
    internal class WatchedFoldersCore
    {
        private ListView _listView;

        private FileSystemState _fileSystemState;

        public WatchedFoldersCore(ListView listView, FileSystemState fileSystemState)
        {
            _listView = listView;
            _fileSystemState = fileSystemState;
        }

        public void HandleWatchedFolderChanged(object sender, WatchedFolderChangedEventArgs e)
        {
            _listView.ListViewItemSorter = null;
            UpdateWatchedFoldersListView();
            _listView.ListViewItemSorter = StringComparer.CurrentCulture;
        }

        private void UpdateWatchedFoldersListView()
        {
            foreach (WatchedFolder folder in _fileSystemState.WatchedFolders)
            {
                ListViewItem item = _listView.Items[folder.Path];
                if (item == null)
                {
                    string text = folder.Path;
                    item = _listView.Items.Add(text);
                    item.Name = text;
                }
            }
        }
    }
}