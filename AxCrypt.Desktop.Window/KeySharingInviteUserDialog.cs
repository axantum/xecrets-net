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
 * The source is maintained at http://bitbucket.org/AxCrypt.Desktop.Window-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using AxCrypt.Common;
using AxCrypt.Core;
using AxCrypt.Forms;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using static AxCrypt.Abstractions.TypeResolve;
using Texts = AxCrypt.Content.Texts;

namespace AxCrypt.Desktop.Window
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

        private async void KeySharingInviteUserDialog_Load(object sender, EventArgs e)
        {
            _languageCultureDropDown.DataSource = (await New<CultureNameMapper>().GetCultureMap()).AsEnumerable().ToList();

            SetValuesForOptionalFields();
        }

        private void SetValuesForOptionalFields()
        {
            _languageCultureDropDown.SelectedValue = Resolve.UserSettings.MessageCulture;

            if (!string.IsNullOrEmpty(Resolve.UserSettings.CustomInvitationMessage))
            {
                _personalizedMessageTextGroupBox.Visible = true;
                _personalizedMessageTextBox.Text = Resolve.UserSettings.CustomInvitationMessage;
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

            string personalizedMessage = _personalizedMessageTextBox.Text;
            if (!string.IsNullOrEmpty(personalizedMessage))
            {
                Resolve.UserSettings.CustomInvitationMessage = personalizedMessage;
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