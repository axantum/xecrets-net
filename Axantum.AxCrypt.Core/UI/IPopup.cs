using System;
using System.Collections.Generic;
using System.Linq;
using Axantum.AxCrypt.Common;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Core.UI
{
    /// <summary>
    /// Display modal alert messages. These need to be async because on some platforms (notably mobile) the implementation
    /// must be async.
    /// </summary>
    public interface IPopup
    {
        Task<PopupButtons> ShowAsync(PopupButtons buttons, string title, string message);

        Task<PopupButtons> ShowAsync(PopupButtons buttons, string title, string message, DoNotShowAgainOptions doNotShowAgainOption);

        Task<PopupButtons> ShowAsync(PopupButtons buttons, string title, string message, DoNotShowAgainOptions doNotShowAgainOption, string doNotShowAgainCustomText);

        Task<string> ShowAsync(string[] buttons, string title, string message);

        Task<string> ShowAsync(string[] buttons, string title, string message, DoNotShowAgainOptions doNotShowAgainOption);
    }
}