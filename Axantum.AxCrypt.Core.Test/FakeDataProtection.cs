using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Axantum.AxCrypt.Core.System;

namespace Axantum.AxCrypt.Core.Test
{
    internal class FakeDataProtection : IDataProtection
    {
        #region IDataProtection Members

        public byte[] Protect(byte[] unprotectedData)
        {
            return (byte[])unprotectedData.Clone();
        }

        public byte[] Unprotect(byte[] protectedData)
        {
            return (byte[])protectedData.Clone();
        }

        #endregion IDataProtection Members
    }
}