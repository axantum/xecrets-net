using Axantum.AxCrypt.Api.Model;
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
using System.Threading.Tasks;
using System.Windows.Forms;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt
{
    public class PremiumLinkLabel : LinkLabel
    {
        private ToolTip _toolTip = new ToolTip();

        private PlanInformation _planInformation = PlanInformation.Empty;

        public async Task ConfigureAsync(LogOnIdentity identity)
        {
            PlanInformation planInformation = await PlanInformation.CreateAsync(identity);
            if (planInformation == _planInformation)
            {
                return;
            }
            _planInformation = planInformation;

            switch (planInformation.PlanState)
            {
                case PlanState.Unknown:
                    Visible = false;
                    break;

                case PlanState.HasPremium:
                case PlanState.HasBusiness:
                    if (planInformation.DaysLeft > 15)
                    {
                        Visible = false;
                        break;
                    }

                    Text = (planInformation.DaysLeft > 1 ? Texts.DaysLeftPluralWarningPattern : Texts.DaysLeftSingularWarningPattern).InvariantFormat(planInformation.DaysLeft);
                    LinkColor = Styling.WarningColor;
                    _toolTip.SetToolTip(this, Texts.DaysLeftWarningToolTip);
                    Visible = true;
                    break;

                case PlanState.NoPremium:
                    Text = Texts.UpgradePromptText;
                    LinkColor = Styling.WarningColor;
                    _toolTip.SetToolTip(this, Texts.NoPremiumWarning);
                    Visible = true;
                    break;

                case PlanState.CanTryPremium:
                    Text = Texts.TryPremiumLabel;
                    LinkColor = Styling.WarningColor;
                    _toolTip.SetToolTip(this, Texts.TryPremiumToolTip);
                    Visible = true;
                    break;

                case PlanState.OfflineNoPremium:
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

            await ConfigureAsync(New<KnownIdentities>().DefaultEncryptionIdentity);
            IAccountService accountService = New<LogOnIdentity, IAccountService>(New<KnownIdentities>().DefaultEncryptionIdentity);
            switch (_planInformation.PlanState)
            {
                case PlanState.CanTryPremium:
                    await accountService.StartPremiumTrialAsync();
                    await New<IPopup>().ShowAsync(PopupButtons.Ok, Texts.InformationTitle, Texts.TrialPremiumStartInfo);
                    await ConfigureAsync(New<KnownIdentities>().DefaultEncryptionIdentity);
                    await New<SessionNotify>().NotifyAsync(new SessionNotification(SessionNotificationType.RefreshLicensePolicy, New<KnownIdentities>().DefaultEncryptionIdentity));
                    break;

                case PlanState.NoPremium:
                case PlanState.OfflineNoPremium:
                case PlanState.HasPremium:
                    accountService.Refresh();
                    await ConfigureAsync(New<KnownIdentities>().DefaultEncryptionIdentity);
                    await New<SessionNotify>().NotifyAsync(new SessionNotification(SessionNotificationType.RefreshLicensePolicy, New<KnownIdentities>().DefaultEncryptionIdentity));

                    if (_planInformation.PlanState == PlanState.CanTryPremium || _planInformation.DaysLeft > 15)
                    {
                        break;
                    }

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