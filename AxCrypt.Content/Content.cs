using AxCrypt.Content.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace AxCrypt.Content
{
    public class Content : Resources
    {
        public string Template_ActivateEmail_cshtml { get { return LongResources.Template_ActivateEmail_cshtml; } }

        public string Template_AppActivateEmail_cshtml { get { return LongResources.Template_AppActivateEmail_cshtml; } }

        public string Template_AppActivateEmailPlainText_cshtml { get { return LongResources.Template_AppActivateEmailPlainText_cshtml; } }

        public string Template_ChangeEmailConfirmEmail_cshtml { get { return LongResources.Template_ChangeEmailConfirmEmail_cshtml; } }

        public string Template_ChangeEmailExistEmail_cshtml { get { return LongResources.Template_ChangeEmailExistEmail_cshtml; } }

        public string Template_EmailLayout_cshtml { get { return LongResources.Template_EmailLayout_cshtml; } }

        public string Template_InvitationEmail_cshtml { get { return LongResources.Template_InvitationEmail_cshtml; } }

        public string Template_InvitationEmailPlainText_cshtml { get { return LongResources.Template_InvitationEmailPlainText_cshtml; } }

        public string Template_PasswordResetEmail_cshtml { get { return LongResources.Template_PasswordResetEmail_cshtml; } }

        public string Template_UnregisterEmail_cshtml { get { return LongResources.Template_UnregisterEmail_cshtml; } }

        public static new ResourceManager ResourceManager { get { return new ContentResourceManager(); } }
    }
}