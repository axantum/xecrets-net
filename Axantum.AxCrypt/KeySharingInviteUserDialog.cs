#region Coypright and License

/*
 * AxCrypt - Copyright 2019, Svante Seleborg, All Rights Reserved
 *
 * This file is part of AxCrypt.
 *
 * AxCrypt is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * AxCrypt is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with AxCrypt.  If not, see <http://www.gnu.org/licenses/>.
 *
 * The source is maintained at http://bitbucket.org/axantum/axcrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Forms;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Windows.Forms;
using Texts = AxCrypt.Content.Texts;

namespace Axantum.AxCrypt
{
    public partial class KeySharingInviteUserDialog : StyledMessageBase
    {
        public KeySharingInviteUserDialog()
        {
            InitializeComponent();
        }

        public KeySharingInviteUserDialog(Form parent)
            : this()
        {
            InitializeStyle(parent);
        }

        public List<object> CultureList { get; set; }

        private async void KeySharingInviteUserDialog_Load(object sender, EventArgs e)
        {
            CultureList = await Resolve.KnownIdentities.DefaultEncryptionIdentity.GetCultureInfoList();
            _languageCultureDropDown.DataSource = new BindingSource(CultureList, null);

            SetValuesForOptionalFields();
        }

        private void SetValuesForOptionalFields()
        {
            _languageCultureDropDown.SelectedValue = Resolve.UserSettings.MessageCulture;

            if (!string.IsNullOrEmpty(Resolve.UserSettings.PersonalizedMessage))
            {
                _personalizedMessageTextGroupBox.Visible = true;
                _personalizedMessageTextBox.Text = WebUtility.HtmlDecode(Resolve.UserSettings.PersonalizedMessage);
            }
        }

        protected override void InitializeContentResources()
        {
            Text = Texts.DialogKeyShareInviteUserTitle;

            _languageCultureGroupBox.Text = Texts.OptionsLanguageToolStripMenuItemText;
            _cancelButton.Text = "&" + Texts.ButtonCancelText;
            _okButton.Text = "&" + Texts.ButtonOkText;
            _keyShareInvitePromptlabel.Text = Texts.KeyShareInviteUserTextPrompt;
            _personalizedMessageTitleLabel.Text = Texts.InviteUserPersonalizedMessageTitle;
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            if (DialogResult != DialogResult.OK)
            {
                DialogResult = DialogResult.Cancel;
                return;
            }

            string personalizedMessage = WebUtility.HtmlEncode(_personalizedMessageTextBox.Text);
            if (!String.IsNullOrEmpty(personalizedMessage))
            {
                Resolve.UserSettings.PersonalizedMessage = personalizedMessage;
            }

            CultureInfo messageCulture = new CultureInfo(_languageCultureDropDown.SelectedValue.ToString());
            Resolve.UserSettings.MessageCulture = messageCulture.Name;
        }

        private void ExpandCollapseIcon_Click(object sender, EventArgs e)
        {
            _personalizedMessageTextGroupBox.Visible = !_personalizedMessageTextGroupBox.Visible;
        }
    }
}