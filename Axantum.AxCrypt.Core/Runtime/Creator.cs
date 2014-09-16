using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Runtime
{
    public class Creator<T>
    {
        public Func<T> CreateFunc { get; set; }

        public Action PostAction { get; set; }

        public Creator(Func<T> creator, Action postAction)
        {
            CreateFunc = creator;
            PostAction = postAction;
        }
    }
}
