using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace AxCrypt.Core.Secrets
{
    public class SecretCard
    {
        public SecretCard(string number, string description, string nameOnCard, string securityCode, string expirationDate)
        {
            Number = number;
            Description = description;
            NameOnCard = nameOnCard;
            SecurityCode = securityCode;
            ExpirationDate = expirationDate;
        }

        [AllowNull]
        private string _number;

        public string Number
        {
            get { return _number ?? String.Empty; }
            set { _number = value; }
        }

        [AllowNull]
        private string _description;

        /// <summary>
        /// A (long) description, not necessarily unique, for this card
        /// </summary>
        public string Description
        {
            get { return _description ?? String.Empty; }
            set { _description = value; }
        }

        [AllowNull]
        private string _nameOnCard;

        public string NameOnCard
        {
            get { return _nameOnCard ?? String.Empty; }
            set { _nameOnCard = value; }
        }

        [AllowNull]
        private string _securityCode;

        public string SecurityCode
        {
            get { return _securityCode ?? String.Empty; }
            set { _securityCode = value; }
        }

        [AllowNull]
        private string _expirationDate;

        public string ExpirationDate
        {
            get { return _expirationDate ?? String.Empty; }
            set { _expirationDate = value; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is empty.
        /// </summary>
        /// <value><c>true</c> if this instance is empty; otherwise, <c>false</c>.</value>
        public bool IsEmpty
        {
            get
            {
                return String.IsNullOrEmpty(Number) && String.IsNullOrEmpty(Description) && String.IsNullOrEmpty(NameOnCard) && String.IsNullOrEmpty(SecurityCode) && String.IsNullOrEmpty(ExpirationDate);
            }
        }
    }
}
