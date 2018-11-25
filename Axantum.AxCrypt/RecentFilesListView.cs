using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Forms;
using Axantum.AxCrypt.Forms.Style;
using Axantum.AxCrypt.Properties;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

using Texts = AxCrypt.Content.Texts;

namespace Axantum.AxCrypt
{
    public class RecentFilesListView : ListView
    {
        private enum ImageKey
        {
            ActiveFile,
            Exclamation,
            DecryptedFile,
            DecryptedUnknownKeyFile,
            ActiveFileKnownKey,
            CleanUpNeeded,
            KeyShared,
            LowEncryption,
        }

        private enum ColumnName
        {
            DocumentName,
            Date,
            EncryptedPath,
            CryptoName,
        }

        public RecentFilesListView()
        {
            DoubleBuffered = true;
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();

            if (DesignMode)
            {
                return;
            }

            SmallImageList = CreateSmallImageListToAvoidLocalizationIssuesWithDesignerAndResources();
            LargeImageList = CreateLargeImageListToAvoidLocalizationIssuesWithDesignerAndResources();

            ColumnWidthChanged += RecentFilesListView_ColumnWidthChanged;
            RestoreUserPreferences();
        }

        private void RecentFilesListView_ColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
        {
            switch (e.ColumnIndex)
            {
                case 0:
                    Preferences.RecentFilesDocumentWidth = Columns[e.ColumnIndex].Width;
                    break;

                case 1:
                    Preferences.RecentFilesDateTimeWidth = Columns[e.ColumnIndex].Width;
                    break;

                case 2:
                    Preferences.RecentFilesEncryptedPathWidth = Columns[e.ColumnIndex].Width;
                    break;

                case 3:
                    Preferences.RecentFilesCryptoNameWidth = Columns[e.ColumnIndex].Width;
                    break;
            }
        }

        private void RestoreUserPreferences()
        {
            Preferences.RecentFilesMaxNumber = 100;

            Columns[0].Width = Preferences.RecentFilesDocumentWidth.Fallback(Columns[0].Width);
            Columns[1].Width = Preferences.RecentFilesDateTimeWidth.Fallback(Columns[1].Width);
            Columns[2].Width = Preferences.RecentFilesEncryptedPathWidth.Fallback(Columns[2].Width);
            Columns[3].Width = Preferences.RecentFilesCryptoNameWidth.Fallback(Columns[3].Width);
        }

        private bool _updateRecentFilesInProgress = false;

        public void UpdateRecentFiles(IEnumerable<ActiveFile> files)
        {
            if (_updateRecentFilesInProgress)
            {
                return;
            }
            _updateRecentFilesInProgress = true;
            this.WithWaitCursor(() => UpdateRecentFilesUnsynchronized(files), () => _updateRecentFilesInProgress = false);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void UpdateRecentFilesUnsynchronized(IEnumerable<ActiveFile> files)
        {
            BeginUpdate();
            try
            {
                Dictionary<string, int> currentFiles = RemoveRemovedFilesFromRecent(files);
                if (NeedClearItems(currentFiles, files))
                {
                    currentFiles.Clear();
                    Items.Clear();
                }

                int i = 0;
                foreach (ActiveFile file in files)
                {
                    try
                    {
                        UpdateOneItem(currentFiles, file);
                        ++i;
                    }
                    catch (Exception ex)
                    {
                        ex.ReportAndDisplay();
                    }
                }

                while (Items.Count > Preferences.RecentFilesMaxNumber)
                {
                    Items.RemoveAt(Items.Count - 1);
                }
            }
            finally
            {
                EndUpdate();
            }
        }

        private static bool NeedClearItems(Dictionary<string, int> currentFiles, IEnumerable<ActiveFile> files)
        {
            if (files.Count() != currentFiles.Count)
            {
                return true;
            }

            int index = 0;
            foreach (ActiveFile file in files)
            {
                int i;
                if (!currentFiles.TryGetValue(file.EncryptedFileInfo.FullName, out i))
                {
                    return true;
                }
                if (index != i)
                {
                    return true;
                }
                ++index;
            }

            return false;
        }

        private void UpdateOneItem(Dictionary<string, int> currentFiles, ActiveFile file)
        {
            string text = Path.GetFileName(file.DecryptedFileInfo.FullName);
            ListViewItem item = new ListViewItem(text);
            item.UseItemStyleForSubItems = false;
            item.Name = file.EncryptedFileInfo.FullName;

            ListViewItem.ListViewSubItem dateColumn = item.SubItems.Add(String.Empty);
            dateColumn.Name = nameof(ColumnName.Date);

            ListViewItem.ListViewSubItem encryptedPathColumn = item.SubItems.Add(String.Empty);
            encryptedPathColumn.Name = nameof(ColumnName.EncryptedPath);

            ListViewItem.ListViewSubItem cryptoNameColumn = item.SubItems.Add(String.Empty);
            cryptoNameColumn.Name = nameof(ColumnName.CryptoName);

            UpdateListViewItem(item, file);

            int i;
            if (!currentFiles.TryGetValue(item.Name, out i))
            {
                Items.Add(item);
                return;
            }

            if (!CompareRecentFileItem(item, Items[i]))
            {
                Items[i] = item;
            }
        }

        private Dictionary<string, int> RemoveRemovedFilesFromRecent(IEnumerable<ActiveFile> files)
        {
            HashSet<string> newFiles = new HashSet<string>(files.Select(f => f.EncryptedFileInfo.FullName));
            Dictionary<string, int> currentFiles = new Dictionary<string, int>();
            for (int i = 0; i < Items.Count;)
            {
                if (!newFiles.Contains(Items[i].Name))
                {
                    Items.RemoveAt(i);
                    continue;
                }
                currentFiles.Add(Items[i].Name, i);
                ++i;
            }

            return currentFiles;
        }

        private static bool CompareRecentFileItem(ListViewItem left, ListViewItem right)
        {
            if (left.SubItems[nameof(ColumnName.EncryptedPath)].Text != right.SubItems[nameof(ColumnName.EncryptedPath)].Text)
            {
                return false;
            }
            if (left.SubItems[nameof(ColumnName.CryptoName)].Text != right.SubItems[nameof(ColumnName.CryptoName)].Text)
            {
                return false;
            }
            if (left.SubItems[nameof(ColumnName.CryptoName)].ForeColor != right.SubItems[nameof(ColumnName.CryptoName)].ForeColor)
            {
                return false;
            }
            if (left.ImageKey != right.ImageKey)
            {
                return false;
            }
            if (left.SubItems[nameof(ColumnName.Date)].Text != right.SubItems[nameof(ColumnName.Date)].Text)
            {
                return false;
            }
            if ((DateTime)left.SubItems[nameof(ColumnName.Date)].Tag != (DateTime)right.SubItems[nameof(ColumnName.Date)].Tag)
            {
                return false;
            }
            return true;
        }

        public static string EncryptedPath(ListViewItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            return item.SubItems[nameof(ColumnName.EncryptedPath)].Text;
        }

        private static void UpdateListViewItem(ListViewItem item, ActiveFile activeFile)
        {
            OpenFileProperties openProperties = OpenFileProperties.Create(activeFile.EncryptedFileInfo);
            item.SubItems[nameof(ColumnName.EncryptedPath)].Text = activeFile.EncryptedFileInfo.FullName;
            item.SubItems[nameof(ColumnName.Date)].Text = activeFile.Properties.LastActivityTimeUtc.ToLocalTime().ToString(CultureInfo.CurrentCulture);
            item.SubItems[nameof(ColumnName.Date)].Tag = activeFile.Properties.LastActivityTimeUtc;

            UpdateStatusDependentPropertiesOfListViewItem(item, activeFile, openProperties.IsShared);

            try
            {
                if (activeFile.Properties.CryptoId != Guid.Empty)
                {
                    item.SubItems[nameof(ColumnName.CryptoName)].Text = Resolve.CryptoFactory.Create(activeFile.Properties.CryptoId).Name;
                    if (activeFile.VisualState.HasFlag(ActiveFileVisualStates.LowEncryption))
                    {
                        item.SubItems[nameof(ColumnName.CryptoName)].ForeColor = Styling.WarningColor;
                    }
                }
            }
            catch (ArgumentException aex)
            {
                New<IReport>().Exception(aex);
                item.SubItems[nameof(ColumnName.CryptoName)].Text = Texts.UnknownCrypto;
            }
        }

        private static void UpdateStatusDependentPropertiesOfListViewItem(ListViewItem item, ActiveFile activeFile, bool isShared)
        {
            if (activeFile.IsDecrypted)
            {
                item.ImageKey = nameof(ImageKey.CleanUpNeeded);
                item.ToolTipText = Texts.CleanUpNeededToolTip;
                return;
            }

            if (isShared)
            {
                item.ImageKey = nameof(ImageKey.KeyShared);
                item.ToolTipText = Texts.KeySharingExistsToolTip;
                return;
            }

            item.ImageKey = String.Empty;
            item.ToolTipText = Texts.DoubleClickToOpenToolTip;
            return;
        }

        private static ImageList CreateSmallImageListToAvoidLocalizationIssuesWithDesignerAndResources()
        {
            ImageList smallImageList = new ImageList();

            smallImageList.Images.Add(nameof(ImageKey.ActiveFile), Resources.activefilegreen16);
            smallImageList.Images.Add(nameof(ImageKey.Exclamation), Resources.exclamationgreen16);
            smallImageList.Images.Add(nameof(ImageKey.DecryptedFile), Resources.decryptedfilered16);
            smallImageList.Images.Add(nameof(ImageKey.DecryptedUnknownKeyFile), Resources.decryptedunknownkeyfilered16);
            smallImageList.Images.Add(nameof(ImageKey.ActiveFileKnownKey), Resources.fileknownkeygreen16);
            smallImageList.Images.Add(nameof(ImageKey.CleanUpNeeded), Resources.clean_broom_red);
            smallImageList.Images.Add(nameof(ImageKey.KeyShared), Resources.share_32px);
            smallImageList.TransparentColor = System.Drawing.Color.Transparent;

            return smallImageList;
        }

        private static ImageList CreateLargeImageListToAvoidLocalizationIssuesWithDesignerAndResources()
        {
            ImageList largeImageList = new ImageList();

            largeImageList.Images.Add(nameof(ImageKey.ActiveFile), Resources.opendocument32);
            largeImageList.Images.Add(nameof(ImageKey.Exclamation), Resources.exclamationgreen32);
            largeImageList.TransparentColor = System.Drawing.Color.Transparent;

            return largeImageList;
        }
    }
}