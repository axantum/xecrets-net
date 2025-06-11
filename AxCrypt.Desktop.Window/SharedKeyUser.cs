using AxCrypt.Api.Model;
using AxCrypt.Core.UI;
using AxCrypt.Desktop.Window.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace AxCrypt.Desktop.Window
{
    public class ShareKeyUser : IEquatable<ShareKeyUser>
    {
        public ShareKeyUser(EmailAddress userEmail, AccountStatus userAccountStatus)
        {
            UserEmail = userEmail.Address;
            Image = Resources.ContactsIcon;
            if (userAccountStatus == AccountStatus.Verified)
            {
                Image = Resources.AxCryptLogo;
            }
        }
        
        public ShareKeyUser(EmailAddress userEmail, string groupName)
        {
            UserEmail = userEmail.Address;
            GroupName = groupName;
            Image = Resources.IcoGrp;
        }

        public string UserEmail { get; set; }

        public string GroupName { get; set; }

        public Image Image { get; set; }

        public Image DotImage { get; } = Resources.DotsIcon;

        public bool Equals(ShareKeyUser other)
        {
            if ((object)other == null)
            {
                return false;
            }

            return UserEmail == other.UserEmail && Image == other.Image && DotImage == other.DotImage;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || typeof(ShareKeyUser) != obj.GetType())
            {
                return false;
            }
            ShareKeyUser other = (ShareKeyUser)obj;

            return Equals(other);
        }

        public override int GetHashCode()
        {
            return UserEmail.GetHashCode() ^ Image.GetHashCode() ^ DotImage.GetHashCode();
        }

        public static bool operator ==(ShareKeyUser left, ShareKeyUser right)
        {
            if (Object.ReferenceEquals(left, right))
            {
                return true;
            }
            if ((object)left == null)
            {
                return false;
            }
            return left.Equals(right);
        }

        public static bool operator !=(ShareKeyUser left, ShareKeyUser right)
        {
            return !(left == right);
        }
    }
}