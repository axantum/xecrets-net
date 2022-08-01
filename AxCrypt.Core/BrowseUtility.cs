using AxCrypt.Abstractions;
using AxCrypt.Content;
using AxCrypt.Core.Extensions;
using AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Core
{
    public static class BrowseUtility
    {
        private static string GetCurrentLanguageCode()
        {
            string langCode = New<UserSettings>().CultureName.Split('-')[0];
            if (langCode.Length == 0 || langCode.Length > 2)
            {
                return "en";
            }

            return langCode;
        }

        private static Uri AccountWebUriWithLangCode
        {
            get
            {
                return new Uri($"{New<UserSettings>().AccountWebUrl}{GetCurrentLanguageCode()}/");
            }
        }

        public static void RedirectToMyAxCryptIDPage()
        {
            string link = Texts.LinkToLoginUrl.QueryFormat(AccountWebUriWithLangCode, New<UserSettings>().UserEmail);
            New<IBrowser>().OpenUri(new Uri(link));
        }

        public static void RedirectToAccountWebUrl()
        {
            New<IBrowser>().OpenUri(AccountWebUriWithLangCode);
        }

        public static void RedirectToPurchasePage(string userEmail, bool fromRenewSubscriptionDialog, string tag)
        {
            string link = Texts.LinkToAxCryptPremiumPurchasePage.QueryFormat(AccountWebUriWithLangCode, userEmail, tag);
            if (fromRenewSubscriptionDialog)
            {
                link += "&ref=renewalpopup";
            }

            New<IBrowser>().OpenUri(new Uri(link));
        }

        public static void RedirectToChangePasswordUrl(string userEmail)
        {
            string link = Texts.LinktoChangePassword.QueryFormat(AccountWebUriWithLangCode, userEmail);
            New<IBrowser>().OpenUri(new Uri(link));
        }

        public static void RedirectToSecretsUrl(string userEmail)
        {
            string link = string.Format(Texts.LinkToSecretsPageWithUserNameFormat, AccountWebUriWithLangCode, userEmail);
            New<IBrowser>().OpenUri(new Uri(link));
        }

        public static void RedirectToAccountWebUrl(string urlTextWithPlaceHolders)
        {
            string link = string.Format(urlTextWithPlaceHolders, AccountWebUriWithLangCode);
            UriBuilder url = new UriBuilder(link);
            if (New<UserSettings>().UserEmail != null) {
                url.Query = $"email={New<UserSettings>().UserEmail}";
            }

            New<IBrowser>().OpenUri(url.Uri);
        }

        private static string mainSiteUrl = Texts.MainSiteUrl;

        private static Uri MainSiteUriWithLangCode
        {
            get
            {
                return new Uri($"{mainSiteUrl}{GetCurrentLanguageCode()}/");
            }
        }

        public static void RedirectTo(string url)
        {
            New<IBrowser>().OpenUri(new Uri(url.Replace(mainSiteUrl, MainSiteUriWithLangCode.ToString())));
        }
    }
}