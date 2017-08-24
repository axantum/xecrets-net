using Axantum.AxCrypt.Common;
using Axantum.AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Fake
{
    public class FakePopup : IPopup
    {
        public Task<string> ShowAsync(string[] buttons, string title, string message)
        {
            return Task.FromResult(buttons.First());
        }

        public Task<PopupButtons> ShowAsync(PopupButtons buttons, string title, string message)
        {
            return Task.FromResult(PopupButtons.Ok);
        }

        public Task<string> ShowAsync(string[] buttons, string title, string message, DoNotShowAgainOptions doNotShowAgainOption)
        {
            return Task.FromResult(buttons.First());
        }

        public Task<PopupButtons> ShowAsync(PopupButtons buttons, string title, string message, DoNotShowAgainOptions doNotShowAgainOption)
        {
            return Task.FromResult(PopupButtons.Ok);
        }

        public Task<PopupButtons> ShowAsync(PopupButtons buttons, string title, string message, DoNotShowAgainOptions doNotShowAgainOption, string doNotShowAgainCustomText)
        {
            return Task.FromResult(PopupButtons.Ok);
        }
    }
}