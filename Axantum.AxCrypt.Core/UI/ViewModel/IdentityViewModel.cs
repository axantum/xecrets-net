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

            Passphrase = null;

            LogOnLogOff = new DelegateAction<Guid>((cryptoId) => Passphrase = LogOnLogOffAction(cryptoId));
            AskForDecryptPassphrase = new DelegateAction<string>((name) => Passphrase = AskForDecryptPassphraseAction(name));
            AskForLogOnPassphrase = new DelegateAction<PassphraseIdentity>((id) => Passphrase = AskForLogOnPassphraseAction(id, String.Empty));
            CryptoId = Instance.CryptoFactory.Default.Id;
        }

        public Passphrase Passphrase { get { return GetProperty<Passphrase>("Passphrase"); } set { SetProperty("Passphrase", value); } }

        public Guid CryptoId { get { return GetProperty<Guid>("CryptoId"); } set { SetProperty("CryptoId", value); } }

        public IAction LogOnLogOff { get; private set; }

        public IAction AskForDecryptPassphrase { get; private set; }

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

        private Passphrase LogOnLogOffAction(Guid cryptoId)
        {
            if (_knownKeys.IsLoggedOn)
            {
                _knownKeys.Clear();
                return null;
            }

            CryptoId = cryptoId != Guid.Empty ? cryptoId : Instance.CryptoFactory.Default.Id;

            Passphrase passphrase;
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

        private Passphrase KeyFromPassphrase(string passphrase)
        {
            foreach (PassphraseIdentity identity in _fileSystemState.Identities)
            {
                Passphrase candidate = new Passphrase(passphrase);
                if (identity.Thumbprint == candidate.Thumbprint)
                {
                    return candidate;
                }
            }
            return null;
        }

        private Passphrase AskForDecryptPassphraseAction(string encryptedFileFullName)
        {
            LogOnEventArgs logOnArgs = new LogOnEventArgs()
            {
                DisplayPassphrase = _userSettings.DisplayEncryptPassphrase,
                Identity = PassphraseIdentity.Empty,
                EncryptedFileFullName = encryptedFileFullName,
            };
            OnLoggingOn(logOnArgs);

            _userSettings.DisplayEncryptPassphrase = logOnArgs.DisplayPassphrase;

            if (logOnArgs.Cancel || logOnArgs.Passphrase.Length == 0)
            {
                return null;
            }

            return new Passphrase(logOnArgs.Passphrase);
        }

        private Passphrase AskForLogOnPassphraseAction(PassphraseIdentity identity, string encryptedFileFullName)
        {
            Passphrase passphrase = AskForLogOnOrEncryptionPassphrase(identity, encryptedFileFullName);
            if (passphrase == null)
            {
                return null;
            }

            _knownKeys.DefaultEncryptionKey = passphrase;
            return passphrase;
        }

        private Passphrase AskForLogOnOrEncryptionPassphrase(PassphraseIdentity identity, string encryptedFileFullName)
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

        private Passphrase AskForNewEncryptionPassphrase(string defaultPassphrase, string encryptedFileFullName)
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

            Passphrase passphrase = new Passphrase(logOnArgs.Passphrase);
            PassphraseIdentity identity = _fileSystemState.Identities.FirstOrDefault(i => i.Thumbprint == passphrase.Thumbprint);
            if (identity != null)
            {
                return passphrase;
            }

            identity = new PassphraseIdentity(passphrase);
            _fileSystemState.Identities.Add(identity);
            _fileSystemState.Save();

            return passphrase;
        }
    }
}