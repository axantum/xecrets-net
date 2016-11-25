using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Core.UI
{
    public interface IPopup
    {
        PopupButtons Show(PopupButtons buttons, string title, string message);

        Task<PopupButtons> ShowAsync(PopupButtons buttons, string title, string message);

        Task<string> ShowAsync(string[] buttons, string title, string message);
    }
}