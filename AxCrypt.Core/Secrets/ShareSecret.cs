using System;
using System.Collections.Generic;

namespace AxCrypt.Core.Secrets
{
    public class ShareSecret
    {
        public ShareSecret(IEnumerable<SecretSharedUser> sharedUsers, string sharedBy, DateTime? shared, string encryptedSecret = "")
        {
            SharedWith = sharedUsers;
            OwnerEmail = sharedBy;
            Shared = shared;
            EncryptedSecret = encryptedSecret;
        }

        private IEnumerable<SecretSharedUser>? _sharedWith;

        public IEnumerable<SecretSharedUser> SharedWith
        {
            get { return _sharedWith ?? new List<SecretSharedUser>(); }
            set { _sharedWith = value; }
        }

        private string? _ownerEmail;

        public string OwnerEmail
        {
            get { return _ownerEmail ?? string.Empty; }
            set { _ownerEmail = value; }
        }

        private DateTime? _shared;

        public DateTime? Shared
        {
            get { return _shared ?? DateTime.MinValue; }
            set { _shared = value; }
        }

        private string? _encryptedSecret;

        public string EncryptedSecret
        {
            get { return _encryptedSecret ?? string.Empty; }
            set { _encryptedSecret = value; }
        }
    }
}
