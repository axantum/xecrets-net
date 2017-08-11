using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Axantum.AxCrypt.Forms.Implementation
{
    public static class FormsTypes
    {
        public static void Register(Form parent)
        {
            TypeMap.Register.Singleton<IPopup>(() => new Popup(parent));
            TypeMap.Register.Singleton<IVerifySignInPassword>(() => new VerifySignInPassword(parent));
            TypeMap.Register.Singleton<IMainUI>(() => new MainUI(parent));
        }
    }
}