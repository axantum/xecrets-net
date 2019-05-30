using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.IO
{
    public interface IEncryptedFileFilter 
    {
        /// <summary>
        /// Gets a value indicating whether this file is openable or not.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this file is openable; otherwise, <c>false</c>.
        /// </value>
        bool IsOpenable(IDataItem encryptedDataStore);
    }
}