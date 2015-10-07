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
    /// Persists a users account information using an IAccountService instance as medium
    /// </summary>
    public class AccountStorage
    {
        private IAccountService _service;

        public AccountStorage(IAccountService service)
        {
            _service = service;
        }

        public bool HasKeyPair
        {
            get
            {
                return _service.List().Any();
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

            IList<UserKeyPair> keyPairs = _service.List();
            if (keyPairs.Any(k => k == keyPair))
            {
                return;
            }

            keyPairs.Add(keyPair);
            _service.Save(keyPairs);
        }

        public virtual IEnumerable<UserKeyPair> AllKeyPairs
        {
            get
            {
                return _service.List().OrderByDescending(uk => uk.Timestamp);
            }
        }

        public UserKeyPair ActiveKeyPair
        {
            get
            {
                return AllKeyPairs.First();
            }
        }

        public EmailAddress UserEmail
        {
            get
            {
                return EmailAddress.Parse(_service.Identity.User);
            }
        }

        public virtual void ChangePassphrase(Passphrase passphrase)
        {
            if (passphrase == null)
            {
                throw new ArgumentNullException(nameof(passphrase));
            }

            _service.ChangePassphrase(passphrase.Text);
            _service.Save(AllKeyPairs);
        }
    }
}