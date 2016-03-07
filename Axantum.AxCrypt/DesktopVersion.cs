using Axantum.AxCrypt.Core.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt
{
    internal class DesktopVersion : IVersion
    {
        public Version Current
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version;
            }
        }
    }
}