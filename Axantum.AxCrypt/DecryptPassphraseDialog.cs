#region Coypright and License

/*
 * AxCrypt - Copyright 2014, Svante Seleborg, All Rights Reserved
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
 * http://www.axantum.com for more information about the author.
*/

#endregion Coypright and License

using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Forms.Style;
using Axantum.AxCrypt.Properties;
using AxCrypt.Content;
using System.IO;
using System.Windows.Forms;

namespace Axantum.AxCrypt
{
    public partial class DecryptPassphraseDialog : StyledMessageBase
    {
        public DecryptPassphraseDialog()
        {
            InitializeComponent();
        }

        public DecryptPassphraseDialog(Form parent, string fullName)
            : this()
        {
            InitializeStyle(parent);

            _fileNameLabel.Text = Path.GetFileName(fullName);
        }

        protected override void InitializeContentResources()
        {
            Text = Content.DialogDecryptTitle;

            _buttonOk.Text = Content.ButtonOkText;
            _buttonCancel.Text = Content.ButtonCancelText;
            PassphraseGroupBox.Text = Content.PassphrasePrompt;
            ShowPassphraseCheckBox.Text = Content.ShowPasswordOptionPrompt;
        }

        private void DecryptPassphraseDialog_Load(object sender, System.EventArgs e)
        {
            ShowHidePassphrase();
        }

        private void ShowHidePassphrase()
        {
            Passphrase.UseSystemPasswordChar = !ShowPassphraseCheckBox.Checked;
        }

        private void ShowPassphraseCheckBox_CheckedChanged(object sender, System.EventArgs e)
        {
            ShowHidePassphrase();
        }
    }
}