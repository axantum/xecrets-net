#region Coypright and License

/*
 * AxCrypt - Copyright 2012, Svante Seleborg, All Rights Reserved
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

using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Axantum.AxCrypt.Core.UI
{
    public class KnownKeys
    {
        private List<AesKey> _keys;

        public event EventHandler<EventArgs> Changed;

        public KnownKeys()
        {
            _keys = new List<AesKey>();
            _knownThumbprints = new List<AesKeyThumbprint>();
        }

        public bool IsLoggedOn
        {
            get
            {
                return DefaultEncryptionKey != null;
            }
        }

        protected virtual void OnChanged(EventArgs e)
        {
            EventHandler<EventArgs> handler = Changed;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public void Add(AesKey key)
        {
            bool changed = false;
            lock (_keys)
            {
                int i = _keys.IndexOf(key);
                if (i < 0)
                {
                    _keys.Insert(0, key);
                    changed = true;
                }
            }
            changed |= AddKnownThumbprint(key);
            if (changed)
            {
                OnChanged(new EventArgs());
            }
        }

        public void Clear()
        {
            lock (_keys)
            {
                if (_keys.Count == 0)
                {
                    return;
                }
                _keys.Clear();
            }
            _defaultEncryptionKey = null;
            OnChanged(new EventArgs());
        }

        public IEnumerable<AesKey> Keys
        {
            get
            {
                lock (_keys)
                {
                    return new List<AesKey>(_keys);
                }
            }
        }

        private AesKey _defaultEncryptionKey;

        /// <summary>
        /// Gets or sets the default encryption key.
        /// </summary>
        /// <value>
        /// The default encryption key, or null if none is known.
        /// </value>
        public AesKey DefaultEncryptionKey
        {
            get
            {
                return _defaultEncryptionKey;
            }
            set
            {
                if (value != null)
                {
                    OS.Current.NotifySessionChanged(new SessionEvent(SessionEventType.LogOn, value));
                }
                else
                {
                    OS.Current.NotifySessionChanged(new SessionEvent(SessionEventType.LogOff, _defaultEncryptionKey));
                }
                _defaultEncryptionKey = value;
                if (value == null)
                {
                    return;
                }
                Add(value);
            }
        }

        private List<AesKeyThumbprint> _knownThumbprints;

        public IEnumerable<AesKeyThumbprint> KnownThumbprints
        {
            get
            {
                lock (_knownThumbprints)
                {
                    return new List<AesKeyThumbprint>(_knownThumbprints);
                }
            }
            set
            {
                lock (_knownThumbprints)
                {
                    _knownThumbprints.Clear();
                    _knownThumbprints.AddRange(value);
                }
            }
        }

        /// <summary>
        /// Add a thumb print to the list of known thumb prints
        /// </summary>
        /// <param name="thumbprint">The key to add the fingerprint of</param>
        /// <returns>True if a new thumb print was added, false if it was already known.</returns>
        private bool AddKnownThumbprint(AesKey key)
        {
            lock (_knownThumbprints)
            {
                if (_knownThumbprints.Contains(key.Thumbprint))
                {
                    return false;
                }
                _knownThumbprints.Add(key.Thumbprint);
                return true;
            }
        }

        public IEnumerable<WatchedFolder> WatchedFolders
        {
            get
            {
                if (!IsLoggedOn)
                {
                    return new WatchedFolder[0];
                }
                return Instance.FileSystemState.WatchedFolders.Where(wf => wf.Thumbprint == DefaultEncryptionKey.Thumbprint);
            }
        }
    }
}