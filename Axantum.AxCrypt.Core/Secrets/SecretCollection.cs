#region License

/*
 *  Axantum.Xecrets.Core - Xecrets Core and Reference Implementation
 *
 *  Copyright (C) 2008 Svante Seleborg
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  If you'd like to license this program under any other terms than the
 *  above, please contact the author and copyright holder.
 *
 *  Contact: mailto:svante@axantum.com
 */

#endregion License

using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

namespace Axantum.AxCrypt.Core.Secrets
{
    public class SecretCollection : KeyedCollection<Guid, Secret>
    {
        protected override Guid GetKeyForItem(Secret item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            return item.Id;
        }

        public void AddRange(IEnumerable<Secret> secrets)
        {
            if (secrets == null)
            {
                throw new ArgumentNullException(nameof(secrets));
            }

            foreach (Secret secret in secrets)
            {
                Add(secret);
            }
        }

        private int _originalCount;

        /// <summary>
        /// If this collection is the result of a filtering operating, this is the original count.
        /// </summary>
        public int OriginalCount
        {
            get { return _originalCount; }
            set { _originalCount = value; }
        }
    }
}