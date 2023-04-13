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
using AxCrypt.Core.Runtime;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Mono
{
    public abstract class DataItem : IDataItem
    {
        public static IDataItem Create(string location)
        {
            if (Directory.Exists(location))
            {
                return New<IDataContainer>(location);
            }
            if (File.Exists(location))
            {
                return New<IDataStore>(location);
            }
            return New<IDataStore>(location);
        }

        protected virtual string Location { get; set; } = string.Empty;

        public abstract bool IsAvailable
        {
            get;
        }

        public abstract bool IsFile
        {
            get;
        }

        public abstract bool IsFolder
        {
            get;
        }

        public abstract string Name
        {
            get;
        }

        public abstract string FullName
        {
            get;
        }

        public abstract void Delete();

        public IDataContainer Container
        {
            get
            {
                return new DataContainer(Path.GetDirectoryName(Location) ?? throw new InternalErrorException("Directory name was null or a root folder."));
            }
        }

        public override string ToString()
        {
            return FullName;
        }

        public abstract bool IsNetworkPath
        {
            get;
        }
    }
}
