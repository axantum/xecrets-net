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

using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.IO;
using System;
using System.Linq;

namespace Axantum.AxCrypt.Core.UI
{
    /// <summary>
    /// Holds properties for specific known folders, such as DropBox, My Documents etc
    /// </summary>
    /// <remarks>
    /// Instances of this class are immutable.
    /// </remarks>
    public class KnownFolder
    {
        public IDataContainer Folder { get; private set; }

        public IDataContainer My { get; private set; }

        public Uri ProviderUrl { get; private set; }

        public object Image { get; private set; }

        public bool Enabled { get; private set; }

        public KnownFolder(IDataContainer knownFolderInfo, string myFolderName, object image, Uri providerUrl)
        {
            if (knownFolderInfo == null)
            {
                throw new ArgumentNullException("knownFolderInfo");
            }
            if (myFolderName == null)
            {
                throw new ArgumentNullException("myFolderName");
            }
            if (image == null)
            {
                throw new ArgumentNullException("image");
            }
            Folder = knownFolderInfo;
            My = knownFolderInfo.FolderItemInfo(myFolderName);
            Image = image;
            ProviderUrl = providerUrl;
            Enabled = false;
        }

        public KnownFolder(KnownFolder knownFolder, bool enabled)
        {
            if (knownFolder == null)
            {
                throw new ArgumentNullException("knownFolder");
            }
            Folder = knownFolder.Folder;
            My = knownFolder.My;
            Image = knownFolder.Image;
            ProviderUrl = knownFolder.ProviderUrl;
            Enabled = enabled;
        }
    }
}