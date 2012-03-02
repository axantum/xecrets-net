using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Header
{
    public enum HeaderSubKey
    {
        None,
        Hmac,
        Validator,
        Headers,
        Data,
    }
}