using Axantum.AxCrypt.Core.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Axantum.AxCrypt.Core;

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

        public void UpdateWatchedFoldersListView()
        {
            _listView.ListViewItemSorter = null;
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
            _listView.ListViewItemSorter = StringComparer.CurrentCulture;
        }
    }
}