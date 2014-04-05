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

using System;
using System.Linq;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Session;

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

            Passphrase = null;

            LogOnLogOff = new DelegateAction<Guid>((cryptoId) => Passphrase = LogOnLogOffAction(cryptoId));
            AskForLogOnOrDecryptPassphrase = new DelegateAction<string>((name) => Passphrase = AskForLogOnOrDecryptPassphraseAction(name));
            AskForLogOnPassphrase = new DelegateAction<PassphraseIdentity>((id) => Passphrase = AskForLogOnPassphraseAction(id, String.Empty));
            CryptoId = Instance.CryptoFactory.Default.Id;
        }

        public IPassphrase Passphrase { get { return GetProperty<IPassphrase>("Passphrase"); } set { SetProperty("Passphrase", value); } }

        public Guid CryptoId { get { return GetProperty<Guid>("CryptoId"); } set { SetProperty("CryptoId", value); } }

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

        private IPassphrase LogOnLogOffAction(Guid cryptoId)
        {
            if (_knownKeys.IsLoggedOn)
            {
                _knownKeys.Clear();
                return null;
            }

            CryptoId = cryptoId != Guid.Empty ? cryptoId : Instance.CryptoFactory.Default.Id;

            IPassphrase passphrase;
            if (_fileSystemState.Identities.Any())
            {
                passphrase = AskForLogOnPassphraseAction(PassphraseIdentity.Empty, String.Empty);
                return passphrase;
            }

            passphrase = AskForNewEncryptionPassphrase(String.Empty, String.Empty);
            if (passphrase == null)
            {
                return null;
            }

            _knownKeys.DefaultEncryptionKey = passphrase;
            return _knownKeys.DefaultEncryptionKey;
        }

        private IPassphrase KeyFromPassphrase(string passphrase)
        {
            foreach (PassphraseIdentity identity in _fileSystemState.Identities)
            {
                IPassphrase candidate = Instance.CryptoFactory.Default.CreatePassphrase(passphrase);
                if (identity.Thumbprint == candidate.Thumbprint)
                {
                    return candidate;
                }
            }
            return null;
        }

        private IPassphrase AskForLogOnOrDecryptPassphraseAction(string encryptedFileFullName)
        {
            ActiveFile openFile = _fileSystemState.FindActiveFileFromEncryptedPath(encryptedFileFullName);
            if (openFile == null || openFile.Thumbprint == null)
            {
                return AskForLogOnPassphraseAction(PassphraseIdentity.Empty, encryptedFileFullName);
            }

            PassphraseIdentity identity = _fileSystemState.Identities.FirstOrDefault(i => i.Thumbprint == openFile.Thumbprint);
            if (identity == null)
            {
                return AskForLogOnPassphraseAction(PassphraseIdentity.Empty, encryptedFileFullName);
            }

            return AskForLogOnPassphraseAction(identity, encryptedFileFullName);
        }

        private IPassphrase AskForLogOnPassphraseAction(PassphraseIdentity identity, string encryptedFileFullName)
        {
            IPassphrase passphrase = AskForLogOnOrEncryptionPassphrase(identity, encryptedFileFullName);
            if (passphrase == null)
            {
                return null;
            }

            _knownKeys.DefaultEncryptionKey = passphrase;
            return passphrase;
        }

        private IPassphrase AskForLogOnOrEncryptionPassphrase(PassphraseIdentity identity, string encryptedFileFullName)
        {
            if (!_fileSystemState.Identities.Any())
            {
                return AskForNewEncryptionPassphrase(String.Empty, encryptedFileFullName);
            }

            LogOnEventArgs logOnArgs = new LogOnEventArgs()
            {
                DisplayPassphrase = _userSettings.DisplayEncryptPassphrase,
                Identity = identity,
                EncryptedFileFullName = encryptedFileFullName,
            };
            OnLoggingOn(logOnArgs);

            if (logOnArgs.CreateNew)
            {
                return AskForNewEncryptionPassphrase(logOnArgs.Passphrase, encryptedFileFullName);
            }

            if (logOnArgs.Cancel || logOnArgs.Passphrase.Length == 0)
            {
                return null;
            }

            _userSettings.DisplayEncryptPassphrase = logOnArgs.DisplayPassphrase;

            return KeyFromPassphrase(logOnArgs.Passphrase);
        }

        private IPassphrase AskForNewEncryptionPassphrase(string defaultPassphrase, string encryptedFileFullName)
        {
            LogOnEventArgs logOnArgs = new LogOnEventArgs()
            {
                CreateNew = true,
                DisplayPassphrase = _userSettings.DisplayEncryptPassphrase,
                Passphrase = defaultPassphrase,
                EncryptedFileFullName = encryptedFileFullName,
            };
            OnLoggingOn(logOnArgs);

            if (logOnArgs.Cancel || logOnArgs.Passphrase.Length == 0)
            {
                return null;
            }

            _userSettings.DisplayEncryptPassphrase = logOnArgs.DisplayPassphrase;

            IPassphrase passphrase = Instance.CryptoFactory.Create(CryptoId).CreatePassphrase(logOnArgs.Passphrase);
            PassphraseIdentity identity = _fileSystemState.Identities.FirstOrDefault(i => i.Thumbprint == passphrase.Thumbprint);
            if (identity != null)
            {
                return passphrase;
            }

            identity = new PassphraseIdentity(logOnArgs.Name, passphrase);
            _fileSystemState.Identities.Add(identity);
            _fileSystemState.Save();

            return passphrase;
        }
    }
}