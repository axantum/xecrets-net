#region Coypright and License

/*
 * AxCrypt - Copyright 2013, Svante Seleborg, All Rights Reserved
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
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Axantum.AxCrypt.Core.UI.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            LogonEnabled = true;
        }

        public bool LogonEnabled { get { return GetProperty<bool>("LogonEnabled"); } set { SetProperty("LogonEnabled", value); } }

        public bool EncryptFileEnabled { get { return GetProperty<bool>("EncryptFileEnabled"); } set { SetProperty("EncryptFileEnabled", value); } }

        public bool DecryptFileEnabled { get { return GetProperty<bool>("DecryptFileEnabled"); } set { SetProperty("DecryptFileEnabled", value); } }

        public bool OpenEncryptedEnabled { get { return GetProperty<bool>("OpenEncryptedEnabled"); } set { SetProperty("OpenEncryptedEnabled", value); } }

        public void LogOnLogOff()
        {
            LogOnLogOffInternal();

            LogonEnabled = !Instance.KnownKeys.IsLoggedOn;
            EncryptFileEnabled = Instance.KnownKeys.IsLoggedOn;
            DecryptFileEnabled = Instance.KnownKeys.IsLoggedOn;
            OpenEncryptedEnabled = Instance.KnownKeys.IsLoggedOn;
        }

        private void LogOnLogOffInternal()
        {
            if (Instance.KnownKeys.IsLoggedOn)
            {
                Instance.KnownKeys.Clear();
                return;
            }

            if (Instance.FileSystemState.Identities.Any(identity => true))
            {
                TryLogOnToExistingIdentity();
                return;
            }

            string passphrase = AskForNewEncryptionPassphrase(String.Empty);
            if (String.IsNullOrEmpty(passphrase))
            {
                return;
            }

            Instance.KnownKeys.DefaultEncryptionKey = Passphrase.Derive(passphrase);
        }

        private void TryLogOnToExistingIdentity()
        {
            string passphrase = AskForLogOnPassphrase(PassphraseIdentity.Empty);
            if (String.IsNullOrEmpty(passphrase))
            {
                return;
            }
        }

        public event EventHandler<LogOnEventArgs> LoggingOn;

        protected virtual void OnLogggingOn(LogOnEventArgs e)
        {
            EventHandler<LogOnEventArgs> handler = LoggingOn;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private string AskForLogOnPassphrase(PassphraseIdentity identity)
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
            LogOnEventArgs logOnArgs = new LogOnEventArgs()
            {
                DisplayPassphrase = Instance.FileSystemState.Settings.DisplayEncryptPassphrase,
            };
            OnLogggingOn(logOnArgs);

            if (logOnArgs.CreateNew)
            {
                return AskForNewEncryptionPassphrase(logOnArgs.Passphrase);
            }

            if (logOnArgs.Cancel || logOnArgs.Passphrase.Length == 0)
            {
                return String.Empty;
            }

            if (logOnArgs.DisplayPassphrase != Instance.FileSystemState.Settings.DisplayEncryptPassphrase)
            {
                Instance.FileSystemState.Settings.DisplayEncryptPassphrase = logOnArgs.DisplayPassphrase;
                Instance.FileSystemState.Save();
            }

            return logOnArgs.Passphrase;
        }

        private string AskForNewEncryptionPassphrase(string defaultPassphrase)
        {
            LogOnEventArgs logOnArgs = new LogOnEventArgs()
            {
                CreateNew = true,
                DisplayPassphrase = Instance.FileSystemState.Settings.DisplayEncryptPassphrase,
                Passphrase = defaultPassphrase
            };
            OnLogggingOn(logOnArgs);

            if (logOnArgs.Cancel || logOnArgs.Passphrase.Length == 0)
            {
                return String.Empty;
            }
            
            if (logOnArgs.DisplayPassphrase != Instance.FileSystemState.Settings.DisplayEncryptPassphrase)
            {
                Instance.FileSystemState.Settings.DisplayEncryptPassphrase = logOnArgs.DisplayPassphrase;
                Instance.FileSystemState.Save();
            }

            Passphrase passphrase = new Passphrase(logOnArgs.Passphrase);
            PassphraseIdentity identity = Instance.FileSystemState.Identities.FirstOrDefault(i => i.Thumbprint == passphrase.DerivedPassphrase.Thumbprint);
            if (identity != null)
            {
                return logOnArgs.Passphrase;
            }

            identity = new PassphraseIdentity(logOnArgs.Passphrase, passphrase.DerivedPassphrase);
            Instance.FileSystemState.Identities.Add(identity);
            Instance.FileSystemState.Save();

            return logOnArgs.Passphrase;
        }
    }
}