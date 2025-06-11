#if DEBUG
#define CODE_ANALYSIS
#endif

#region License

/*
 *  AxCrypt.Core - Core and Reference Implementation
 *
 *  Copyright (C) 2025 AxCrypt AB
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
 *  Contact: mailto:support@axcrypt.net
 */

#endregion License

using AxCrypt.Core.Secrets;
using System;

namespace AxCrypt.Core.SecuredMessenger
{
    /// <summary>
    /// A single message - by definition three fields, an id, a description and an associated message.
    /// </summary>
    public class Messenger : IEquatable<Messenger>
    {
        /// <summary>
        /// Only used by the framework - don't use this.
        /// </summary>
        public Messenger()
        {
        }

        //public Messenger(Messenger message)
        //{
        //    if (message == null)
        //    {
        //        throw new ArgumentNullException("message");
        //    }

        //    Id = message.Id;
        //    DBId = message.DBId;
        //    Message = message.Message;
        //    EncryptionKey = message.EncryptionKey;
        //    Read = message.Read;
        //    VisibileUntil = message.VisibileUntil;
        //    CreatedUtc = message.CreatedUtc;
        //    UpdatedUtc = message.UpdatedUtc;
        //    DeletedUtc = message.DeletedUtc;
        //}

        public Messenger(Guid id, SecuredMessenger securedMessage)
        {
            Id = id;
            Message = securedMessage;
        }

        public Messenger(Guid id, SecuredMessenger securedMessage, EncryptionKey encryptionKey)
            : this(id, securedMessage)
        {
            EncryptionKey = encryptionKey;
        }

        public Messenger(Guid id, SecuredMessenger securedMessage, EncryptionKey encryptionKey, DateTime createdUtc, DateTime updatedUtc, DateTime deletedUtc)
            : this(id, securedMessage, encryptionKey)
        {
            CreatedUtc = createdUtc;
            UpdatedUtc = updatedUtc;
            DeletedUtc = deletedUtc;
        }

        private Guid _id;

        /// <summary>
        /// The unique id used for this message
        /// </summary>
        public Guid Id
        {
            get { return _id; }
            set { _id = value; }
        }

        private Guid _parentId;

        /// <summary>
        /// The unique id used for this message
        /// </summary>
        public Guid ParentId
        {
            get { return _parentId; }
            set { _parentId = value; }
        }

        private long _dbid;

        /// <summary>
        /// The unique id used for this message
        /// </summary>
        public long DBId
        {
            get { return _dbid; }
            set { _dbid = value; }
        }

        private EncryptionKey? _encryptionKey;

        /// <summary>
        /// Gets or sets the encryption key.
        /// </summary>
        /// <value>The encryption key.</value>
        public EncryptionKey? EncryptionKey
        {
            get { return _encryptionKey; }
            set { _encryptionKey = value; }
        }

        private SecuredMessenger? _securedMessage;

        public SecuredMessenger? Message
        {
            get { return _securedMessage; }
            set { _securedMessage = value; }
        }

        private DateTime _createdUtc = DateTime.MinValue;

        public DateTime CreatedUtc
        {
            get { return _createdUtc; }
            set { _createdUtc = value; }
        }

        private DateTime _updatedUtc = DateTime.MinValue;

        public DateTime UpdatedUtc
        {
            get { return _updatedUtc; }
            set { _updatedUtc = value; }
        }

        private DateTime _deletedUtc = DateTime.MinValue;

        public DateTime DeletedUtc
        {
            get { return _deletedUtc; }
            set { _deletedUtc = value; }
        }

        //private DateTime _visibleUntil = DateTime.MinValue;

        //public DateTime VisibileUntil
        //{
        //    get { return _visibleUntil; }
        //    set { _visibleUntil = value; }
        //}

        //private DateTime _read;

        //public DateTime Read
        //{
        //    get { return _read; }
        //    set { _read = value; }
        //}

        #region IEquatable<Message> Members

        public bool Equals(Messenger? other)
        {
            if (other == null)
            {
                return false;
            }
            return Id == other.Id;
        }

        #endregion IEquatable<Message> Members
    }
}
