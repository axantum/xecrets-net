using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Abstractions.Rest
{
    public class RestIdentity
    {
        public RestIdentity()
            : this(String.Empty, String.Empty)
        {
        }

        public RestIdentity(string user, string password)
        {
            User = user;
            Password = password;
        }

        public string User { get; private set; }

        public string Password { get; private set; }
    }
}