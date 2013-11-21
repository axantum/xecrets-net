using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Presentation
{
    public class ButtonPresentation
    {
        private IMainView _mainView;

        public ButtonPresentation(IMainView mainView)
        {
            _mainView = mainView;
        }
    }
}