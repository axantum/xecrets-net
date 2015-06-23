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
            Icon icon = Resources.DocumentsLibrary;
            IDataContainer myDocumentsInfo = TypeMap.Resolve.New<IDataContainer>(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            KnownFolder windowsDesktopFolder = new KnownFolder(myDocumentsInfo, Resources.MyAxCryptFolderName, icon.ToBitmap(), null);
            knownFolders.Add(windowsDesktopFolder);
        }

        private static void CheckDropBox(IList<KnownFolder> knownFolders)
        {
            RegistryKey dropBoxKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\Dropbox");
            if (dropBoxKey == null)
            {
                return;
            }

            string dropBoxFolder = Path.Combine(Environment.GetEnvironmentVariable("HOMEDRIVE") + Environment.GetEnvironmentVariable("HOMEPATH"), "DropBox");
            if (!Directory.Exists(dropBoxFolder))
            {
                return;
            }

            Uri url = null;
            string urlInfoAbout = dropBoxKey.GetValue(@"URLInfoAbout") as string;
            if (!String.IsNullOrEmpty(urlInfoAbout))
            {
                Uri.TryCreate(urlInfoAbout, UriKind.Absolute, out url);
            }

            Icon icon = Resources.DropBox;
            IDataContainer dropBoxFolderInfo = TypeMap.Resolve.New<IDataContainer>(dropBoxFolder);
            KnownFolder knownFolder = new KnownFolder(dropBoxFolderInfo, Resources.MyAxCryptFolderName, icon.ToBitmap(), url);

            knownFolders.Add(knownFolder);
        }

        private static void CheckSkyDrive(IList<KnownFolder> knownFolders)
        {
            RegistryKey skyDriveKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\SkyDrive");
            if (skyDriveKey == null)
            {
                return;
            }

            string skyDriveFolder = skyDriveKey.GetValue("UserFolder") as string;
            if (String.IsNullOrEmpty(skyDriveFolder) || !Directory.Exists(skyDriveFolder))
            {
                return;
            }

            Uri url = new Uri("https://skydrive.live.com/");

            Icon icon = Resources.SkyDrive;
            IDataContainer skyDriveFolderInfo = TypeMap.Resolve.New<IDataContainer>(skyDriveFolder);
            KnownFolder knownFolder = new KnownFolder(skyDriveFolderInfo, Resources.MyAxCryptFolderName, icon.ToBitmap(), url);

            knownFolders.Add(knownFolder);
        }

        private static void CheckGoogleDrive(IList<KnownFolder> knownFolders)
        {
            if (!IsGoogleDriveInstalled)
            {
                return;
            }

            string googleDriveFolder = Path.Combine(Environment.GetEnvironmentVariable("HOMEDRIVE") + Environment.GetEnvironmentVariable("HOMEPATH"), "Google Drive");
            if (String.IsNullOrEmpty(googleDriveFolder) || !Directory.Exists(googleDriveFolder))
            {
                return;
            }

            Uri url = new Uri("https://drive.google.com/");

            Icon icon = Resources.GoogleDrive;
            IDataContainer googleDriveFolderInfo = TypeMap.Resolve.New<IDataContainer>(googleDriveFolder);
            KnownFolder knownFolder = new KnownFolder(googleDriveFolderInfo, Resources.MyAxCryptFolderName, icon.ToBitmap(), url);
            knownFolders.Add(knownFolder);
        }

        private static string[] _googleDriveRegistryKeyNames = new string[]
        {
            @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\{56D4499E-AC3E-4B8D-91C9-C700C148C44B}",
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{56D4499E-AC3E-4B8D-91C9-C700C148C44B}",
            @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\{35574F09-89F9-4B16-B69B-64F3E25901B8}",
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{35574F09-89F9-4B16-B69B-64F3E25901B8}",
            @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\{CBC9F5FD-5CFA-4A33-81CD-369EAB77E3A6}",
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{CBC9F5FD-5CFA-4A33-81CD-369EAB77E3A6}",
        };

        private static bool IsGoogleDriveInstalled
        {
            get
            {
                foreach (string driveRegistryKeyName in _googleDriveRegistryKeyNames)
                {
                    RegistryKey googleDriveKey = Registry.LocalMachine.OpenSubKey(driveRegistryKeyName);
                    if (googleDriveKey != null)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
    }
}