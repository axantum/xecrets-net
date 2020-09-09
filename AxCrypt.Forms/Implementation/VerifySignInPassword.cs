using AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace AxCrypt.Forms.Implementation
{
    public class VerifySignInPassword : VerifySignInPasswordBase
    {
        private Form _parent;

        public VerifySignInPassword(Form parent)
        {
            _parent = parent;
        }

        protected override bool VerifyDialog(string description)
        {
            return _parent.ShowVerifySignInPasswordDialog(description);
        }
    }
}