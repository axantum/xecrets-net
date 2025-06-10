using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCrypt.Core.UI.SecuredMessenger
{
    public enum SecMsgSearchFilters
    {
        [Category("IgnoreSelectList")]
        None,
        OneWeek,
        OneMonth,
        ThreeMonth,
        SixMonth,
        OneYear
    }
}
