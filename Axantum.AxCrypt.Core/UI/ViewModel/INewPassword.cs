using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.UI.ViewModel
{
    public interface INewPassword : IPasswordEntry
    {
        string Verification { get; set; }
    }
}