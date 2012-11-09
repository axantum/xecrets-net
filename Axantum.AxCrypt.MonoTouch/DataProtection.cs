using System;
using Axantum.AxCrypt.Core.Runtime;

namespace Axantum.AxCrypt.MonoTouch
{
    /// <summary>
    /// Flow-through implementation of in-memory DataProtection as its concept is not backed by the platform.
    /// </summary>
    public class DataProtection : IDataProtection
    {
        public DataProtection ()
        {
        }
        
#region IDataProtection implementation
        
        byte[] IDataProtection.Protect (byte[] unprotectedData)
        {
            return unprotectedData;
        }
        
        byte[] IDataProtection.Unprotect (byte[] protectedData)
        {
            return protectedData;
        }
        
#endregion
    }
}
