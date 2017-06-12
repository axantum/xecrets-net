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
        private PremiumStatus? _status;

        public async Task ConfigureAsync(LogOnIdentity identity)
        {
            PremiumInfo pi = await PremiumInfo.CreateAsync(identity);
            _status = pi.PremiumStatus;
            switch (pi.PremiumStatus)
            {
                case PremiumStatus.Unknown:
                    Visible = false;
                    break;

                case PremiumStatus.HasPremium:
                    if (pi.DaysLeft > 15)
                    {
                        Visible = false;
                        break;
                    }

                    Text = (pi.DaysLeft > 1 ? Texts.DaysLeftPluralWarningPattern : Texts.DaysLeftSingularWarningPattern).InvariantFormat(pi.DaysLeft);
                    LinkColor = Styling.WarningColor;
                    _toolTip.SetToolTip(this, Texts.DaysLeftWarningToolTip);
                    Visible = true;
                    break;

                case PremiumStatus.NoPremium:
                    Text = Texts.UpgradePromptText;
                    LinkColor = Styling.WarningColor;
                    _toolTip.SetToolTip(this, Texts.NoPremiumWarning);
                    Visible = true;
                    break;

                case PremiumStatus.CanTryPremium:
                    Text = Texts.TryPremiumLabel;
                    LinkColor = Styling.WarningColor;
                    _toolTip.SetToolTip(this, Texts.TryPremiumToolTip);
                    Visible = true;
                    break;

                case PremiumStatus.OfflineNoPremium:
                    Text = Texts.UpgradePromptText;
                    _toolTip.SetToolTip(this, Texts.OfflineNoPremiumWarning);
                    LinkColor = Styling.WarningColor;
                    Visible = true;
                    break;

                default:
                    break;
            }
        }

        protected override async void OnClick(EventArgs e)
        {
            base.OnClick(e);
            await ConfigureAsync(New<KnownIdentities>().DefaultEncryptionIdentity);
            if (!Visible)
            {
                await New<SessionNotify>().NotifyAsync(new SessionNotification(SessionNotificationType.LicensePolicyChange, New<KnownIdentities>().DefaultEncryptionIdentity));
            }
            await PremiumWarningClickAsync();
        }

        private async Task PremiumWarningClickAsync()
        {
            if (New<KnownIdentities>().DefaultEncryptionIdentity == LogOnIdentity.Empty)
            {
                await New<IPopup>().ShowAsync(PopupButtons.Ok, Texts.ApplicationTitle, Texts.SignInBeforePremiumMessage);
                New<UserSettings>().RestoreFullWindow = true;
                return;
            }

            IAccountService accountService = New<LogOnIdentity, IAccountService>(New<KnownIdentities>().DefaultEncryptionIdentity);

            switch (_status.GetValueOrDefault())
            {
                case PremiumStatus.CanTryPremium:
                    await accountService.StartPremiumTrialAsync();
                    await New<IPopup>().ShowAsync(PopupButtons.Ok, Texts.InformationTitle, Texts.TrialPremiumStartInfo);
                    await ConfigureAsync(New<KnownIdentities>().DefaultEncryptionIdentity);
                    await New<SessionNotify>().NotifyAsync(new SessionNotification(SessionNotificationType.LicensePolicyChange, New<KnownIdentities>().DefaultEncryptionIdentity));
                    break;

                case PremiumStatus.NoPremium:
                case PremiumStatus.OfflineNoPremium:
                    await DisplayPremiumPurchasePage(accountService);
                    break;

                default:
                    break;
            }
        }

        private static async Task DisplayPremiumPurchasePage(IAccountService accountService)
        {
            string tag = New<KnownIdentities>().IsLoggedOn ? (await accountService.AccountAsync()).Tag ?? string.Empty : string.Empty;
            string link = Texts.LinkToAxCryptPremiumPurchasePage.QueryFormat(Resolve.UserSettings.AccountWebUrl, New<KnownIdentities>().DefaultEncryptionIdentity.UserEmail, tag);
            Process.Start(link);
        }
    }
}