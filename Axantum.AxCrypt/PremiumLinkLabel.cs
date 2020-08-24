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

            UpdateText();
        }

        public void UpdateText()
        {
            switch (_planInformation.PlanState)
            {
                case PlanState.Unknown:
                    Visible = false;
                    break;

                case PlanState.HasPremium:
                case PlanState.HasBusiness:
                    if (_planInformation.DaysLeft > 15)
                    {
                        Visible = false;
                        break;
                    }

                    Text = (_planInformation.DaysLeft > 1 ? Texts.DaysLeftPluralWarningPattern : Texts.DaysLeftSingularWarningPattern).InvariantFormat(_planInformation.DaysLeft);
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

            IAccountService accountService = New<LogOnIdentity, IAccountService>(New<KnownIdentities>().DefaultEncryptionIdentity);
            accountService.Refresh();
            await ConfigureAsync(New<KnownIdentities>().DefaultEncryptionIdentity);

            switch (_planInformation.PlanState)
            {
                case PlanState.CanTryPremium:
                    await New<KnownIdentities>().DefaultEncryptionIdentity.DisplayPremiumPurchasePage();
                    break;

                case PlanState.NoPremium:
                case PlanState.OfflineNoPremium:
                case PlanState.HasPremium:
                    await New<SessionNotify>().NotifyAsync(new SessionNotification(SessionNotificationType.RefreshLicensePolicy, New<KnownIdentities>().DefaultEncryptionIdentity));

                    if (_planInformation.PlanState == PlanState.CanTryPremium || _planInformation.DaysLeft > 15)
                    {
                        break;
                    }

                    await New<KnownIdentities>().DefaultEncryptionIdentity.DisplayPremiumPurchasePage();
                    break;

                default:
                    break;
            }
        }
    }
}