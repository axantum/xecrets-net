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

using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Mono
{
    public class RuntimeFolderInfo : RuntimeFileInfo, IRuntimeFolderInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RuntimeFileInfo"/> class.
        /// </summary>
        /// <param name="fullName">The full path and name of the file or folder.</param>
        /// <exception cref="System.ArgumentNullException">fullName</exception>
        public RuntimeFolderInfo(string fullName) :
            base(fullName.NormalizeFolderPath())
        {
        }

        /// <summary>
        /// Combine the path of this instance with another path, creating a new instance.
        /// </summary>
        /// <param name="path">The path to combine with.</param>
        /// <returns>
        /// A new instance representing the combined path.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public IRuntimeFileInfo FileItemInfo(string item)
        {
            return new RuntimeFileInfo(Path.Combine(FullName, item));
        }

        /// <summary>
        /// Get a folder item from this instance (which must represent a folder or container)..
        /// </summary>
        /// <param name="item">The name of the file item.</param>
        /// <returns>
        /// A new instance representing the file item in the folder or container.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public IRuntimeFolderInfo FolderItemInfo(string item)
        {
            return new RuntimeFolderInfo(Path.Combine(FullName, item));
        }
    }
}