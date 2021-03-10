using AxCrypt.Content;
using AxCrypt.Core.Crypto;
using AxCrypt.Core.Extensions;
using AxCrypt.Core.Runtime;
using AxCrypt.Core.Service;
using AxCrypt.Core.Session;
using AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Desktop.Window
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
            LinkColor = System.Drawing.Color.White;

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
                    _toolTip.SetToolTip(this, Texts.DaysLeftWarningToolTip);
                    Visible = true;
                    break;

                case PlanState.NoPremium:
                    Text = Texts.UpgradePromptText;
                    _toolTip.SetToolTip(this, Texts.NoPremiumWarning);
                    Visible = true;
                    break;

                case PlanState.CanTryPremium:
                    Text = Texts.TryPremiumLabel;
                    _toolTip.SetToolTip(this, Texts.TryPremiumToolTip);
                    Visible = true;
                    break;

                case PlanState.OfflineNoPremium:
                    Text = Texts.UpgradePromptText;
                    _toolTip.SetToolTip(this, Texts.OfflineNoPremiumWarning);
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
                    await New<PremiumManager>().BuyPremium(New<KnownIdentities>().DefaultEncryptionIdentity);
                    break;

                case PlanState.NoPremium:
                case PlanState.OfflineNoPremium:
                case PlanState.HasPremium:
                    await New<SessionNotify>().NotifyAsync(new SessionNotification(SessionNotificationType.RefreshLicensePolicy, New<KnownIdentities>().DefaultEncryptionIdentity));

                    if (_planInformation.PlanState == PlanState.CanTryPremium || _planInformation.DaysLeft > 15)
                    {
                        break;
                    }

                    await New<PremiumManager>().BuyPremium(New<KnownIdentities>().DefaultEncryptionIdentity);
                    break;

                default:
                    break;
            }
        }
    }
}