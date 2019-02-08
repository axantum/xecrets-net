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

using Axantum.AxCrypt.Core.UI.ViewModel;
using Axantum.AxCrypt.Forms;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Texts = AxCrypt.Content.Texts;

namespace Axantum.AxCrypt
{
    public partial class InviteNewContactKeySharingDialog : StyledMessageBase
    {
        private object[] _languageList;

        private SharingListViewModel _viewModel;

        public InviteNewContactKeySharingDialog()
        {
            InitializeComponent();
        }

        public InviteNewContactKeySharingDialog(Form parent, SharingListViewModel viewModel)
            : this()
        {
            _viewModel = viewModel;
            InitializeStyle(parent);
            InitializeCultureList();
        }

        private void InitializeCultureList()
        {
            _languageList = new[] {
                new { Name = Texts.EnglishLanguageToolStripMenuItemText, Tag = "en-US" },
                new { Name = Texts.GermanLanguageSelectionText, Tag = "de-DE" },
                new { Name = Texts.DutchLanguageSelection, Tag = "nl-NL" },
                new { Name = Texts.SpanishLanguageToolStripMenuItemText, Tag = "es-ES" },
                new { Name = Texts.FrancaisLanguageToolStripMenuItemText, Tag = "fr-FR" },
                new { Name = Texts.ItalianLanguageSelection, Tag = "it-IT" },
                new { Name = Texts.KoreanLanguageSelection, Tag = "ko" },
                new { Name = Texts.PolishLanguageToolStripMenuItemText, Tag = "pl-PL" },
                new { Name = Texts.PortugueseBrazilLanguageSelection, Tag = "pt-BR" },
                new { Name = Texts.RussianLanguageSelection, Tag = "ru-RU" },
                new { Name = Texts.SwedishLanguageToolStripMenuItemText, Tag = "sv-SE" },
                new { Name = Texts.TurkishLanguageToolStripMenuItemText, Tag = "tr-TR" }
            };

            _languageCultureDropDown.DataSource = new BindingSource(_languageList, null);
        }

        protected override void InitializeContentResources()
        {
            Text = Texts.DialogKeyShareInviteUserTitle;

            _languageCultureGroupBox.Text = Texts.OptionsLanguageToolStripMenuItemText;
            _cancelButton.Text = "&" + Texts.ButtonCancelText;
            _okButton.Text = "&" + Texts.ButtonOkText;
            _keyShareInvitePromptlabel.Text = Texts.KeyShareInviteUserTextPrompt;
            _personalizedMessageGroupBox.Text = Texts.InviteUserPersonalizedMessageTitle;
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            if (DialogResult != DialogResult.OK)
            {
                DialogResult = DialogResult.Cancel;
                return;
            }

            CultureInfo messageCulture = new CultureInfo(_languageCultureDropDown.SelectedValue.ToString());
            string invitationPersonalizedMessage = _personalizedMessageTextBox.Text;
            _viewModel.SetInvitationPersonalizedMessage(messageCulture, invitationPersonalizedMessage);
        }
    }
}