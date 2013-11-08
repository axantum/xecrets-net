using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Axantum.AxCrypt.Core
{
    /// <summary>
    /// Provides syntactically convenient access to the various class factories registered.
    /// </summary>
    public static class Factory
    {
        public static AxCryptFile AxCryptFile
        {
            get
            {
                return FactoryRegistry.Instance.Create<AxCryptFile>();
            }
        }
    }
}