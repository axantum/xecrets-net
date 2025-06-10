using AxCrypt.Abstractions;
using AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Core.Secrets
{
    public class SecretSharedUser
    {
        public SecretSharedUser(EmailAddress userEmail, SecretShareVisibility visibilityType, string groupName = "")
        {
            UserEmail = userEmail;
            VisibilityType = visibilityType;
            Visibility = ShareExpiration(visibilityType);
            Group = groupName;
        }

        private EmailAddress _userEmail;

        public EmailAddress UserEmail
        {
            get { return _userEmail ?? EmailAddress.Empty; }
            set { _userEmail = value; }
        }

        private DateTime? _visibility;

        public DateTime? Visibility
        {
            get { return _visibility ?? DateTime.MinValue; }
            set { _visibility = value; }
        }

        private SecretShareVisibility _visibilityType;

        public SecretShareVisibility VisibilityType
        {
            get { return _visibilityType; }
            set { _visibilityType = value; }
        }

        private string _group;

        public string Group
        {
            get { return _group; }
            set { _group = value; }
        }

        private DateTime ShareExpiration(SecretShareVisibility visibility)
        {
            DateTime currentDateTime = New<INow>().Utc;

            switch (visibility)
            {
                case SecretShareVisibility.Once:
                case SecretShareVisibility.Forever:
                    return DateTime.MaxValue;

                case SecretShareVisibility.OneHour:
                    return currentDateTime.AddHours(1);

                case SecretShareVisibility.OneDay:
                    return currentDateTime.AddDays(1).Date;

                case SecretShareVisibility.OneWeek:
                    return currentDateTime.AddDays(7).Date;

                case SecretShareVisibility.OneMonth:
                    return currentDateTime.AddMonths(1).Date;

                case SecretShareVisibility.OneYear:
                    return currentDateTime.AddYears(1).Date;

                default:
                    throw new ArgumentOutOfRangeException(nameof(visibility), visibility, null);
            }
        }

    }
}
