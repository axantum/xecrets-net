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
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.UI;
using Axantum.AxCrypt.Properties;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Axantum.AxCrypt.Presentation
{
    public class RecentFilesPresentation
    {
        private class RecentFilesByDateComparer : IComparer
        {
            private SortOrder _sortOrder;

            public RecentFilesByDateComparer(SortOrder sortOrder)
            {
                _sortOrder = sortOrder;
            }

            #region IComparer Members

            public int Compare(object x, object y)
            {
                ListViewItem item1 = (ListViewItem)x;
                ListViewItem item2 = (ListViewItem)y;
                DateTime dateTime1 = DateFromSubItem(item1.SubItems["Date"]);
                DateTime dateTime2 = DateFromSubItem(item2.SubItems["Date"]);

                return dateTime1.CompareTo(dateTime2) * (_sortOrder == SortOrder.Ascending ? 1 : -1);
            }

            #endregion IComparer Members

            private static DateTime DateFromSubItem(ListViewItem.ListViewSubItem subItem)
            {
                if (subItem == null || subItem.Tag == null)
                {
                    return DateTime.MinValue;
                }
                return (DateTime)subItem.Tag;
            }
        }

        private class RecentFilesByDecryptedFileNameComparer : IComparer
        {
            private SortOrder _sortOrder;

            public RecentFilesByDecryptedFileNameComparer(SortOrder sortOrder)
            {
                _sortOrder = sortOrder;
            }

            public int Compare(object x, object y)
            {
                ListViewItem item1 = (ListViewItem)x;
                ListViewItem item2 = (ListViewItem)y;

                return StringComparer.OrdinalIgnoreCase.Compare(item1.Text, item2.Text) * (_sortOrder == SortOrder.Ascending ? 1 : -1);
            }
        }

        private class RecentFilesByEncryptedPathComparer : IComparer
        {
            private SortOrder _sortOrder;

            public RecentFilesByEncryptedPathComparer(SortOrder sortOrder)
            {
                _sortOrder = sortOrder;
            }

            public int Compare(object x, object y)
            {
                ListViewItem item1 = (ListViewItem)x;
                ListViewItem item2 = (ListViewItem)y;
                string path1 = item1.SubItems["EncryptedPath"].Text;
                string path2 = item2.SubItems["EncryptedPath"].Text;

                return StringComparer.OrdinalIgnoreCase.Compare(path1, path2) * (_sortOrder == SortOrder.Ascending ? 1 : -1);
            }
        }

        private IMainView _mainView;

        private ListViewActions Actions { get { return new ListViewActions(_mainView.RecentFiles); } }

        public RecentFilesPresentation(IMainView mainView)
        {
            _mainView = mainView;

            _mainView.RecentFiles.SmallImageList = CreateSmallImageListToAvoidLocalizationIssuesWithDesignerAndResources();
            _mainView.RecentFiles.LargeImageList = CreateLargeImageListToAvoidLocalizationIssuesWithDesignerAndResources();

            RestoreUserPreferences();
        }

        private void RestoreUserPreferences()
        {
            _mainView.RecentFiles.Columns[0].Width = Instance.FileSystemState.Settings.RecentFilesDocumentWidth > 0 ? Instance.FileSystemState.Settings.RecentFilesDocumentWidth : _mainView.RecentFiles.Columns[0].Width;
            _mainView.RecentFiles.Columns[1].Width = Instance.FileSystemState.Settings.RecentFilesDateTimeWidth > 0 ? Instance.FileSystemState.Settings.RecentFilesDateTimeWidth : _mainView.RecentFiles.Columns[1].Width;
            _mainView.RecentFiles.Columns[2].Width = Instance.FileSystemState.Settings.RecentFilesEncryptedPathWidth > 0 ? Instance.FileSystemState.Settings.RecentFilesEncryptedPathWidth : _mainView.RecentFiles.Columns[2].Width;

            _mainView.RecentFiles.Sorting = Instance.FileSystemState.Settings.RecentFilesAscending ? SortOrder.Ascending : SortOrder.Descending;
            SetSorter(Instance.FileSystemState.Settings.RecentFilesSortColumn, _mainView.RecentFiles.Sorting);
        }

        private void SetSorter(int column, SortOrder sortOrder)
        {
            switch (column)
            {
                case 0:
                    _mainView.RecentFiles.ListViewItemSorter = new RecentFilesByDecryptedFileNameComparer(sortOrder);
                    break;

                case 1:
                    _mainView.RecentFiles.ListViewItemSorter = new RecentFilesByDateComparer(sortOrder);
                    break;

                case 2:
                    _mainView.RecentFiles.ListViewItemSorter = new RecentFilesByEncryptedPathComparer(sortOrder);
                    break;
            }
        }

        public void UpdateActiveFilesViews(ActiveFile activeFile)
        {
            if (activeFile.Status.HasMask(ActiveFileStatus.NoLongerActive))
            {
                _mainView.RecentFiles.Items.RemoveByKey(activeFile.EncryptedFileInfo.FullName);
                return;
            }

            UpdateRecentFilesListView(activeFile);
        }

        public string SelectedEncryptedPath
        {
            get
            {
                string encryptedPath = _mainView.RecentFiles.SelectedItems[0].SubItems["EncryptedPath"].Text; //MLHIDE
                return encryptedPath;
            }
        }

        public void ChangeColumnWidth(int columnIndex)
        {
            ChangeColumnWidth(_mainView.RecentFiles, columnIndex);
        }

        public void ShowContextMenu(ToolStripDropDown contextMenu, MouseEventArgs e)
        {
            Actions.ShowContextMenu(contextMenu, e);
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public void StartDragAndDrop(DragEventArgs e)
        {
            IEnumerable<IRuntimeFileInfo> droppedFiles = GetDroppedFiles(e.Data);
            if (!droppedFiles.Any(fileInfo => fileInfo.Type() == FileInfoType.EncryptedFile || (Instance.KnownKeys.DefaultEncryptionKey != null && fileInfo.Type() == FileInfoType.EncryptableFile)))
            {
                return;
            }
            e.Effect = DragDropEffects.Link;
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public void DropDragAndDrop(DragEventArgs e)
        {
            IEnumerable<IRuntimeFileInfo> droppedFiles = GetDroppedFiles(e.Data);

            IEnumerable<IRuntimeFileInfo> encryptableFiles = droppedFiles.Where(fileInfo => Instance.KnownKeys.DefaultEncryptionKey != null && fileInfo.Type() == FileInfoType.EncryptableFile);
            ProcessEncryptableFilesDroppedInRecentList(encryptableFiles);

            IEnumerable<IRuntimeFileInfo> encryptedFiles = droppedFiles.Where(fileInfo => fileInfo.Type() == FileInfoType.EncryptedFile);
            ProcessEncryptedFilesDroppedInRecentList(encryptedFiles);
        }

        internal void ColumnClick(int column)
        {
            SetSorterOrOrder(column);
            Instance.FileSystemState.Save();
        }

        private void SetSorterOrOrder(int column)
        {
            if (column == Instance.FileSystemState.Settings.RecentFilesSortColumn)
            {
                _mainView.RecentFiles.Sorting = _mainView.RecentFiles.Sorting == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
                Instance.FileSystemState.Settings.RecentFilesAscending = _mainView.RecentFiles.Sorting == SortOrder.Ascending;
            }

            SetSorter(column, _mainView.RecentFiles.Sorting);
            Instance.FileSystemState.Settings.RecentFilesSortColumn = column;
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        private void ProcessEncryptedFilesDroppedInRecentList(IEnumerable<IRuntimeFileInfo> encryptedFiles)
        {
        }

        private void ProcessEncryptableFilesDroppedInRecentList(IEnumerable<IRuntimeFileInfo> encryptableFiles)
        {
            Instance.Background.ProcessFiles(encryptableFiles.Select(fileInfo => fileInfo.FullName), EncryptFile);
        }

        private void EncryptFile(string fullName, IThreadWorker worker, ProgressContext progress)
        {
            FileOperationsController operationsController = new FileOperationsController(Instance.FileSystemState, progress);

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
                    ActiveFile activeFile = new ActiveFile(encryptedInfo, decryptedInfo, e.Key, ActiveFileStatus.NotDecrypted, null);
                    Instance.FileSystemState.Add(activeFile);
                    Instance.FileSystemState.Save();
                }
            };

            operationsController.EncryptFile(fullName, worker);
        }

        private static IEnumerable<IRuntimeFileInfo> GetDroppedFiles(IDataObject dataObject)
        {
            IList<string> dropped = dataObject.GetData(DataFormats.FileDrop) as IList<string>;
            if (dropped == null)
            {
                return new IRuntimeFileInfo[0];
            }

            return dropped.Select(path => OS.Current.FileInfo(path));
        }

        private static void ChangeColumnWidth(ListView listView, int columnIndex)
        {
            switch (columnIndex)
            {
                case 0:
                    Instance.FileSystemState.Settings.RecentFilesDocumentWidth = listView.Columns[columnIndex].Width;
                    break;

                case 1:
                    Instance.FileSystemState.Settings.RecentFilesDateTimeWidth = listView.Columns[columnIndex].Width;
                    break;

                case 2:
                    Instance.FileSystemState.Settings.RecentFilesEncryptedPathWidth = listView.Columns[columnIndex].Width;
                    break;
            }
            Instance.FileSystemState.Save();
        }

        private void UpdateRecentFilesListView(ActiveFile activeFile)
        {
            _mainView.RecentFiles.BeginUpdate();
            ListViewItem item = _mainView.RecentFiles.Items[activeFile.EncryptedFileInfo.FullName];
            if (item == null)
            {
                string text = Path.GetFileName(activeFile.DecryptedFileInfo.FullName);
                item = new ListViewItem(text);
                item.Name = activeFile.EncryptedFileInfo.FullName;

                ListViewItem.ListViewSubItem dateColumn = item.SubItems.Add(String.Empty);
                dateColumn.Name = "Date"; //MLHIDE

                ListViewItem.ListViewSubItem encryptedPathColumn = item.SubItems.Add(String.Empty);
                encryptedPathColumn.Name = "EncryptedPath"; //MLHIDE

                _mainView.RecentFiles.Items.Add(item);
            }

            UpdateListViewItem(item, activeFile);
            while (_mainView.RecentFiles.Items.Count > Instance.FileSystemState.Settings.RecentFilesMaxNumber)
            {
                _mainView.RecentFiles.Items.RemoveAt(_mainView.RecentFiles.Items.Count - 1);
            }
            _mainView.RecentFiles.EndUpdate();
        }

        private static void UpdateListViewItem(ListViewItem item, ActiveFile activeFile)
        {
            UpdateStatusDependentPropertiesOfListViewItem(item, activeFile);

            item.SubItems["EncryptedPath"].Text = activeFile.EncryptedFileInfo.FullName;
            item.SubItems["Date"].Text = activeFile.LastActivityTimeUtc.ToLocalTime().ToString(CultureInfo.CurrentCulture);
            item.SubItems["Date"].Tag = activeFile.LastActivityTimeUtc;
        }

        private static void UpdateStatusDependentPropertiesOfListViewItem(ListViewItem item, ActiveFile activeFile)
        {
            switch (activeFile.VisualState)
            {
                case ActiveFileVisualState.DecryptedWithKnownKey:
                    item.ImageKey = "DecryptedFile";
                    item.ToolTipText = Resources.DecryptedFileToolTip;
                    break;

                case ActiveFileVisualState.DecryptedWithoutKnownKey:
                    item.ImageKey = "DecryptedUnknownKeyFile";
                    item.ToolTipText = Resources.DecryptedUnknownKeyFileToolTip;
                    break;

                case ActiveFileVisualState.EncryptedNeverBeenDecrypted:
                    item.ImageKey = "InactiveFile";
                    item.ToolTipText = Resources.InactiveFileToolTip;
                    break;

                case ActiveFileVisualState.EncryptedWithoutKnownKey:
                    item.ImageKey = "ActiveFile";
                    item.ToolTipText = Resources.ActiveFileToolTip;
                    break;

                case ActiveFileVisualState.EncryptedWithKnownKey:
                    item.ImageKey = "ActiveFileKnownKey";
                    item.ToolTipText = Resources.ActiveFileKnownKeyToolTip;
                    break;

                default:
                    throw new InvalidOperationException("Unexpected ActiveFileVisualState value.");
            }
        }

        private ImageList CreateSmallImageListToAvoidLocalizationIssuesWithDesignerAndResources()
        {
            ImageList smallImageList = new ImageList(_mainView.Components);

            smallImageList.Images.Add("ActiveFile", Resources.activefilegreen16);
            smallImageList.Images.Add("InactiveFile", Resources.inactivefilegreen16);
            smallImageList.Images.Add("Exclamation", Resources.exclamationgreen16);
            smallImageList.Images.Add("DecryptedFile", Resources.decryptedfilered16);
            smallImageList.Images.Add("DecryptedUnknownKeyFile", Resources.decryptedunknownkeyfilered16);
            smallImageList.Images.Add("ActiveFileKnownKey", Resources.fileknownkeygreen16);
            smallImageList.TransparentColor = System.Drawing.Color.Transparent;

            return smallImageList;
        }

        private ImageList CreateLargeImageListToAvoidLocalizationIssuesWithDesignerAndResources()
        {
            ImageList largeImageList = new ImageList(_mainView.Components);

            largeImageList.Images.Add("ActiveFile", Resources.opendocument32);
            largeImageList.Images.Add("InactiveFile", Resources.helpquestiongreen32);
            largeImageList.Images.Add("Exclamation", Resources.exclamationgreen32);
            largeImageList.TransparentColor = System.Drawing.Color.Transparent;

            return largeImageList;
        }
    }
}