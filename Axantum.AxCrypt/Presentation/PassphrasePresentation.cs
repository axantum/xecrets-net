using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Axantum.AxCrypt.Presentation
{
    public class PassphrasePresentation
    {
        private IMainView _mainView;

        public PassphrasePresentation(IMainView mainView)
        {
            _mainView = mainView;
        }

        public string AskForDecryptPassphrase()
        {
            using (DecryptPassphraseDialog passphraseDialog = new DecryptPassphraseDialog())
            {
                passphraseDialog.ShowPassphraseCheckBox.Checked = Settings.Default.ShowDecryptPassphrase;
                DialogResult dialogResult = passphraseDialog.ShowDialog(_mainView.Control);
                if (passphraseDialog.ShowPassphraseCheckBox.Checked != Settings.Default.ShowDecryptPassphrase)
                {
                    Settings.Default.ShowDecryptPassphrase = passphraseDialog.ShowPassphraseCheckBox.Checked;
                    Settings.Default.Save();
                }
                if (dialogResult != DialogResult.OK)
                {
                    return null;
                }
                return passphraseDialog.Passphrase.Text;
            }
        }

        public string AskForLogOnPassphrase(PassphraseIdentity identity)
        {
            string passphrase = AskForLogOnOrEncryptionPassphrase(identity);
            if (passphrase.Length == 0)
            {
                return String.Empty;
            }

            Instance.KnownKeys.DefaultEncryptionKey = Passphrase.Derive(passphrase);
            return passphrase;
        }

        private string AskForLogOnOrEncryptionPassphrase(PassphraseIdentity identity)
        {
            using (LogOnDialog logOnDialog = new LogOnDialog(Instance.FileSystemState, identity))
            {
                logOnDialog.ShowPassphraseCheckBox.Checked = Settings.Default.ShowEncryptPasshrase;
                DialogResult dialogResult = logOnDialog.ShowDialog();
                if (dialogResult == DialogResult.Retry)
                {
                    return AskForNewEncryptionPassphrase();
                }

                if (dialogResult != DialogResult.OK || logOnDialog.PassphraseTextBox.Text.Length == 0)
                {
                    return String.Empty;
                }

                if (logOnDialog.ShowPassphraseCheckBox.Checked != Settings.Default.ShowEncryptPasshrase)
                {
                    Settings.Default.ShowEncryptPasshrase = logOnDialog.ShowPassphraseCheckBox.Checked;
                    Settings.Default.Save();
                }
                return logOnDialog.PassphraseTextBox.Text;
            }
        }

        public string AskForNewEncryptionPassphrase()
        {
            using (EncryptPassphraseDialog passphraseDialog = new EncryptPassphraseDialog(Instance.FileSystemState))
            {
                passphraseDialog.ShowPassphraseCheckBox.Checked = Settings.Default.ShowEncryptPasshrase;
                DialogResult dialogResult = passphraseDialog.ShowDialog();
                if (dialogResult != DialogResult.OK || passphraseDialog.PassphraseTextBox.Text.Length == 0)
                {
                    return String.Empty;
                }

                if (passphraseDialog.ShowPassphraseCheckBox.Checked != Settings.Default.ShowEncryptPasshrase)
                {
                    Settings.Default.ShowEncryptPasshrase = passphraseDialog.ShowPassphraseCheckBox.Checked;
                    Settings.Default.Save();
                }

                Passphrase passphrase = new Passphrase(passphraseDialog.PassphraseTextBox.Text);
                PassphraseIdentity identity = Instance.FileSystemState.Identities.FirstOrDefault(i => i.Thumbprint == passphrase.DerivedPassphrase.Thumbprint);
                if (identity != null)
                {
                    return passphraseDialog.PassphraseTextBox.Text;
                }

                identity = new PassphraseIdentity(passphraseDialog.NameTextBox.Text, passphrase.DerivedPassphrase);
                Instance.FileSystemState.Identities.Add(identity);
                Instance.FileSystemState.Save();

                return passphraseDialog.PassphraseTextBox.Text;
            }
        }

        public string AskForLogOnOrDecryptPassphrase(string fullName)
        {
            ActiveFile openFile = Instance.FileSystemState.FindEncryptedPath(fullName);
            if (openFile == null || openFile.Thumbprint == null)
            {
                return AskForDecryptPassphrase();
            }

            PassphraseIdentity identity = Instance.FileSystemState.Identities.FirstOrDefault(i => i.Thumbprint == openFile.Thumbprint);
            if (identity == null)
            {
                return AskForDecryptPassphrase();
            }

            return AskForLogOnPassphrase(identity);
        }
    }
}