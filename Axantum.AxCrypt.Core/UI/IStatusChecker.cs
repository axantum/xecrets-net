using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.UI
{
    public interface IStatusChecker
    {
        bool CheckStatusAndShowMessage(FileOperationStatus status, string displayContext);
    }
}