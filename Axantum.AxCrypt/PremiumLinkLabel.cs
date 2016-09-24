using Axantum.AxCrypt.Common;
using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.Service;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.UI;
using Axantum.AxCrypt.Forms.Style;
using AxCrypt.Content;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt
{
    public class PremiumLinkLabel : LinkLabel
    {
        private ToolTip _toolTip = new ToolTip();

        public async Task ConfigureAsync(LicensePolicy license)
        {
            if (await license.SubscriptionWarningTimeAsync() == TimeSpan.MaxValue)
            {
                Visible = false;
                return;
            }

            if (await license.SubscriptionWarningTimeAsync() == TimeSpan.Zero)
            {
                await SetUpgradeOrTrialAsync(license);
                return;
            }

            int days = (await license.SubscriptionWarningTimeAsync()).Days;
            Text = (days > 1 ? Texts.DaysLeftPluralWarningPattern : Texts.DaysLeftSingularWarningPattern).InvariantFormat(days);
            LinkColor = Styling.WarningColor;
            _toolTip.SetToolTip(this, Texts.DaysLeftWarningToolTip);
            Visible = true;
        }

        private async Task SetUpgradeOrTrialAsync(LicensePolicy license)
        {
            if (New<AxCryptOnlineState>().IsOffline)
            {
                Text = Texts.UpgradePromptText;
                _toolTip.SetToolTip(this, Texts.OfflineNoPremiumWarning);
                LinkColor = Styling.WarningColor;
                Visible = true;
                return;
            }

            if (!await license.IsTrialAvailableAsync())
            {
                Text = Texts.UpgradePromptText;
                LinkColor = Styling.WarningColor;
                _toolTip.SetToolTip(this, Texts.NoPremiumWarning);
                Visible = true;
                return;
            }

            Text = Texts.TryPremiumLabel;
            LinkColor = Styling.WarningColor;
            _toolTip.SetToolTip(this, Texts.TryPremiumToolTip);
            Visible = true;
        }

        protected override async void OnClick(EventArgs e)
        {
            base.OnClick(e);
            await PremiumWarningClickAsync();
        }

        private async Task PremiumWarningClickAsync()
        {
            IAccountService accountService = New<LogOnIdentity, IAccountService>(New<KnownIdentities>().DefaultEncryptionIdentity);

            if (Text == Texts.TryPremiumLabel)
            {
                await accountService.StartPremiumTrialAsync();
                await New<IPopup>().ShowAsync(PopupButtons.Ok, Texts.InformationTitle, Texts.TrialPremiumStartInfo);
                await ConfigureAsync(New<LicensePolicy>());
                New<SessionNotify>().Notify(new SessionNotification(SessionNotificationType.LicensePolicyChange));
                return;
            }

            await DisplayPremiumPurchasePage(accountService);
            return;
        }

        private static async Task DisplayPremiumPurchasePage(IAccountService accountService)
        {
            string tag = New<KnownIdentities>().IsLoggedOn ? (await accountService.AccountAsync()).Tag ?? string.Empty : string.Empty;
            string link = Texts.LinkToAxCryptPremiumPurchasePage.QueryFormat(Resolve.UserSettings.AccountWebUrl, New<KnownIdentities>().DefaultEncryptionIdentity.UserEmail, tag);
            Process.Start(link);
        }
    }
}