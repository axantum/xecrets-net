using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Mono
{
    public class EmailParser : IEmailParser
    {
        public IEnumerable<string> Extract(string text)
        {
            foreach (string email in new RegexEmailParser().Extract(text))
            {
                string address;
                if (TryParseInternal(email, out address))
                {
                    yield return address;
                }
            }
        }

        public bool TryParse(string email, out string address)
        {
            if (! new RegexEmailParser().TryParse(email, out address))
            {
                return false;
            }

            return TryParseInternal(email, out address);
        }

        private bool TryParseInternal(string email, out string address)
        {
            try
            {
                MailAddress mailAddress = new MailAddress(email);
                address = mailAddress.Address.ToLowerInvariant();
                return true;
            }
            catch (FormatException fex)
            {
                address = null;
                New<IReport>().Exception(fex);
                return false;
            }
        }
    }
}