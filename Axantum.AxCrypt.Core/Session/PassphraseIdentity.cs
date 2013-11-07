using Axantum.AxCrypt.Core.Crypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Axantum.AxCrypt.Core.Session
{
    [DataContract(Namespace = "http://www.axantum.com/Serialization/")]
    public class PassphraseIdentity
    {
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