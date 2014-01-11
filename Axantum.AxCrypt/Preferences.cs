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

using Axantum.AxCrypt.Core;
using System;
using System.Drawing;
using System.Linq;

namespace Axantum.AxCrypt
{
    internal static class Preferences
    {
        public static int MainWindowWidth { get { return Instance.UserSettings.Load<int>("MainWindowWidth"); } set { Instance.UserSettings.Store("MainWindowWidth", value); } }

        public static int MainWindowHeight { get { return Instance.UserSettings.Load<int>("MainWindowHeight"); } set { Instance.UserSettings.Store("MainWindowHeight", value); } }

        public static Point MainWindowLocation { get { return new Point(Instance.UserSettings.Load<int>("MainWindowLocationX"), Instance.UserSettings.Load<int>("MainWindowLocationY")); } set { Instance.UserSettings.Store("MainWindowLocationX", value.X); Instance.UserSettings.Store("MainWindowLocationY", value.Y); } }

        public static int RecentFilesMaxNumber { get { return Instance.UserSettings.Load<int>("RecentFilesMaxNumber", 250); } set { Instance.UserSettings.Store("RecentFilesMaxNumber", value); } }

        public static int RecentFilesDocumentWidth { get { return Instance.UserSettings.Load<int>("RecentFilesDocumentWidth"); } set { Instance.UserSettings.Store("RecentFilesDocumentWidth", value); } }

        public static int RecentFilesDateTimeWidth { get { return Instance.UserSettings.Load<int>("RecentFilesDateTimeWidth"); } set { Instance.UserSettings.Store("RecentFilesDateTimeWidth", value); } }

        public static int RecentFilesEncryptedPathWidth { get { return Instance.UserSettings.Load<int>("RecentFilesEncryptedPathWidth"); } set { Instance.UserSettings.Store("RecentFilesEncryptedPathWidth", value); } }

        public static bool RecentFilesAscending { get { return Instance.UserSettings.Load<bool>("RecentFilesAscending", true); } set { Instance.UserSettings.Store("RecentFilesAscending", value); } }

        public static int RecentFilesSortColumn { get { return Instance.UserSettings.Load<int>("RecentFilesSortColumn", 0); } set { Instance.UserSettings.Store("RecentFilesSortColumn", value); } }
    }
}