using AxCrypt.Core.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCrypt.Fake
{
    public class FakeVersion : IVersion
    {
        public Version Current
        {
            get
            {
                return new Version("2.0.1234.0");
            }
        }
    }
}