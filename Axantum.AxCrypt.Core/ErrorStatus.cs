using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core
{
    public enum ErrorStatus
    {
        Success,
        MagicGuidMissing,
        InternalError,
        EndOfStream,
        TooNewFileFormatVersion,
        FileFormatError,
        HmacValidationError,
        DataError,
    }
}