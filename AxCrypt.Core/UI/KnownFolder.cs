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

using AxCrypt.Core.IO;
using System.Linq;
using System;

using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Core.UI
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

        public object Image
        {
            get
            {
                return New<IKnownFolderImageProvider>().GetImage(_knownFolderKind);
            }
        }

        public bool Enabled { get; private set; }

        public string DisplayName { get; private set; }

        private readonly KnownFolderKind _knownFolderKind;

        public KnownFolder(IDataContainer knownFolderInfo, string myFolderName, KnownFolderKind knownFolderKind, Uri providerUrl)
            : this(knownFolderInfo, myFolderName, knownFolderKind, providerUrl, string.Empty)
        {
        }

        public KnownFolder(IDataContainer knownFolderInfo, string myFolderName, KnownFolderKind knownFolderKind, Uri providerUrl, string displayName)
        {
            if (myFolderName == null)
            {
                throw new ArgumentNullException(nameof(myFolderName));
            }
            Folder = knownFolderInfo ?? throw new ArgumentNullException(nameof(knownFolderInfo));
            My = knownFolderInfo.FolderItemInfo(myFolderName);
            _knownFolderKind = knownFolderKind;
            ProviderUrl = providerUrl;
            Enabled = false;
            DisplayName = displayName;
        }

        public KnownFolder(KnownFolder knownFolder, bool enabled)
        {
            if (knownFolder == null)
            {
                throw new ArgumentNullException(nameof(knownFolder));
            }
            Folder = knownFolder.Folder;
            My = knownFolder.My;
            _knownFolderKind = knownFolder._knownFolderKind;
            ProviderUrl = knownFolder.ProviderUrl;
            DisplayName = knownFolder.DisplayName;
            Enabled = enabled;
        }
    }
}
