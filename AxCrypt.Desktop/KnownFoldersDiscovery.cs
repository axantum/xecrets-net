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
 * The source is maintained at http://bitbucket.org/AxCrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using AxCrypt.Core;
using AxCrypt.Core.IO;
using AxCrypt.Core.Runtime;
using AxCrypt.Core.UI;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static AxCrypt.Abstractions.TypeResolve;
using Texts = AxCrypt.Content.Texts;

namespace AxCrypt.Desktop
{
    public class KnownFoldersDiscovery : IKnownFoldersDiscovery
    {
        private static readonly string MyAxCryptFolderName = "My AxCrypt";

        public IEnumerable<KnownFolder> Discover()
        {
            List<KnownFolder> knownFolders = new List<KnownFolder>();
            if (OS.Current.Platform != Platform.WindowsDesktop)
            {
                return knownFolders;
            }

            CheckDocumentsLibrary(knownFolders);
            CheckDropBox(knownFolders);
            CheckOneDrive(knownFolders);
            CheckGoogleDrive(knownFolders);

            return knownFolders;
        }

        private static void CheckDocumentsLibrary(IList<KnownFolder> knownFolders)
        {
            IDataContainer myDocumentsInfo = New<IDataContainer>(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            KnownFolder windowsDesktopFolder = new KnownFolder(myDocumentsInfo, MyAxCryptFolderName, KnownFolderKind.WindowsMyDocuments, null, Texts.KnownFolderNameWindowsMyDocuments);
            knownFolders.Add(windowsDesktopFolder);
        }

        private static void CheckDropBox(IList<KnownFolder> knownFolders)
        {
            string dropBoxFolder = Path.Combine(Environment.GetEnvironmentVariable("HOMEDRIVE") + Environment.GetEnvironmentVariable("HOMEPATH"), "DropBox");
            if (!Directory.Exists(dropBoxFolder))
            {
                return;
            }

            IDataContainer dropBoxFolderInfo = New<IDataContainer>(dropBoxFolder);
            KnownFolder knownFolder = new KnownFolder(dropBoxFolderInfo, MyAxCryptFolderName, KnownFolderKind.Dropbox, null, Texts.KnownFolderNameDropbox);

            knownFolders.Add(knownFolder);
        }

        private static void CheckOneDrive(IList<KnownFolder> knownFolders)
        {
            string oneDriveFolder = FindOneDriveFolder();
            if (!Directory.Exists(oneDriveFolder))
            {
                return;
            }

            Uri url = new Uri("https://onedrive.live.com/");
            IDataContainer oneDriveFolderInfo = New<IDataContainer>(oneDriveFolder);
            KnownFolder knownFolder = new KnownFolder(oneDriveFolderInfo, MyAxCryptFolderName, KnownFolderKind.OneDrive, url, Texts.KnownFolderNameOneDrive);

            knownFolders.Add(knownFolder);
        }

        private static string FindOneDriveFolder()
        {
            string oneDriveFolder = null;

            oneDriveFolder = TryRegistryLocationForOneDriveFolder(@"Software\Microsoft\OneDrive");
            if (oneDriveFolder != null)
            {
                return oneDriveFolder;
            }

            oneDriveFolder = TryRegistryLocationForOneDriveFolder(@"Software\Microsoft\Windows\CurrentVersion\SkyDrive");
            if (oneDriveFolder != null)
            {
                return oneDriveFolder;
            }

            oneDriveFolder = Path.Combine(Environment.GetEnvironmentVariable("HOMEDRIVE") + Environment.GetEnvironmentVariable("HOMEPATH"), "OneDrive");
            return oneDriveFolder;
        }

        private static string TryRegistryLocationForOneDriveFolder(string name)
        {
            RegistryKey oneDriveKey = Registry.CurrentUser.OpenSubKey(name);
            if (oneDriveKey == null)
            {
                return null;
            }

            string oneDriveFolder = oneDriveKey.GetValue("UserFolder") as string;
            if (String.IsNullOrEmpty(oneDriveFolder))
            {
                return null;
            }

            return oneDriveFolder;
        }

        private static void CheckGoogleDrive(IList<KnownFolder> knownFolders)
        {
            string googleDriveFolder = Path.Combine(Environment.GetEnvironmentVariable("HOMEDRIVE") + Environment.GetEnvironmentVariable("HOMEPATH"), "Google Drive");
            if (String.IsNullOrEmpty(googleDriveFolder) || !Directory.Exists(googleDriveFolder))
            {
                return;
            }

            Uri url = new Uri("https://drive.google.com/");

            IDataContainer googleDriveFolderInfo = New<IDataContainer>(googleDriveFolder);
            KnownFolder knownFolder = new KnownFolder(googleDriveFolderInfo, MyAxCryptFolderName, KnownFolderKind.GoogleDrive, url, Texts.KnownFolderNameGoogleDrive);
            knownFolders.Add(knownFolder);
        }
    }
}