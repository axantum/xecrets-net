using Axantum.AxCrypt.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

using Texts = AxCrypt.Content.Texts;

namespace Axantum.AxCrypt
{
    partial class AboutBox : StyledMessageBase
    {
        public AboutBox()
        {
            InitializeComponent();
        }

        public AboutBox(Form parent)
            : this()
        {
            InitializeStyle(parent);
        }

        protected override void InitializeContentResources()
        {
            Text = Texts.About.InvariantFormat(AssemblyProduct);

            Description.Text = Texts.AxCryptAboutDescription;
            okButton.Text = Texts.ButtonOkText;
        }

        private void AboutBox_Load(object sender, EventArgs e)
        {
            this.ProductNameText.Text = AssemblyProduct;
            this.VersionText.Text = AssemblyVersion + (String.IsNullOrEmpty(AssemblyDescription) ? String.Empty : " " + AssemblyDescription);
            this.CopyrightText.Text = AssemblyCopyright;
            this.CompanyNameText.Text = AssemblyCompany;
        }

        #region Assembly Attribute Accessors

        public static string AssemblyTitle
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0)
                {
                    AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                    if (!String.IsNullOrEmpty(titleAttribute.Title))
                    {
                        return titleAttribute.Title;
                    }
                }
                return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
            }
        }

        public static string AssemblyVersion
        {
            get
            {
                return Application.ProductVersion;
            }
        }

        public static string AssemblyDescription
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyDescriptionAttribute)attributes[0]).Description;
            }
        }

        public static string AssemblyProduct
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyProductAttribute)attributes[0]).Product;
            }
        }

        public static string AssemblyCopyright
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
            }
        }

        public static string AssemblyCompany
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCompanyAttribute)attributes[0]).Company;
            }
        }

        #endregion Assembly Attribute Accessors
    }
}