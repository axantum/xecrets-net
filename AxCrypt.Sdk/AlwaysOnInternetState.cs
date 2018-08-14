using Axantum.AxCrypt.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace AxCrypt.Sdk
{
    internal class AlwaysOnInternetState : IInternetState
    {
        public bool Connected => true;

        public IInternetState Clear()
        {
            return this;
        }
    }
}
