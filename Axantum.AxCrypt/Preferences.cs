#region Coypright and License

/*
 * AxCrypt - Copyright 2014, Svante Seleborg, All Rights Reserved
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

using System;
using System.Drawing;
using System.Linq;
using Axantum.AxCrypt.Core;

namespace Axantum.AxCrypt
{
    internal static class Preferences
    {
        public static int MainWindowWidth { get { return Resolve.UserSettings.Load<int>("MainWindowWidth"); } set { Resolve.UserSettings.Store("MainWindowWidth", value); } }

        public static int MainWindowHeight { get { return Resolve.UserSettings.Load<int>("MainWindowHeight"); } set { Resolve.UserSettings.Store("MainWindowHeight", value); } }

        public static Point MainWindowLocation { get { return new Point(Resolve.UserSettings.Load<int>("MainWindowLocationX"), Resolve.UserSettings.Load<int>("MainWindowLocationY")); } set { Resolve.UserSettings.Store("MainWindowLocationX", value.X); Resolve.UserSettings.Store("MainWindowLocationY", value.Y); } }

        public static int RecentFilesMaxNumber { get { return Resolve.UserSettings.Load<int>("RecentFilesMaxNumber", 250); } set { Resolve.UserSettings.Store("RecentFilesMaxNumber", value); } }

        public static int RecentFilesDocumentWidth { get { return Resolve.UserSettings.Load<int>("RecentFilesDocumentWidth"); } set { Resolve.UserSettings.Store("RecentFilesDocumentWidth", value); } }

        public static int RecentFilesDateTimeWidth { get { return Resolve.UserSettings.Load<int>("RecentFilesDateTimeWidth"); } set { Resolve.UserSettings.Store("RecentFilesDateTimeWidth", value); } }

        public static int RecentFilesEncryptedPathWidth { get { return Resolve.UserSettings.Load<int>("RecentFilesEncryptedPathWidth"); } set { Resolve.UserSettings.Store("RecentFilesEncryptedPathWidth", value); } }

        public static int RecentFilesCryptoNameWidth { get { return Resolve.UserSettings.Load<int>("RecentFilesCryptoNameWidth"); } set { Resolve.UserSettings.Store("RecentFilesCryptoNameWidth", value); } }

        public static bool RecentFilesAscending { get { return Resolve.UserSettings.Load<bool>("RecentFilesAscending", true); } set { Resolve.UserSettings.Store("RecentFilesAscending", value); } }

        public static int RecentFilesSortColumn { get { return Resolve.UserSettings.Load<int>("RecentFilesSortColumn", 0); } set { Resolve.UserSettings.Store("RecentFilesSortColumn", value); } }
    }
}