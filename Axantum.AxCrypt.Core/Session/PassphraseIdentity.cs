using Axantum.AxCrypt.Core.Crypto;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Axantum.AxCrypt.Core.Session
{
    /// <summary>
    /// A passphrase identity, associating a name with a thumbprint and optionally a key.
    /// Instances of this class are immutable.
    /// </summary>
    [DataContract(Namespace = "http://www.axantum.com/Serialization/")]
    public class PassphraseIdentity
    {
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "This type is immutable.")]
        public static readonly PassphraseIdentity Empty = new PassphraseIdentity(String.Empty, AesKey.Zero);

        public PassphraseIdentity(string name)
        {
            Name = name;
        }

        public PassphraseIdentity(string name, AesKey key)
            : this(name)
        {
            Key = key;
            Thumbprint = Key.Thumbprint;
        }

        public AesKey Key { get; private set; }

        [DataMember(Name = "Name")]
        public string Name { get; private set; }

        [DataMember(Name = "Thumbprint")]
        public AesKeyThumbprint Thumbprint { get; private set; }
    }
}