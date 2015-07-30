using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.UI
{
    public interface IEmailParser
    {
        bool TryParse(string email, out string address);
    }
}