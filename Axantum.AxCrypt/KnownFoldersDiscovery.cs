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
 * The source is maintained at http://bitbucket.org/axantum/axcrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.UI;
using Axantum.AxCrypt.Properties;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using static Axantum.AxCrypt.Abstractions.TypeResolve;
using Texts = AxCrypt.Content.Texts;

namespace Axantum.AxCrypt
{
    internal static class KnownFoldersDiscovery
    {
        public static IEnumerable<KnownFolder> Discover()
        {
            List<KnownFolder> knownFolders = new List<KnownFolder>();
            if (OS.Current.Platform != Platform.WindowsDesktop)
            {
                return knownFolders;
            }

            CheckDocumentsLibrary(knownFolders);
            CheckDropBox(knownFolders);
            CheckSkyDrive(knownFolders);
            CheckGoogleDrive(knownFolders);

            return knownFolders;
        }

        private static void CheckDocumentsLibrary(IList<KnownFolder> knownFolders)
        {
            Bitmap bitmap = Resources.folder_40px;
            IDataContainer myDocumentsInfo = New<IDataContainer>(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            KnownFolder windowsDesktopFolder = new KnownFolder(myDocumentsInfo, Texts.MyAxCryptFolderName, bitmap, null);
            knownFolders.Add(windowsDesktopFolder);
        }

        private static void CheckDropBox(IList<KnownFolder> knownFolders)
        {
            string dropBoxFolder = Path.Combine(Environment.GetEnvironmentVariable("HOMEDRIVE") + Environment.GetEnvironmentVariable("HOMEPATH"), "DropBox");
            if (!Directory.Exists(dropBoxFolder))
            {
                return;
            }

            Bitmap bitmap = Resources.dropbox_40px;
            IDataContainer dropBoxFolderInfo = New<IDataContainer>(dropBoxFolder);
            KnownFolder knownFolder = new KnownFolder(dropBoxFolderInfo, Texts.MyAxCryptFolderName, bitmap, null);

            knownFolders.Add(knownFolder);
        }

        private static void CheckSkyDrive(IList<KnownFolder> knownFolders)
        {
            string skyDriveFolder = FindOneDriveFolder();
            if (String.IsNullOrEmpty(skyDriveFolder) || !Directory.Exists(skyDriveFolder))
            {
                return;
            }

            Uri url = new Uri("https://onedrive.live.com/");

            Bitmap bitmap = Resources.skydrive_40px;
            IDataContainer skyDriveFolderInfo = New<IDataContainer>(skyDriveFolder);
            KnownFolder knownFolder = new KnownFolder(skyDriveFolderInfo, Texts.MyAxCryptFolderName, bitmap, url);

            knownFolders.Add(knownFolder);
        }

        private static string FindOneDriveFolder()
        {
            string skyDriveFolder = null;

            skyDriveFolder = TryRegistryLocationForOneDriveFolder(@"Software\Microsoft\OneDrive");
            if (skyDriveFolder != null)
            {
                return skyDriveFolder;
            }

            skyDriveFolder = TryRegistryLocationForOneDriveFolder(@"Software\Microsoft\Windows\CurrentVersion\SkyDrive");
            if (skyDriveFolder != null)
            {
                return skyDriveFolder;
            }

            return null;
        }

        private static string TryRegistryLocationForOneDriveFolder(string name)
        {
            RegistryKey skyDriveKey = Registry.CurrentUser.OpenSubKey(name);
            if (skyDriveKey == null)
            {
                return null;
            }

            string skyDriveFolder = skyDriveKey.GetValue("UserFolder") as string;
            if (String.IsNullOrEmpty(skyDriveFolder))
            {
                return null;
            }

            return skyDriveFolder;
        }

        private static void CheckGoogleDrive(IList<KnownFolder> knownFolders)
        {
            string googleDriveFolder = Path.Combine(Environment.GetEnvironmentVariable("HOMEDRIVE") + Environment.GetEnvironmentVariable("HOMEPATH"), "Google Drive");
            if (String.IsNullOrEmpty(googleDriveFolder) || !Directory.Exists(googleDriveFolder))
            {
                return;
            }

            Uri url = new Uri("https://drive.google.com/");

            Bitmap bitmap = Resources.google_40px;
            IDataContainer googleDriveFolderInfo = New<IDataContainer>(googleDriveFolder);
            KnownFolder knownFolder = new KnownFolder(googleDriveFolderInfo, Texts.MyAxCryptFolderName, bitmap, url);
            knownFolders.Add(knownFolder);
        }
    }
}