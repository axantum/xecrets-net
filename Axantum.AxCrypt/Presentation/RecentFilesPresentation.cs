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

using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Properties;
using System;
using System.Linq;
using System.Windows.Forms;

namespace Axantum.AxCrypt.Presentation
{
    public class RecentFilesPresentation
    {
        private ListView _recentFiles;

        public RecentFilesPresentation(ListView recentFiles)
        {
            _recentFiles = recentFiles;

            _recentFiles.SmallImageList = CreateSmallImageListToAvoidLocalizationIssuesWithDesignerAndResources();
            _recentFiles.LargeImageList = CreateLargeImageListToAvoidLocalizationIssuesWithDesignerAndResources();

            RestoreUserPreferences();
        }

        private void RestoreUserPreferences()
        {
            _recentFiles.Columns[0].Width = Preferences.RecentFilesDocumentWidth.Fallback(_recentFiles.Columns[0].Width);
            _recentFiles.Columns[1].Width = Preferences.RecentFilesDateTimeWidth.Fallback(_recentFiles.Columns[1].Width);
            _recentFiles.Columns[2].Width = Preferences.RecentFilesEncryptedPathWidth.Fallback(_recentFiles.Columns[2].Width);
        }

        public void ChangeColumnWidth(int columnIndex)
        {
            ChangeColumnWidth(_recentFiles, columnIndex);
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
            ImageList smallImageList = new ImageList();

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
            ImageList largeImageList = new ImageList();

            largeImageList.Images.Add("ActiveFile", Resources.opendocument32);
            largeImageList.Images.Add("InactiveFile", Resources.helpquestiongreen32);
            largeImageList.Images.Add("Exclamation", Resources.exclamationgreen32);
            largeImageList.TransparentColor = System.Drawing.Color.Transparent;

            return largeImageList;
        }
    }
}