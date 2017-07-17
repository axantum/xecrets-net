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

using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Common;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Crypto.Asymmetric;
using Axantum.AxCrypt.Core.Service;
using Axantum.AxCrypt.Core.Session;
using System;
using System.Linq;
using System.Threading.Tasks;
using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.UI.ViewModel
{
    public class IdentityViewModel : ViewModelBase
    {
        private FileSystemState _fileSystemState;

        private KnownIdentities _knownIdentities;

        private UserSettings _userSettings;

        private SessionNotify _sessionNotify;

        public IdentityViewModel(FileSystemState fileSystemState, KnownIdentities knownIdentities, UserSettings userSettings, SessionNotify sessionNotify)
        {
            _fileSystemState = fileSystemState;
            _knownIdentities = knownIdentities;
            _userSettings = userSettings;
            _sessionNotify = sessionNotify;

            LogOnIdentity = LogOnIdentity.Empty;

            LogOnAsync = new AsyncDelegateAction<object>(async (o) => { if (!_knownIdentities.IsLoggedOn) { LogOnIdentity = await LogOnActionAsync(); } });
            LogOff = new AsyncDelegateAction<object>(async (p) => { await LogOffAction(); LogOnIdentity = null; }, (o) => Task.FromResult(_knownIdentities.IsLoggedOn));
            LogOnLogOff = new AsyncDelegateAction<object>(async (o) => LogOnIdentity = await LogOnLogOffActionAsync());
            AskForDecryptPassphrase = new AsyncDelegateAction<string>(async (name) => LogOnIdentity = await AskForDecryptPassphraseActionAsync(name));
            AskForLogOnPassphrase = new AsyncDelegateAction<LogOnIdentity>(async (id) => LogOnIdentity = await AskForLogOnPassphraseActionAsync(id, String.Empty));

            _sessionNotify.AddCommand(HandleLogOnLogOffNotifications);
        }

        private Task HandleLogOnLogOffNotifications(SessionNotification notification)
        {
            switch (notification.NotificationType)
            {
                case SessionNotificationType.LogOn:
                case SessionNotificationType.LogOff:
                    LogOnAsync.RaiseCanExecuteChanged();
                    LogOff.RaiseCanExecuteChanged();
                    break;
            }
            return Constant.CompletedTask;
        }

        public LogOnIdentity LogOnIdentity { get { return GetProperty<LogOnIdentity>(nameof(LogOnIdentity)); } set { SetProperty(nameof(LogOnIdentity), value); } }

        public AsyncDelegateAction<object> LogOnAsync { get; private set; }

        public AsyncDelegateAction<object> LogOff { get; private set; }

        public IAsyncAction LogOnLogOff { get; private set; }

        public IAsyncAction AskForDecryptPassphrase { get; private set; }

        public IAsyncAction AskForLogOnPassphrase { get; private set; }

        public Func<LogOnEventArgs, Task> LoggingOnAsync { get; set; }

        protected virtual async Task OnLoggingOnAsync(LogOnEventArgs e)
        {
            AlwaysStartSigningInWithOnlineStateRechecked();
            await (LoggingOnAsync?.Invoke(e) ?? Constant.CompletedTask);
        }

        private static void AlwaysStartSigningInWithOnlineStateRechecked()
        {
            New<AxCryptOnlineState>().IsOnline = New<IInternetState>().Clear().Connected;
        }

        private async Task<LogOnIdentity> LogOnActionAsync()
        {
            if (_knownIdentities.IsLoggedOn)
            {
                return _knownIdentities.DefaultEncryptionIdentity;
            }

            LogOnIdentity logOnIdentity;
            logOnIdentity = await AskForLogOnPassphraseActionAsync(LogOnIdentity.Empty, String.Empty);
            if (logOnIdentity == LogOnIdentity.Empty)
            {
                return LogOnIdentity.Empty;
            }
            foreach (UserPublicKey userPublicKey in logOnIdentity.PublicKeys)
            {
                using (KnownPublicKeys knownPublicKeys = New<KnownPublicKeys>())
                {
                    knownPublicKeys.AddOrReplace(userPublicKey);
                }
            }
            return logOnIdentity;
        }

        private async Task LogOffAction()
        {
            if (!_knownIdentities.IsLoggedOn)
            {
                return;
            }
            await _knownIdentities.SetDefaultEncryptionIdentity(LogOnIdentity.Empty);
        }

        private async Task<LogOnIdentity> LogOnLogOffActionAsync()
        {
            if (!_knownIdentities.IsLoggedOn)
            {
                return await LogOnActionAsync();
            }
            await LogOffAction();
            return LogOnIdentity.Empty;
        }

        private async Task<LogOnIdentity> LogOnIdentityFromCredentialsAsync(EmailAddress emailAddress, Passphrase passphrase)
        {
            if (emailAddress != EmailAddress.Empty)
            {
                return await LogOnIdentityFromUserAsync(emailAddress, passphrase);
            }

            return LogOnIdentityFromPassphrase(passphrase);
        }

        private async static Task<LogOnIdentity> LogOnIdentityFromUserAsync(EmailAddress emailAddress, Passphrase passphrase)
        {
            AccountStorage store = new AccountStorage(New<LogOnIdentity, IAccountService>(new LogOnIdentity(emailAddress, passphrase)));
            if (await store.IsIdentityValidAsync())
            {
                return new LogOnIdentity(await store.ActiveKeyPairAsync(), passphrase);
            }
            return LogOnIdentity.Empty;
        }

        private LogOnIdentity LogOnIdentityFromPassphrase(Passphrase passphrase)
        {
            foreach (Passphrase candidate in _fileSystemState.KnownPassphrases)
            {
                if (candidate.Thumbprint == passphrase.Thumbprint)
                {
                    return new LogOnIdentity(passphrase);
                }
            }
            return LogOnIdentity.Empty;
        }

        private async Task<LogOnIdentity> AskForDecryptPassphraseActionAsync(string encryptedFileFullName)
        {
            LogOnEventArgs logOnArgs = new LogOnEventArgs()
            {
                DisplayPassphrase = _userSettings.DisplayEncryptPassphrase,
                Identity = LogOnIdentity.Empty,
                EncryptedFileFullName = encryptedFileFullName,
            };
            await OnLoggingOnAsync(logOnArgs);

            LogOnIdentity identy = await AddKnownIdentityFromEventAsync(logOnArgs);
            if (!_knownIdentities.IsLoggedOn && identy.UserEmail != EmailAddress.Empty)
            {
                await _knownIdentities.SetDefaultEncryptionIdentity(identy);
            }
            return identy;
        }

        private async Task<LogOnIdentity> AskForLogOnPassphraseActionAsync(LogOnIdentity identity, string encryptedFileFullName)
        {
            LogOnIdentity logOnIdentity = await AskForLogOnAsync(identity, encryptedFileFullName);
            if (logOnIdentity == LogOnIdentity.Empty)
            {
                return LogOnIdentity.Empty;
            }

            await _knownIdentities.SetDefaultEncryptionIdentity(logOnIdentity);
            return logOnIdentity;
        }

        private async Task<LogOnIdentity> AskForLogOnAsync(LogOnIdentity identity, string encryptedFileFullName)
        {
            LogOnEventArgs logOnArgs = new LogOnEventArgs()
            {
                DisplayPassphrase = _userSettings.DisplayEncryptPassphrase,
                Identity = identity,
                EncryptedFileFullName = encryptedFileFullName,
            };
            await OnLoggingOnAsync(logOnArgs);

            while (logOnArgs.IsAskingForPreviouslyUnknownPassphrase)
            {
                LogOnIdentity newIdentity = await AskForNewEncryptionPassphraseAsync(logOnArgs.Passphrase, encryptedFileFullName);
                if (newIdentity != LogOnIdentity.Empty)
                {
                    return newIdentity;
                }
                logOnArgs.IsAskingForPreviouslyUnknownPassphrase = false;
                await OnLoggingOnAsync(logOnArgs);
            }

            if (logOnArgs.Cancel || logOnArgs.Passphrase == Passphrase.Empty)
            {
                return LogOnIdentity.Empty;
            }

            _userSettings.DisplayEncryptPassphrase = logOnArgs.DisplayPassphrase;

            return await LogOnIdentityFromCredentialsAsync(EmailAddress.Parse(logOnArgs.UserEmail), logOnArgs.Passphrase);
        }

        private async Task<LogOnIdentity> AskForNewEncryptionPassphraseAsync(Passphrase defaultPassphrase, string encryptedFileFullName)
        {
            LogOnEventArgs logOnArgs = new LogOnEventArgs()
            {
                IsAskingForPreviouslyUnknownPassphrase = true,
                DisplayPassphrase = _userSettings.DisplayEncryptPassphrase,
                Passphrase = defaultPassphrase,
                EncryptedFileFullName = encryptedFileFullName,
            };
            await OnLoggingOnAsync(logOnArgs);

            return await AddKnownIdentityFromEventAsync(logOnArgs);
        }

        private async Task<LogOnIdentity> AddKnownIdentityFromEventAsync(LogOnEventArgs logOnArgs)
        {
            if (logOnArgs.Cancel || logOnArgs.Passphrase == Passphrase.Empty)
            {
                return LogOnIdentity.Empty;
            }

            _userSettings.DisplayEncryptPassphrase = logOnArgs.DisplayPassphrase;

            Passphrase passphrase = logOnArgs.Passphrase;
            LogOnIdentity identity = await LogOnIdentityFromCredentialsAsync(EmailAddress.Parse(logOnArgs.UserEmail), passphrase);
            if (identity == LogOnIdentity.Empty)
            {
                identity = new LogOnIdentity(passphrase);
                _fileSystemState.KnownPassphrases.Add(passphrase);
                await _fileSystemState.Save();
            }
            await _knownIdentities.Add(identity);
            return identity;
        }
    }
}