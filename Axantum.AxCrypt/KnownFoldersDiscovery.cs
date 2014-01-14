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
    internal class KnownFoldersDiscovery
    {
        public IEnumerable<KnownFolder> Discover()
        {
            List<KnownFolder> knownFolders = new List<KnownFolder>();

            CheckDocumentsLibrary(knownFolders);
            CheckDropBox(knownFolders);
            CheckSkyDrive(knownFolders);
            CheckGoogleDrive(knownFolders);

            return knownFolders;
        }

        private void CheckDocumentsLibrary(IList<KnownFolder> knownFolders)
        {
            Icon icon = Resources.DocumentsLibrary;
            KnownFolder windowsDesktopFolder = new KnownFolder(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Resources.MyAxCryptFolderName, icon.ToBitmap(), null);
            knownFolders.Add(windowsDesktopFolder);
        }

        private void CheckDropBox(IList<KnownFolder> knownFolders)
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
            KnownFolder knownFolder = new KnownFolder(dropBoxFolder, Resources.MyAxCryptFolderName, icon.ToBitmap(), url);

            knownFolders.Add(knownFolder);
        }

        private void CheckSkyDrive(IList<KnownFolder> knownFolders)
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
            KnownFolder knownFolder = new KnownFolder(skyDriveFolder, Resources.MyAxCryptFolderName, icon.ToBitmap(), url);

            knownFolders.Add(knownFolder);
        }

        private void CheckGoogleDrive(IList<KnownFolder> knownFolders)
        {
            RegistryKey googleDriveKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\{56D4499E-AC3E-4B8D-91C9-C700C148C44B}");
            if (googleDriveKey == null)
            {
                googleDriveKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{56D4499E-AC3E-4B8D-91C9-C700C148C44B}");
            }
            if (googleDriveKey == null)
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
            KnownFolder knownFolder = new KnownFolder(googleDriveFolder, Resources.MyAxCryptFolderName, icon.ToBitmap(), url);
            knownFolders.Add(knownFolder);
        }
    }
}