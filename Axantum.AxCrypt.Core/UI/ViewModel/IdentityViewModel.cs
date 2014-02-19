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

using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Session;
using System;
using System.Linq;

namespace Axantum.AxCrypt.Core.UI.ViewModel
{
    public class IdentityViewModel : ViewModelBase
    {
        private FileSystemState _fileSystemState;

        private KnownKeys _knownKeys;

        private IUserSettings _userSettings;

        public IdentityViewModel(FileSystemState fileSystemState, KnownKeys knownKeys, IUserSettings userSettings)
        {
            _fileSystemState = fileSystemState;
            _knownKeys = knownKeys;
            _userSettings = userSettings;

            PassphraseText = String.Empty;

            LogOnLogOff = new DelegateAction<object>((parameter) => PassphraseText = LogOnLogOffAction());
            AskForLogOnOrDecryptPassphrase = new DelegateAction<string>((name) => PassphraseText = AskForLogOnOrDecryptPassphraseAction(name));
            AskForLogOnPassphrase = new DelegateAction<PassphraseIdentity>((id) => PassphraseText = AskForLogOnPassphraseAction(id));
        }

        public string PassphraseText { get { return GetProperty<string>("PassphraseText"); } set { SetProperty("PassphraseText", value); } }

        public IAction LogOnLogOff { get; private set; }

        public IAction AskForLogOnOrDecryptPassphrase { get; private set; }

        public IAction AskForLogOnPassphrase { get; private set; }

        public event EventHandler<LogOnEventArgs> LoggingOn;

        protected virtual void OnLoggingOn(LogOnEventArgs e)
        {
            EventHandler<LogOnEventArgs> handler = LoggingOn;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private string LogOnLogOffAction()
        {
            if (_knownKeys.IsLoggedOn)
            {
                _knownKeys.Clear();
                return String.Empty;
            }

            if (_fileSystemState.Identities.Any())
            {
                return AskForLogOnPassphraseAction(PassphraseIdentity.Empty);
            }

            string passphrase = AskForNewEncryptionPassphrase(String.Empty);
            if (String.IsNullOrEmpty(passphrase))
            {
                return String.Empty;
            }

            _knownKeys.DefaultEncryptionKey = new V1Passphrase(passphrase).DerivedKey;
            return passphrase;
        }

        private string AskForLogOnOrDecryptPassphraseAction(string encryptedFileFullName)
        {
            ActiveFile openFile = _fileSystemState.FindEncryptedPath(encryptedFileFullName);
            if (openFile == null || openFile.Thumbprint == null)
            {
                return AskForLogOnPassphraseAction(PassphraseIdentity.Empty);
            }

            PassphraseIdentity identity = _fileSystemState.Identities.FirstOrDefault(i => i.Thumbprint == openFile.Thumbprint);
            if (identity == null)
            {
                return AskForLogOnPassphraseAction(PassphraseIdentity.Empty);
            }

            return AskForLogOnPassphraseAction(identity);
        }

        private string AskForLogOnPassphraseAction(PassphraseIdentity identity)
        {
            string passphrase = AskForLogOnOrEncryptionPassphrase(identity);
            if (passphrase.Length == 0)
            {
                return String.Empty;
            }

            _knownKeys.DefaultEncryptionKey = new V1Passphrase(passphrase).DerivedKey;
            return passphrase;
        }

        private string AskForLogOnOrEncryptionPassphrase(PassphraseIdentity identity)
        {
            LogOnEventArgs logOnArgs = new LogOnEventArgs()
            {
                DisplayPassphrase = _userSettings.DisplayEncryptPassphrase,
                Identity = identity,
            };
            OnLoggingOn(logOnArgs);

            if (logOnArgs.CreateNew)
            {
                return AskForNewEncryptionPassphrase(logOnArgs.Passphrase);
            }

            if (logOnArgs.Cancel || logOnArgs.Passphrase.Length == 0)
            {
                return String.Empty;
            }

            _userSettings.DisplayEncryptPassphrase = logOnArgs.DisplayPassphrase;

            return logOnArgs.Passphrase;
        }

        private string AskForNewEncryptionPassphrase(string defaultPassphrase)
        {
            LogOnEventArgs logOnArgs = new LogOnEventArgs()
            {
                CreateNew = true,
                DisplayPassphrase = _userSettings.DisplayEncryptPassphrase,
                Passphrase = defaultPassphrase
            };
            OnLoggingOn(logOnArgs);

            if (logOnArgs.Cancel || logOnArgs.Passphrase.Length == 0)
            {
                return String.Empty;
            }

            _userSettings.DisplayEncryptPassphrase = logOnArgs.DisplayPassphrase;

            V1Passphrase passphrase = new V1Passphrase(logOnArgs.Passphrase);
            PassphraseIdentity identity = _fileSystemState.Identities.FirstOrDefault(i => i.Thumbprint == passphrase.DerivedKey.Thumbprint);
            if (identity != null)
            {
                return logOnArgs.Passphrase;
            }

            identity = new PassphraseIdentity(logOnArgs.Name, passphrase.DerivedKey);
            _fileSystemState.Identities.Add(identity);
            _fileSystemState.Save();

            return logOnArgs.Passphrase;
        }
    }
}