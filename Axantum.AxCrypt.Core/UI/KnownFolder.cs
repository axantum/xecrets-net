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
using System.Linq;
using Axantum.AxCrypt.Core.IO;

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
        public IRuntimeFileInfo RootFullPath { get; private set; }

        public IRuntimeFileInfo MyFullPath { get; private set; }

        public Uri ProviderUrl { get; private set; }

        public object Image { get; private set; }

        public bool Enabled { get; private set; }

        public KnownFolder(string rootFullPath, string myRelativePath, object image, Uri providerUrl)
        {
            if (rootFullPath == null)
            {
                throw new ArgumentNullException("rootFullPath");
            }
            if (myRelativePath == null)
            {
                throw new ArgumentNullException("myRelativePath");
            }
            if (image == null)
            {
                throw new ArgumentNullException("image");
            }
            RootFullPath = Factory.New<IRuntimeFileInfo>(rootFullPath);
            MyFullPath = RootFullPath.Combine(myRelativePath);
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
            RootFullPath = knownFolder.RootFullPath;
            MyFullPath = knownFolder.MyFullPath;
            Image = knownFolder.Image;
            ProviderUrl = knownFolder.ProviderUrl;
            Enabled = enabled;
        }
    }
}