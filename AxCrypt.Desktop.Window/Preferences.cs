#region Coypright and License

/*
 * AxCrypt - Copyright 2016, Svante Seleborg, All Rights Reserved
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
 * The source is maintained at http://bitbucket.org/AxCrypt.Desktop.Window-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using AxCrypt.Core;
using System.Drawing;

namespace AxCrypt.Desktop.Window
{
    internal static class Preferences
    {
        public static int MainWindowWidth
        { get { return Resolve.UserSettings.Load<int>(nameof(MainWindowWidth)); } set { Resolve.UserSettings.Store(nameof(MainWindowWidth), value); } }

        public static int MainWindowHeight
        { get { return Resolve.UserSettings.Load<int>(nameof(MainWindowHeight)); } set { Resolve.UserSettings.Store(nameof(MainWindowHeight), value); } }

        public static Point MainWindowLocation
        { get { return new Point(Resolve.UserSettings.Load<int>(nameof(MainWindowLocation)), Resolve.UserSettings.Load<int>(nameof(MainWindowLocation))); } set { Resolve.UserSettings.Store("MainWindowLocationX", value.X); Resolve.UserSettings.Store("MainWindowLocationY", value.Y); } }

        public static int RecentFilesMaxNumber
        { get { return Resolve.UserSettings.Load<int>(nameof(RecentFilesMaxNumber), 250); } set { Resolve.UserSettings.Store(nameof(RecentFilesMaxNumber), value); } }

        public static int RecentFilesSizeWidth
        { get { return Resolve.UserSettings.Load<int>(nameof(RecentFilesSizeWidth)); } set { Resolve.UserSettings.Store(nameof(RecentFilesSizeWidth), value); } }

        public static int RecentFilesDocumentWidth
        { get { return Resolve.UserSettings.Load<int>(nameof(RecentFilesDocumentWidth)); } set { Resolve.UserSettings.Store(nameof(RecentFilesDocumentWidth), value); } }

        public static int RecentFilesAccessedDateWidth
        { get { return Resolve.UserSettings.Load<int>(nameof(RecentFilesAccessedDateWidth)); } set { Resolve.UserSettings.Store(nameof(RecentFilesAccessedDateWidth), value); } }

        public static int RecentFilesEncryptedPathWidth
        { get { return Resolve.UserSettings.Load<int>(nameof(RecentFilesEncryptedPathWidth)); } set { Resolve.UserSettings.Store(nameof(RecentFilesEncryptedPathWidth), value); } }

        public static int RecentFilesCryptoNameWidth
        { get { return Resolve.UserSettings.Load<int>(nameof(RecentFilesCryptoNameWidth)); } set { Resolve.UserSettings.Store(nameof(RecentFilesCryptoNameWidth), value); } }

        public static int RecentFilesModifiedDateWidth
        { get { return Resolve.UserSettings.Load<int>(nameof(RecentFilesModifiedDateWidth)); } set { Resolve.UserSettings.Store(nameof(RecentFilesModifiedDateWidth), value); } }

        public static bool RecentFilesAscending
        { get { return Resolve.UserSettings.Load<bool>(nameof(RecentFilesAscending), true); } set { Resolve.UserSettings.Store(nameof(RecentFilesAscending), value); } }

        public static int RecentFilesSortColumn
        { get { return Resolve.UserSettings.Load<int>(nameof(RecentFilesSortColumn), 0); } set { Resolve.UserSettings.Store(nameof(RecentFilesSortColumn), value); } }
    }
}