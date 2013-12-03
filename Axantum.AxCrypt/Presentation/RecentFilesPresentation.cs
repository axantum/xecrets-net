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

        public RecentFilesPresentation(IMainView mainView)
        {
            _mainView = mainView;

            _mainView.RecentFiles.SmallImageList = CreateSmallImageListToAvoidLocalizationIssuesWithDesignerAndResources();
            _mainView.RecentFiles.LargeImageList = CreateLargeImageListToAvoidLocalizationIssuesWithDesignerAndResources();

            RestoreUserPreferences();
        }

        private void RestoreUserPreferences()
        {
            _mainView.RecentFiles.Columns[0].Width = Preferences.RecentFilesDocumentWidth.Fallback(_mainView.RecentFiles.Columns[0].Width);
            _mainView.RecentFiles.Columns[1].Width = Preferences.RecentFilesDateTimeWidth.Fallback(_mainView.RecentFiles.Columns[1].Width);
            _mainView.RecentFiles.Columns[2].Width = Preferences.RecentFilesEncryptedPathWidth.Fallback(_mainView.RecentFiles.Columns[2].Width);

            _mainView.RecentFiles.Sorting = Preferences.RecentFilesAscending ? SortOrder.Ascending : SortOrder.Descending;
            SetSorter(Preferences.RecentFilesSortColumn, _mainView.RecentFiles.Sorting);
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

        public void ChangeColumnWidth(int columnIndex)
        {
            ChangeColumnWidth(_mainView.RecentFiles, columnIndex);
        }

        internal void ColumnClick(int column)
        {
            SetSorterOrOrder(column);
        }

        private void SetSorterOrOrder(int column)
        {
            if (column == Preferences.RecentFilesSortColumn)
            {
                _mainView.RecentFiles.Sorting = _mainView.RecentFiles.Sorting == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
                Preferences.RecentFilesAscending = _mainView.RecentFiles.Sorting == SortOrder.Ascending;
            }

            SetSorter(column, _mainView.RecentFiles.Sorting);
            Preferences.RecentFilesSortColumn = column;
        }

        private static void ChangeColumnWidth(ListView listView, int columnIndex)
        {
            switch (columnIndex)
            {
                case 0:
                    Preferences.RecentFilesDocumentWidth = listView.Columns[columnIndex].Width;
                    break;

                case 1:
                    Preferences.RecentFilesDateTimeWidth = listView.Columns[columnIndex].Width;
                    break;

                case 2:
                    Preferences.RecentFilesEncryptedPathWidth = listView.Columns[columnIndex].Width;
                    break;
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