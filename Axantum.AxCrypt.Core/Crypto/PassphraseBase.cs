using System;
using System.Linq;

namespace Axantum.AxCrypt.Core.Crypto
{
    public abstract class PassphraseBase : IPassphrase
    {
        public SymmetricKey DerivedKey
        {
            get;
            protected set;
        }

        public string Passphrase
        {
            get;
            protected set;
        }

        public string CryptoName
        {
            get;
            protected set;
        }

        private byte[] _derivationSalt;

        public byte[] GetDerivationSalt()
        {
            return (byte[])_derivationSalt.Clone();
        }

        protected void SetDeriviationSalt(byte[] derivationSalt)
        {
            _derivationSalt = (byte[])derivationSalt.Clone();
        }

        public long DerivationIterations { get; protected set; }

        private SymmetricKeyThumbprint _thumbprint;

        public SymmetricKeyThumbprint Thumbprint
        {
            get
            {
                if (_thumbprint == null)
                {
                    _thumbprint = new SymmetricKeyThumbprint(this, Instance.UserSettings.ThumbprintSalt, Instance.UserSettings.V1KeyWrapIterations);
                }
                return _thumbprint;
            }
            set
            {
                _thumbprint = value;
            }
        }

        #region IEquatable<SymmetricKey> Members

        /// <summary>
        /// Check if one instance is equivalent to another.
        /// </summary>
        /// <param name="other">The instance to compare to</param>
        /// <returns>true if the keys are equivalent</returns>
        public bool Equals(IPassphrase other)
        {
            if ((object)other == null)
            {
                return false;
            }
            return CryptoName == other.CryptoName && DerivedKey == other.DerivedKey && Passphrase == other.Passphrase;
        }

        #endregion IEquatable<SymmetricKey> Members

        public override bool Equals(object obj)
        {
            if (obj == null || typeof(IPassphrase) != obj.GetType())
            {
                return false;
            }
            IPassphrase other = (IPassphrase)obj;

            return Equals(other);
        }

        public override int GetHashCode()
        {
            return CryptoName.GetHashCode() ^ DerivedKey.GetHashCode() ^ Passphrase.GetHashCode();
        }
    }
}