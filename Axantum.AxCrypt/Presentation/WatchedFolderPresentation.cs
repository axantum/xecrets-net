#region Coypright and License

/*
 * AxCrypt - Copyright 2012, Svante Seleborg, All Rights Reserved
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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Axantum.AxCrypt.Presentation
{
    /// <summary>
    /// Handle the presentation logic for Watched Folders. This is not really a Presenter in the MVP-sense, but rather
    /// somewhere to place presentation logic for one aspect of user interface. It is still very much tied to the MainForm,
    /// but it makes it easer to work with the particular code. It's also testable to some degree.
    /// </summary>
    public class WatchedFolderPresentation
    {
        private IMainView _mainView;

        public WatchedFolderPresentation(IMainView mainView)
        {
            _mainView = mainView;
            ShowOrHideWatchedFoldersTabPage();
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

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public void StartDragAndDrop(DragEventArgs e)
        {
            IRuntimeFileInfo droppedFolder = GetDroppedFolderIfAny(e.Data);
            if (droppedFolder == null)
            {
                return;
            }
            e.Effect = (DragDropEffects.Link | DragDropEffects.Copy) & e.AllowedEffect;
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public void DropDragAndDrop(DragEventArgs e)
        {
            IRuntimeFileInfo droppedFolder = GetDroppedFolderIfAny(e.Data);
            if (droppedFolder == null)
            {
                return;
            }
            Instance.FileSystemState.AddWatchedFolder(new WatchedFolder(droppedFolder.FullName, Instance.KnownKeys.DefaultEncryptionKey.Thumbprint));
            Instance.FileSystemState.Save();
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public void DecryptSelectedFolder(string folder, IProgressContext progress)
        {
            Factory.AxCryptFile.DecryptFilesUniqueWithWipeOfOriginal(OS.Current.FileInfo(folder), Instance.KnownKeys.DefaultEncryptionKey, progress);
        }

        private void AddRemoveWatchedFolders()
        {
            _mainView.WatchedFolders.BeginUpdate();
            try
            {
                _mainView.WatchedFolders.Items.Clear();
                if (!Instance.KnownKeys.IsLoggedOn)
                {
                    return;
                }

                foreach (WatchedFolder folder in Instance.KnownKeys.WatchedFolders)
                {
                    string text = folder.Path;
                    ListViewItem item = _mainView.WatchedFolders.Items.Add(text);
                    item.Name = text;
                }
            }
            finally
            {
                _mainView.WatchedFolders.EndUpdate();
            }
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

            if (!fileInfo.NormalizeFolder().IsEncryptable())
            {
                return null;
            }
            return fileInfo;
        }

        public void RemoveSelectedWatchedFolders()
        {
            foreach (string watchedFolderPath in SelectedWatchedFoldersItems())
            {
                Instance.FileSystemState.RemoveWatchedFolder(OS.Current.FileInfo(watchedFolderPath));
            }
            Instance.FileSystemState.Save();
        }

        private IEnumerable<string> SelectedWatchedFoldersItems()
        {
            IEnumerable<string> selected = _mainView.WatchedFolders.SelectedItems.Cast<ListViewItem>().Select((ListViewItem item) => { return item.Text; }).ToArray();
            return selected;
        }

        private TabPage _hiddenWatchedFoldersTabPage;

        private void ShowOrHideWatchedFoldersTabPage()
        {
            TabPage current = _mainView.Tabs.TabPages["_watchedFoldersTabPage"];
            if (Instance.KnownKeys.IsLoggedOn && current == null)
            {
                _mainView.Tabs.TabPages.Add(_hiddenWatchedFoldersTabPage);
                return;
            }
            if (!Instance.KnownKeys.IsLoggedOn && current != null)
            {
                _hiddenWatchedFoldersTabPage = current;
                _mainView.Tabs.TabPages.Remove(_hiddenWatchedFoldersTabPage);
                return;
            }
        }
    }
}