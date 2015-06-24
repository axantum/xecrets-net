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
    public class KnownIdentities
    {
        private List<LogOnIdentity> _logOnIdentities;

        private FileSystemState _fileSystemState;

        private SessionNotify _notificationMonitor;

        /// <summary>
        /// Initializes a new instance of the <see cref="KnownIdentities"/> class. Only for use by mocking frameork.
        /// </summary>
        protected KnownIdentities()
        {
            _logOnIdentities = new List<LogOnIdentity>();
            _knownThumbprints = new List<SymmetricKeyThumbprint>();
        }

        public KnownIdentities(FileSystemState fileSystemState, SessionNotify notificationMonitor)
            : this()
        {
            _fileSystemState = fileSystemState;
            _notificationMonitor = notificationMonitor;
        }

        public bool IsLoggedOn
        {
            get
            {
                return DefaultEncryptionIdentity != null;
            }
        }

        public void LogOff()
        {
            DefaultEncryptionIdentity = null;
        }

        public virtual void Add(LogOnIdentity logOnIdentity)
        {
            bool changed = false;
            lock (_logOnIdentities)
            {
                int i = _logOnIdentities.IndexOf(logOnIdentity);
                if (i < 0)
                {
                    _logOnIdentities.Insert(0, logOnIdentity);
                    changed = true;
                }
            }
            changed |= AddKnownThumbprint(logOnIdentity);
            if (changed)
            {
                DefaultEncryptionIdentity = logOnIdentity;
                _notificationMonitor.Notify(new SessionNotification(SessionNotificationType.KnownKeyChange, logOnIdentity));
            }
        }

        public void Clear()
        {
            lock (_logOnIdentities)
            {
                if (_logOnIdentities.Count == 0)
                {
                    return;
                }
                _logOnIdentities.Clear();
            }
            LogOff();
            _notificationMonitor.Notify(new SessionNotification(SessionNotificationType.KnownKeyChange));
        }

        public IEnumerable<LogOnIdentity> Identities
        {
            get
            {
                lock (_logOnIdentities)
                {
                    return new List<LogOnIdentity>(_logOnIdentities);
                }
            }
        }

        private LogOnIdentity _defaultEncryptionIdentity;

        /// <summary>
        /// Gets or sets the default encryption key.
        /// </summary>
        /// <value>
        /// The default encryption key, or null if none is known.
        /// </value>
        public virtual LogOnIdentity DefaultEncryptionIdentity
        {
            get
            {
                return _defaultEncryptionIdentity;
            }
            set
            {
                if (_defaultEncryptionIdentity == value)
                {
                    return;
                }
                if (_defaultEncryptionIdentity != null)
                {
                    LogOnIdentity oldKey = _defaultEncryptionIdentity;
                    _defaultEncryptionIdentity = null;
                    _notificationMonitor.Notify(new SessionNotification(SessionNotificationType.LogOff, oldKey));
                }
                if (value == null)
                {
                    return;
                }
                _defaultEncryptionIdentity = value;
                Add(_defaultEncryptionIdentity);
                _notificationMonitor.Notify(new SessionNotification(SessionNotificationType.LogOn, value));
            }
        }

        private List<SymmetricKeyThumbprint> _knownThumbprints;

        /// <summary>
        /// Add a thumb print to the list of known thumb prints
        /// </summary>
        /// <param name="thumbprint">The key to add the fingerprint of</param>
        /// <returns>True if a new thumb print was added, false if it was already known.</returns>
        private bool AddKnownThumbprint(LogOnIdentity identity)
        {
            lock (_knownThumbprints)
            {
                if (_knownThumbprints.Contains(identity.Passphrase.Thumbprint))
                {
                    return false;
                }
                _knownThumbprints.Add(identity.Passphrase.Thumbprint);
                return true;
            }
        }

        public IEnumerable<WatchedFolder> LoggedOnWatchedFolders
        {
            get
            {
                if (!IsLoggedOn)
                {
                    return new WatchedFolder[0];
                }
                return _fileSystemState.WatchedFolders.Where(wf => wf.Tag.Matches(DefaultEncryptionIdentity.Tag));
            }
        }
    }
}