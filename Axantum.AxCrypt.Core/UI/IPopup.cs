using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Axantum.AxCrypt.Common;

namespace Axantum.AxCrypt.Core.UI
{
    /// <summary>
    /// Display modal alert messages. These need to be async because on some platforms (notably mobile) the implementation
    /// must be async.
    /// </summary>
    public interface IPopup
    {
        Task<PopupButtons> ShowAsync(PopupButtons buttons, string title, string message);

        Task<PopupButtons> ShowAsync(PopupButtons buttons, string title, string message, DontShowAgain dontShowAgainFlag);

        Task<string> ShowAsync(string[] buttons, string title, string message);

        Task<string> ShowAsync(string[] buttons, string title, string message, DontShowAgain dontShowAgainFlag);
    }
}