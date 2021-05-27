using AxCrypt.Common;
using AxCrypt.Content;
using AxCrypt.Core.UI;
using AxCrypt.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Desktop.Window
{
    public partial class RenewSubscriptionPromptDialog : StyledMessageBase
    {
        private DoNotShowAgainOptions _dontShowAgainFlag;

        public bool HideDialog { get; set; } = false;

        public RenewSubscriptionPromptDialog()
        {
            _dontShowAgainFlag = DoNotShowAgainOptions.UpgradeSubscriptionWarning;
            if (_dontShowAgainFlag != DoNotShowAgainOptions.None && New<Core.UI.UserSettings>().DoNotShowAgain.HasFlag(_dontShowAgainFlag))
            {
                HideDialog = true;
                return;
            }

            InitializeComponent();
        }

        public RenewSubscriptionPromptDialog(Form parent)
            : this()
        {
            InitializeStyle(parent);

            if (!HideDialog)
            {
                IntializeControls();
            }
        }

        protected override void InitializeContentResources()
        {
            this.Text = Texts.RenewSubscriptionTitle;

            string upgradeSubscriptionPromptText = Texts.UpgradeSubscriptionPromptText;
            upgradeSubscriptionPromptText += Environment.NewLine + Environment.NewLine + "          •   " + System.Text.RegularExpressions.Regex.Replace(Texts.RenewSubscriptionDialogFeaturesList, "\n", "          •   ");
            _labelPromptText.Text = upgradeSubscriptionPromptText;
            _buttonRenewNow.Text = Texts.BusinessSubscriptionRenewMenu;
            _buttonNotNow.Text = Texts.ButtonNotNow;
            _checkboxDontShowThisAgain.Text = Texts.DontShowAgainCheckBoxText;
        }

        private void IntializeControls()
        {
            _buttonNotNow.BackColor = Color.White;
            _buttonNotNow.ForeColor = Color.Black;
            _buttonRenewNow.Click += async (sender, e) => { await RenewSubscriptionAsync(); };
            _buttonNotNow.Click += (sender, e) => { UpdateDoNotShowAgainOptions(); };
        }

        private void UpdateDoNotShowAgainOptions()
        {
            if (_dontShowAgainFlag != DoNotShowAgainOptions.None && _checkboxDontShowThisAgain.Checked)
            {
                New<UserSettings>().DoNotShowAgain = New<UserSettings>().DoNotShowAgain | _dontShowAgainFlag;
            }
        }

        protected async Task RenewSubscriptionAsync()
        {
            UpdateDoNotShowAgainOptions();
            await New<PremiumManager>().BuyPremium(New<KnownIdentities>().DefaultEncryptionIdentity, true);
            this.Close();
        }
    }
}