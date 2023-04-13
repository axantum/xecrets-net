using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace AxCrypt.Core.Secrets
{
    public class SecretPassword
    {
        public SecretPassword(string title, string url, string description, string userName, string theSecret)
        {
            Title = title;
            Url = url;
            Description = description;
            Username = userName;
            TheSecret = theSecret;
        }

        [AllowNull]
        private string _title;

        public string Title
        {
            get { return _title ?? String.Empty; }
            set { _title = value; }
        }

        [AllowNull]
        private string _url;

        public string Url
        {
            get { return _url ?? String.Empty; }
            set { _url = value; }
        }

        [AllowNull]
        private string _description;

        /// <summary>
        /// A (long) description, not necessarily unique, for this secret
        /// </summary>
        public string Description
        {
            get { return _description ?? String.Empty; }
            set { _description = value; }
        }

        [AllowNull]
        private string _username;

        public string Username
        {
            get { return _username ?? String.Empty; }
            set { _username = value; }
        }

        [AllowNull]
        private string _theSecret;

        /// <summary>
        /// The (short) actual secret - it may be any text
        /// </summary>
        public string TheSecret
        {
            get { return _theSecret ?? String.Empty; }
            set { _theSecret = value; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is empty.
        /// </summary>
        /// <value><c>true</c> if this instance is empty; otherwise, <c>false</c>.</value>
        public bool IsEmpty
        {
            get
            {
                return String.IsNullOrEmpty(Title) && String.IsNullOrEmpty(Url) && String.IsNullOrEmpty(Description) && String.IsNullOrEmpty(Username) && String.IsNullOrEmpty(TheSecret);
            }
        }
    }
}
