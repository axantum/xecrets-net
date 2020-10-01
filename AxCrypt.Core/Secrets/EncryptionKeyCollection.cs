#if DEBUG
#define CODE_ANALYSIS
#endif

#region License

/*
 *  AxCrypt.Xecrets.Core - Xecrets Core and Reference Implementation
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
using System.Collections.ObjectModel;
using System.Text;
using System.Collections;

namespace AxCrypt.Core.Secrets
{
    /// <summary>
    /// A collection of encryption keys, associated with a given user name.
    /// </summary>
    /// <remarks>
    /// Instances of this class cannot be serialized. Do not place in a location that survives an AppDomain restart.
    /// </remarks>
    public class EncryptionKeyCollection : ICollection<EncryptionKey>
    {
        private string _userName;

        private List<EncryptionKey> _list = new List<EncryptionKey>();

        public EncryptionKeyCollection(string userName)
        {
            _userName = userName;
        }

        public EncryptionKeyCollection(EncryptionKey key, string userName)
            : this(userName)
        {
            Add(key);
        }

        public string UserName
        {
            get
            {
                return _userName ?? String.Empty;
            }
        }

        public void AddRange(IEnumerable<EncryptionKey> keys)
        {
            _list.AddRange(keys);
        }

        public EncryptionKey this[int index]
        {
            get
            {
                return _list[index];
            }

            set
            {
                _list[index] = value;
            }
        }

        #region ICollection<EncryptionKey> Members

        public void Add(EncryptionKey item)
        {
            _list.Add(item);
        }

        public void Clear()
        {
            _list.Clear();
        }

        public bool Contains(EncryptionKey item)
        {
            return _list.Contains(item);
        }

        public void CopyTo(EncryptionKey[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return _list.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(EncryptionKey item)
        {
            return _list.Remove(item);
        }

        #endregion ICollection<EncryptionKey> Members

        #region IEnumerable<EncryptionKey> Members

        public IEnumerator<EncryptionKey> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        #endregion IEnumerable<EncryptionKey> Members

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        #endregion IEnumerable Members
    }
}