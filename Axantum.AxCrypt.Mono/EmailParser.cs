using Axantum.AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;

namespace Axantum.AxCrypt.Mono
{
    public class EmailParser : IEmailParser
    {
        public bool TryParse(string email, out string address)
        {
            address = String.Empty;
            if (String.IsNullOrEmpty(email))
            {
                return false;
            }

            try
            {
                MailAddress mailAddress = new MailAddress(email);
                address = mailAddress.Address.ToLowerInvariant();
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}