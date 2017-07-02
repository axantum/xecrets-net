using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.UI.ViewModel
{
    public interface INewPassword
    {
        string Password { get; set; }

        string Verification { get; set; }

        bool ShowPassword { get; set; }
    }
}