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
using System.Diagnostics.CodeAnalysis;

namespace AxCrypt.Core.Secrets
{
    /// <summary>
    /// A single secret - by definition three fields, an id, a description and an associated secret.
    /// </summary>
    public class Secret : IEquatable<Secret>
    {
        /// <summary>
        /// Only used by the framework - don't use this.
        /// </summary>
        public Secret()
        {
        }

        public Secret(Secret secret)
        {
            if (secret == null)
            {
                throw new ArgumentNullException("secret");
            }

            Id = secret.Id;
            Type = secret.Type;
            Password = secret.Password;
            Card = secret.Card;
            Note = secret.Note;
            EncryptionKey = secret.EncryptionKey;
            CreatedUtc = secret.CreatedUtc;
            UpdatedUtc = secret.UpdatedUtc;
            DeletedUtc = secret.DeletedUtc;
        }

        public Secret(Guid id, SecretPassword secretPassword)
        {
            Id = id;
            Password = secretPassword;
            Type = AxCrypt.Api.Model.Secret.SecretType.Password;
        }

        public Secret(Guid id, SecretPassword password, EncryptionKey encryptionKey)
            : this(id, password)
        {
            EncryptionKey = encryptionKey;
        }

        public Secret(Guid id, SecretPassword password, EncryptionKey encryptionKey, DateTime createdUtc, DateTime updatedUtc, DateTime deletedUtc)
            : this(id, password, encryptionKey)
        {
            CreatedUtc = createdUtc;
            UpdatedUtc = updatedUtc;
            DeletedUtc = deletedUtc;
        }

        public Secret(Guid id, SecretCard secretCard)
        {
            Id = id;
            Card = secretCard;
            Type = AxCrypt.Api.Model.Secret.SecretType.Card;
        }

        public Secret(Guid id, SecretCard secretCard, EncryptionKey encryptionKey)
            : this(id, secretCard)
        {
            EncryptionKey = encryptionKey;
        }

        public Secret(Guid id, SecretCard secretCard, EncryptionKey encryptionKey, DateTime createdUtc, DateTime updatedUtc, DateTime deletedUtc)
            : this(id, secretCard, encryptionKey)
        {
            CreatedUtc = createdUtc;
            UpdatedUtc = updatedUtc;
            DeletedUtc = deletedUtc;
        }

        public Secret(Guid id, SecretNote secretNote)
        {
            Id = id;
            Note = secretNote;
            Type = AxCrypt.Api.Model.Secret.SecretType.Note;
        }

        public Secret(Guid id, SecretNote secretNote, EncryptionKey encryptionKey)
            : this(id, secretNote)
        {
            EncryptionKey = encryptionKey;
        }

        public Secret(Guid id, SecretNote secretNote, EncryptionKey encryptionKey, DateTime createdUtc, DateTime updatedUtc, DateTime deletedUtc)
            : this(id, secretNote, encryptionKey)
        {
            CreatedUtc = createdUtc;
            UpdatedUtc = updatedUtc;
            DeletedUtc = deletedUtc;
        }

        private Guid _id;

        /// <summary>
        /// The unique id used for this secret
        /// </summary>
        public Guid Id
        {
            get { return _id; }
            set { _id = value; }
        }

        private int _dbid;

        /// <summary>
        /// The unique id used for this secret
        /// </summary>
        public int DBId
        {
            get { return _dbid; }
            set { _dbid = value; }
        }

        [AllowNull]
        private EncryptionKey _encryptionKey;

        /// <summary>
        /// Gets or sets the encryption key.
        /// </summary>
        /// <value>The encryption key.</value>
        public EncryptionKey EncryptionKey
        {
            get { return _encryptionKey; }
            set { _encryptionKey = value; }
        }

        private AxCrypt.Api.Model.Secret.SecretType _secretType;

        /// <summary>
        /// The secret type
        /// </summary>
        public AxCrypt.Api.Model.Secret.SecretType Type
        {
            get { return _secretType; }
            set { _secretType = value; }
        }

        [AllowNull]
        private SecretPassword _password;

        public SecretPassword Password
        {
            get { return _password; }
            set { _password = value; }
        }

        [AllowNull]
        private SecretCard _card;

        public SecretCard Card
        {
            get { return _card; }
            set { _card = value; }
        }

        [AllowNull]
        private SecretNote _note;

        public SecretNote Note
        {
            get { return _note; }
            set { _note = value; }
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

        #region IEquatable<Secret> Members

        public bool Equals(Secret? other)
        {
            if (other == null)
            {
                return false;
            }
            return Id == other.Id;
        }

        #endregion IEquatable<Secret> Members
    }
}
