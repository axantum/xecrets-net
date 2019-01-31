#region Coypright and License

/*
 * AxCrypt - Copyright 2016, Svante Seleborg, All Rights Reserved
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

        public InviteNewContactKeySharingDialog()
        {
            InitializeComponent();
        }

        public InviteNewContactKeySharingDialog(Form parent)
            : this()
        {
            InitializeStyle(parent);
            InitializeCulture();
        }

        private void InitializeCulture()
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
            Text = " Invite keyShare Dialog";

            _languageCultureGroupBox.Text = "[Choose Language]";
            _cancelButton.Text = "&" + Texts.ButtonCancelText;
            _okButton.Text = "&" + Texts.ButtonOkText;
            _keyShareInvitePromptlabel.Text = "You are about to share securely with someone Who has received but not yet respond" +
    " to an invitation to use AxCrypt ";

            _personalizedMessageGroupBox.Text = "[Invitation Personalized Message]";
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            if (DialogResult != DialogResult.OK)
            {
                DialogResult = DialogResult.Cancel;
                return;
            }

            SharingListViewModel.InvitationCulture = new CultureInfo(_languageCultureDropDown.SelectedValue.ToString());
            SharingListViewModel.InvitationPersonalizedMessage = _personalizedMessageTextBox.Text;
        }
    }
}