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

        private SessionNotify _sessionNotify;

        public IdentityViewModel(FileSystemState fileSystemState, KnownKeys knownKeys, IUserSettings userSettings, SessionNotify sessionNotify)
        {
            _fileSystemState = fileSystemState;
            _knownKeys = knownKeys;
            _userSettings = userSettings;
            _sessionNotify = sessionNotify;

            Passphrase = null;

            LogOn = new DelegateAction<Guid>((cryptoId) => Passphrase = LogOnAction(cryptoId), (o) => !_knownKeys.IsLoggedOn);
            LogOff = new DelegateAction<object>((p) => { LogOffAction(); Passphrase = null; }, (o) => _knownKeys.IsLoggedOn);
            LogOnLogOff = new DelegateAction<Guid>((cryptoId) => Passphrase = LogOnLogOffAction(cryptoId));
            AskForDecryptPassphrase = new DelegateAction<string>((name) => Passphrase = AskForDecryptPassphraseAction(name));
            AskForLogOnPassphrase = new DelegateAction<Passphrase>((id) => Passphrase = AskForLogOnPassphraseAction(id, String.Empty));
            CryptoId = Resolve.CryptoFactory.Default.Id;

            _sessionNotify.Notification += HandleLogOnLogOffNotifications;
        }

        private void HandleLogOnLogOffNotifications(object sender, SessionNotificationEventArgs e)
        {
            switch (e.Notification.NotificationType)
            {
                case SessionNotificationType.LogOn:
                case SessionNotificationType.LogOff:
                    LogOn.RaiseCanExecuteChanged();
                    LogOff.RaiseCanExecuteChanged();
                    break;
            }
        }

        public LogOnIdentity Passphrase { get { return GetProperty<LogOnIdentity>("Passphrase"); } set { SetProperty("Passphrase", value); } }

        public Guid CryptoId { get { return GetProperty<Guid>("CryptoId"); } set { SetProperty("CryptoId", value); } }

        public DelegateAction<Guid> LogOn { get; private set; }

        public DelegateAction<object> LogOff { get; private set; }

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

        private LogOnIdentity LogOnAction(Guid cryptoId)
        {
            if (_knownKeys.IsLoggedOn)
            {
                return _knownKeys.DefaultEncryptionKey;
            }

            CryptoId = cryptoId != Guid.Empty ? cryptoId : Resolve.CryptoFactory.Default.Id;

            LogOnIdentity passphrase;
            if (_fileSystemState.Identities.Any())
            {
                passphrase = AskForLogOnPassphraseAction(Axantum.AxCrypt.Core.Crypto.Passphrase.Empty, String.Empty);
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

        private void LogOffAction()
        {
            if (!_knownKeys.IsLoggedOn)
            {
                return;
            }
            _knownKeys.Clear();
        }

        private LogOnIdentity LogOnLogOffAction(Guid cryptoId)
        {
            if (!_knownKeys.IsLoggedOn)
            {
                return LogOnAction(cryptoId);
            }
            LogOffAction();
            return null;
        }

        private Passphrase KeyFromPassphrase(string passphrase)
        {
            foreach (Passphrase identity in _fileSystemState.Identities)
            {
                Passphrase candidate = new Passphrase(passphrase);
                if (identity.Thumbprint == candidate.Thumbprint)
                {
                    return candidate;
                }
            }
            return null;
        }

        private LogOnIdentity AskForDecryptPassphraseAction(string encryptedFileFullName)
        {
            LogOnEventArgs logOnArgs = new LogOnEventArgs()
            {
                DisplayPassphrase = _userSettings.DisplayEncryptPassphrase,
                Identity = Axantum.AxCrypt.Core.Crypto.Passphrase.Empty,
                EncryptedFileFullName = encryptedFileFullName,
            };
            OnLoggingOn(logOnArgs);

            _userSettings.DisplayEncryptPassphrase = logOnArgs.DisplayPassphrase;

            if (logOnArgs.Cancel || logOnArgs.Passphrase.Length == 0)
            {
                return null;
            }

            return new LogOnIdentity(new Passphrase(logOnArgs.Passphrase));
        }

        private LogOnIdentity AskForLogOnPassphraseAction(Passphrase identity, string encryptedFileFullName)
        {
            LogOnIdentity passphrase = AskForLogOnOrEncryptionPassphrase(identity, encryptedFileFullName);
            if (passphrase == null)
            {
                return null;
            }

            _knownKeys.DefaultEncryptionKey = passphrase;
            return passphrase;
        }

        private LogOnIdentity AskForLogOnOrEncryptionPassphrase(Passphrase identity, string encryptedFileFullName)
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

            if (logOnArgs.IsAskingForPreviouslyUnknownPassphrase)
            {
                return AskForNewEncryptionPassphrase(logOnArgs.Passphrase, encryptedFileFullName);
            }

            if (logOnArgs.Cancel || logOnArgs.Passphrase.Length == 0)
            {
                return null;
            }

            _userSettings.DisplayEncryptPassphrase = logOnArgs.DisplayPassphrase;

            return new LogOnIdentity(KeyFromPassphrase(logOnArgs.Passphrase));
        }

        private LogOnIdentity AskForNewEncryptionPassphrase(string defaultPassphrase, string encryptedFileFullName)
        {
            LogOnEventArgs logOnArgs = new LogOnEventArgs()
            {
                IsAskingForPreviouslyUnknownPassphrase = true,
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
            Passphrase identity = _fileSystemState.Identities.FirstOrDefault(i => i.Thumbprint == passphrase.Thumbprint);
            if (identity != null)
            {
                return new LogOnIdentity(passphrase);
            }

            identity = passphrase;
            _fileSystemState.Identities.Add(identity);
            _fileSystemState.Save();

            return new LogOnIdentity(passphrase);
        }
    }
}