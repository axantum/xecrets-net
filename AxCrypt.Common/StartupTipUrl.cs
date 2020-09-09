using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCrypt.Common
{
    public class StartupTipUrl
    {
        private Uri _url;

        public StartupTipUrl(Uri url)
        {
            _url = url;
        }

        public override string ToString()
        {
            return _url.ToString();
        }
    }
}