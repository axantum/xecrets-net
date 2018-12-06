using System;
using System.Collections.Generic;
using System.Linq;

namespace Axantum.AxCrypt.Core.UI
{
    /// <summary>
    /// Display global popup notifications. In Windows, this might be a baloon tip for example.
    /// </summary>
    public interface IGlobalNotification
    {
        /// <summary>
        /// Show a text in a global notification area. It should disappear by itself if not closed by the user.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="text"></param>
        void ShowTransient(string title, string text);
    }
}