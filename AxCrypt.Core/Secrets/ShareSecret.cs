using System;
using System.Collections.Generic;
using System.Linq;

namespace AxCrypt.Core.Secrets
{
    public class ShareSecret
    {
        public ShareSecret(IEnumerable<SecretSharedUser> sharedUsers, string sharedBy, DateTime? shared)
        {
            SharedWith = sharedUsers;
            OwnerEmail = sharedBy;
            Shared = shared;
        }

        private IEnumerable<SecretSharedUser> _sharedWith;

        public IEnumerable<SecretSharedUser> SharedWith
        {
            get { return _sharedWith ?? new List<SecretSharedUser>(); }
            set { _sharedWith = value; }
        }

        private string _ownerEmail;

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
    }
}