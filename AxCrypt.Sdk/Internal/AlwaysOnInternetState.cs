using Axantum.AxCrypt.Abstractions;
using System;
using System.Collections.Generic;

namespace AxCrypt.Sdk.Internal
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