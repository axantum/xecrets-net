using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Axantum.AxCrypt.Core.IO
{
    public class DefaultEncryptedFileFilter : IEncryptedFileFilter
    {
        public bool IsOpenable(string fileName)
        {
            return true;
        }
    }
}