using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AxCrypt.Core.Secrets
{
    public class SecretNote
    {
        public SecretNote(string description, string note)
        {
            Description = description;
            Note = note;
        }

        private string _description;

        /// <summary>
        /// A (long) description, not necessarily unique, for this secret
        /// </summary>
        public string Description
        {
            get { return _description ?? String.Empty; }
            set { _description = value; }
        }

        private string _note;

        /// <summary>
        /// The (short) actual secret - it may be any text
        /// </summary>
        public string Note
        {
            get { return _note ?? String.Empty; }
            set { _note = value; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is empty.
        /// </summary>
        /// <value><c>true</c> if this instance is empty; otherwise, <c>false</c>.</value>
        public bool IsEmpty
        {
            get
            {
                return String.IsNullOrEmpty(Description) && String.IsNullOrEmpty(Note);
            }
        }
    }
}