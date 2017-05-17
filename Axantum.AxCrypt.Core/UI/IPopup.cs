using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Axantum.AxCrypt.Common;

namespace Axantum.AxCrypt.Core.UI
{
    public interface IPopup
    {
        PopupButtons Show(PopupButtons buttons, string title, string message);

        PopupButtons Show(PopupButtons buttons, string title, string message, DontShowAgain dontShowAgainFlag);

        Task<PopupButtons> ShowAsync(PopupButtons buttons, string title, string message);

        Task<string> ShowAsync(string[] buttons, string title, string message);
    }
}