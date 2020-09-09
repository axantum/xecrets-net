using AxCrypt.Core.Extensions;
using AxCrypt.Core.Runtime;
using AxCrypt.Core.UI;
using AxCrypt.Forms;
using AxCrypt.Forms.Style;
using AxCrypt.Desktop.Window.Properties;
using System;
using System.Windows.Forms;

using static AxCrypt.Abstractions.TypeResolve;
using Texts = AxCrypt.Content.Texts;

namespace AxCrypt.Desktop.Window
{
    partial class AboutBox : StyledMessageBase
    {
        public AboutBox()
        {
            InitializeComponent();
            new Styling(Resources.axcrypticon).Style(this);
        }

        protected override void InitializeContentResources()
        {
            Text = Texts.About.InvariantFormat(New<AboutAssembly>().AssemblyProduct);

            Description.Text = Texts.AxCryptAboutDescription;
            okButton.Text = Texts.ButtonOkText;
        }

        private void AboutBox_Load(object sender, EventArgs e)
        {
            ProductNameText.Text = New<AboutAssembly>().AssemblyProduct;
            VersionText.Text = New<AboutAssembly>().AboutVersionText;
            CopyrightText.Text = New<AboutAssembly>().AssemblyCopyright;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            Hide();
        }

        public void ShowNow()
        {
            Show();
            Activate();
            Focus();
            BringToFront();
        }

        private void AboutBox_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }

        private void AboutBox_VisibleChanged(object sender, EventArgs e)
        {
            if (Visible)
            {
                SubscriptionStatusAndExpirationText.Text = new Display().GetLicenseStatusAndExpiration();
            }
        }
    }
}