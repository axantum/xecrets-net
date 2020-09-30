using AxCrypt.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCrypt.Fake
{
    public class FakeInternetState : IInternetState
    {
        public bool Connected
        {
            get
            {
                return true;
            }
        }

        public IInternetState Clear()
        {
            return this;
        }
    }
}