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

using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Session;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Axantum.AxCrypt.Core.UI
{
    public class KnownKeys
    {
        private List<SymmetricKey> _keys;

        private FileSystemState _fileSystemState;

        private SessionNotify _notificationMonitor;

        public KnownKeys(FileSystemState fileSystemState, SessionNotify notificationMonitor)
        {
            _fileSystemState = fileSystemState;
            _notificationMonitor = notificationMonitor;
            _keys = new List<SymmetricKey>();
            _knownThumbprints = new List<SymmetricKeyThumbprint>();
        }

        public bool IsLoggedOn
        {
            get
            {
                return DefaultEncryptionKey != null;
            }
        }

        public void LogOff()
        {
            DefaultEncryptionKey = null;
        }

        public void Add(SymmetricKey key)
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
                if (_fileSystemState.Identities.Any(i => i.Thumbprint == key.Thumbprint))
                {
                    DefaultEncryptionKey = key;
                }
                _notificationMonitor.Notify(new SessionNotification(SessionNotificationType.KnownKeyChange, key));
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
            LogOff();
            _notificationMonitor.Notify(new SessionNotification(SessionNotificationType.KnownKeyChange));
        }

        public IEnumerable<SymmetricKey> Keys
        {
            get
            {
                lock (_keys)
                {
                    return new List<SymmetricKey>(_keys);
                }
            }
        }

        private SymmetricKey _defaultEncryptionKey;

        /// <summary>
        /// Gets or sets the default encryption key.
        /// </summary>
        /// <value>
        /// The default encryption key, or null if none is known.
        /// </value>
        public SymmetricKey DefaultEncryptionKey
        {
            get
            {
                return _defaultEncryptionKey;
            }
            set
            {
                if (_defaultEncryptionKey == value)
                {
                    return;
                }
                if (_defaultEncryptionKey != null)
                {
                    SymmetricKey oldKey = _defaultEncryptionKey;
                    _defaultEncryptionKey = null;
                    _notificationMonitor.Notify(new SessionNotification(SessionNotificationType.LogOff, oldKey));
                }
                if (value == null)
                {
                    return;
                }
                _defaultEncryptionKey = value;
                Add(_defaultEncryptionKey);
                _notificationMonitor.Notify(new SessionNotification(SessionNotificationType.LogOn, value));
            }
        }

        private List<SymmetricKeyThumbprint> _knownThumbprints;

        /// <summary>
        /// Add a thumb print to the list of known thumb prints
        /// </summary>
        /// <param name="thumbprint">The key to add the fingerprint of</param>
        /// <returns>True if a new thumb print was added, false if it was already known.</returns>
        private bool AddKnownThumbprint(SymmetricKey key)
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
                return _fileSystemState.WatchedFolders.Where(wf => wf.Thumbprint == DefaultEncryptionKey.Thumbprint);
            }
        }
    }
}