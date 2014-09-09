using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Core.UI
{
    public class EmailAddress
    {
        public string Address { get; set; }

        public EmailAddress(string address)
        {
            Address = address;
        }

        public override string ToString()
        {
            return Address;
        }
    }
}