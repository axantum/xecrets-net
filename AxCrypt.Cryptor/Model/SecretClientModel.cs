using AxCrypt.Core.Secrets;
using System;
using System.Collections.Generic;

namespace AxCrypt.Cryptor.Model
{
    public class SecretClientModel : IEquatable<SecretClientModel>
    {
        /// <summary>
        /// Only used by the client
        /// </summary>
        public SecretClientModel()
        {
        }

        public SecretClientModel(SecretClientModel secret)
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
            Share = secret.Share;
            CreatedUtc = secret.CreatedUtc;
            UpdatedUtc = secret.UpdatedUtc;
            DeletedUtc = secret.DeletedUtc;
        }

        public SecretClientModel(Guid id, SecretPassword secretPassword)
        {
            Id = id;
            Password = secretPassword;
            Type = AxCrypt.Api.Model.Secret.SecretType.Password;
        }

        public SecretClientModel(Guid id, SecretPassword password, DateTime createdUtc, DateTime updatedUtc, DateTime deletedUtc)
            : this(id, password)
        {
            CreatedUtc = createdUtc;
            UpdatedUtc = updatedUtc;
            DeletedUtc = deletedUtc;
        }

        public SecretClientModel(Guid id, SecretCard secretCard)
        {
            Id = id;
            Card = secretCard;
            Type = AxCrypt.Api.Model.Secret.SecretType.Card;
        }

        public SecretClientModel(Guid id, SecretCard secretCard, DateTime createdUtc, DateTime updatedUtc, DateTime deletedUtc)
            : this(id, secretCard)
        {
            CreatedUtc = createdUtc;
            UpdatedUtc = updatedUtc;
            DeletedUtc = deletedUtc;
        }

        public SecretClientModel(Guid id, SecretNote secretNote)
        {
            Id = id;
            Note = secretNote;
            Type = AxCrypt.Api.Model.Secret.SecretType.Note;
        }

        public SecretClientModel(Guid id, SecretNote secretNote, DateTime createdUtc, DateTime updatedUtc, DateTime deletedUtc)
            : this(id, secretNote)
        {
            CreatedUtc = createdUtc;
            UpdatedUtc = updatedUtc;
            DeletedUtc = deletedUtc;
        }

        public SecretClientModel(Guid id, ShareSecret share)
        {
            Id = id;
            Share = share;
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

        private long _dbid;

        /// <summary>
        /// The unique id used for this secret
        /// </summary>
        public long DBId
        {
            get { return _dbid; }
            set { _dbid = value; }
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

        private SecretPassword _password;

        public SecretPassword Password
        {
            get { return _password; }
            set { _password = value; }
        }

        private SecretCard _card;

        public SecretCard Card
        {
            get { return _card; }
            set { _card = value; }
        }

        private SecretNote _note;

        public SecretNote Note
        {
            get { return _note; }
            set { _note = value; }
        }

        private ShareSecret _share;

        public ShareSecret Share
        {
            get { return _share; }
            set { _share = value; }
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

        #region IEquatable<SecretClientModel> Members

        public bool Equals(SecretClientModel other)
        {
            if (other == null)
            {
                return false;
            }
            return Id == other.Id;
        }

        #endregion IEquatable<SecretClientModel> Members
    }
}