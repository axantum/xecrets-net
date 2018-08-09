using Axantum.AxCrypt.Core.Extensions;
using System.Text.RegularExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Core.UI
{
    public class RegexEmailParser : IEmailParser
    {
        private static Regex validEmailChars = new Regex(@"^[-A-Za-z0-9@.+'_]+$");

        private static Regex validEmail = new Regex(@"^[^\.@]+(\.[^\.@]+)*@[-a-zA-Z0-9]{1,63}(\.[-a-zA-Z0-9]{1,63})+$");

        private static Regex matchEmail = new Regex(@"[-A-Za-z0-9+'_]+(\.[-A-Za-z0-9+'_]+)*@[-a-zA-Z0-9]{1,63}(\.[-a-zA-Z0-9]{1,63})+");

        /// <summary>
        /// Determines whether the specified email is a valid string for the purpose.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <returns>
        /// 	<c>true</c> if email is valid; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This is a stricter evaluation than the full set of allowable e-mail adresses to faciliate early detection of bad e-mails. The set
        /// of allowed characters is based on a set of 175000 e-mails actually validated by AxCrypt.
        /// </remarks>
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrEmpty(email) || email.Length > 200 || !validEmailChars.IsMatch(email))
            {
                return false;
            }

            if (!validEmail.IsMatch(email))
            {
                return false;
            }

            if (!email.Split('.').Last().IsValidTopLevelDomain())
            {
                return false;
            }
            return true;
        }

        public bool TryParse(string email, out string address)
        {
            address = String.Empty;
            if (String.IsNullOrEmpty(email))
            {
                return false;
            }
            if (!IsValidEmail(email))
            {
                return false;
            }
            address = email;
            return true;
        }

        public IEnumerable<string> Extract(string text)
        {
            foreach (string candidate in matchEmail.Matches(text ?? string.Empty).Cast<Match>().Select(m => m.Value))
            {
                string validated;
                if (TryParse(candidate, out validated))
                {
                    yield return validated;
                }
            }
        }
    }
}
