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
using System.Text;

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
            Title = secret.Title;
            Description = secret.Description;
            TheSecret = secret.TheSecret;
            EncryptionKey = secret.EncryptionKey;
        }

        public Secret(Guid id, string title, string description, string theSecret)
        {
            Id = id;
            Title = title;
            Description = description;
            TheSecret = theSecret;
        }

        public Secret(Guid id, string title, string description, string theSecret, EncryptionKey encryptionKey)
            : this(id, title, description, theSecret)
        {
            EncryptionKey = encryptionKey;
        }

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

        private Guid _id;

        /// <summary>
        /// The unique id used for this secret
        /// </summary>
        public Guid Id
        {
            get { return _id; }
            set { _id = value; }
        }

        private string _title;

        public string Title
        {
            get { return _title ?? String.Empty; }
            set { _title = value; }
        }

        private string _description;

        /// <summary>
        /// A (long) description, not necessarily unique, for this secret
        /// </summary>
        public string Description
        {
            get { return _description ?? String.Empty; }
            set { _description = value; }
        }

        private string _theSecret;

        /// <summary>
        /// The (short) actual secret - it may be any text
        /// </summary>
        public string TheSecret
        {
            get { return _theSecret ?? String.Empty; }
            set { _theSecret = value; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is empty.
        /// </summary>
        /// <value><c>true</c> if this instance is empty; otherwise, <c>false</c>.</value>
        public bool IsEmpty
        {
            get
            {
                return String.IsNullOrEmpty(Title) && String.IsNullOrEmpty(Description) && String.IsNullOrEmpty(TheSecret);
            }
        }

        #region IEquatable<Secret> Members

        public bool Equals(Secret other)
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