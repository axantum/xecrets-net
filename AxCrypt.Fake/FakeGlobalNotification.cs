using AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AxCrypt.Fake
{
    public class FakeGlobalNotification : IGlobalNotification
    {
        public void ShowTransient(string title, string text)
        {
            return;
        }
    }
}