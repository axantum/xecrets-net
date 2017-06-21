using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Forms;
using Axantum.AxCrypt.Forms.Style;
using Axantum.AxCrypt.Properties;
using System;
using System.Windows.Forms;

using static Axantum.AxCrypt.Abstractions.TypeResolve;
using Texts = AxCrypt.Content.Texts;

namespace Axantum.AxCrypt
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
            this.ProductNameText.Text = New<AboutAssembly>().AssemblyProduct;
            this.VersionText.Text = New<AboutAssembly>().AboutVersionText;
            this.CopyrightText.Text = New<AboutAssembly>().AssemblyCopyright;
            this.CompanyNameText.Text = New<AboutAssembly>().AssemblyCompany;
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
    }
}