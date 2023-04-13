using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AxCrypt.Core.UI.ViewModel
{
    public class AxCryptUserAccountViewModel
    {
        private static Api.Model.UserAccount _userAccount = new Api.Model.UserAccount();

        public void Initilaize(Api.Model.UserAccount? userAccount)
        {
            if (userAccount != null)
            {
                _userAccount = userAccount;
            }
        }

        public static bool HadAnyPaidSubscription
        {
            get
            {
                return _userAccount.HadAnyPaidSubscription;
            }
        }
    }
}
