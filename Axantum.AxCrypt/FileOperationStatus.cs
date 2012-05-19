using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt
{
    public enum FileOperationStatus
    {
        Success,
        UnspecifiedError,
        FileAlreadyExists,
        FileDoesNotExist,
        CannotWriteDestination,
        CannotStartApplication,
        InconsistentState,
        InvalidKey,
    }
}