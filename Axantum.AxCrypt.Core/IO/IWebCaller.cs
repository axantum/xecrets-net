using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.IO
{
    public interface IWebCaller
    {
        string Go(Uri url);
    }
}