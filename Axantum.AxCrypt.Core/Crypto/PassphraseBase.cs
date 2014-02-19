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
            return DerivedKey == other.DerivedKey && Passphrase == other.Passphrase;
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
            return DerivedKey.GetHashCode() ^ Passphrase.GetHashCode();
        }
    }
}