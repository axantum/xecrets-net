using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Service;
using Axantum.AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Session
{
    /// <summary>
    /// Persists a users asymmetric keys in the file system, encrypted with AxCrypt
    /// </summary>
    public class UserAsymmetricKeysStore
    {
        private IAccountService _service;

        private Lazy<IList<UserKeyPair>> _userKeyPairs;

        public UserAsymmetricKeysStore(IAccountService service)
        {
            _service = service;
            _userKeyPairs = new Lazy<IList<UserKeyPair>>(() => _service.List());
        }

        public bool HasKeyPair
        {
            get
            {
                return _userKeyPairs.Value.Any();
            }
        }

        public void Import(UserKeyPair keyPair)
        {
            if (keyPair == null)
            {
                throw new ArgumentNullException(nameof(keyPair));
            }

            if (keyPair.UserEmail != EmailAddress.Parse(_service.Identity.User))
            {
                throw new ArgumentException("User email mismatch in key pair and store.", nameof(keyPair));
            }

            if (_userKeyPairs.Value.Any(k => k == keyPair))
            {
                return;
            }

            _userKeyPairs.Value.Add(keyPair);
            _service.Save(_userKeyPairs.Value);
        }

        public virtual IEnumerable<UserKeyPair> UserKeyPairs
        {
            get
            {
                return _userKeyPairs.Value;
            }
        }

        public UserKeyPair UserKeyPair
        {
            get
            {
                return UserKeyPairs.First();
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has any shareable identities, i.e. key pairs.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has a store; otherwise, <c>false</c>.
        /// </value>
        public virtual bool HasStore
        {
            get
            {
                return _service.List().Any();
            }
        }

        public EmailAddress UserEmail
        {
            get
            {
                return UserKeyPair.UserEmail;
            }
        }

        public virtual void Save(Passphrase passphrase)
        {
            _service.ChangePassword(passphrase.Text);
            _service.Save(UserKeyPairs);
        }
    }
}