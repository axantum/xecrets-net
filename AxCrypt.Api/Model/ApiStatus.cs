using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AxCrypt.Api.Model
{
    public enum ApiStatus
    {
        Success = 0,
        PaymentRequired = 1,
        PasswordResetFailed = 2,
    }
}