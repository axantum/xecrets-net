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
 * The source is maintained at http://bitbucket.org/AxCrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using AxCrypt.Core.Crypto;
using AxCrypt.Core.Extensions;
using AxCrypt.Core.IO;
using AxCrypt.Core.Service;
using AxCrypt.Core.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Core.UI.ViewModel
{
    public class ImportPrivateKeysViewModel : ViewModelBase, IPasswordEntry
    {
        private UserSettings _userSettings;

        private KnownIdentities _knownIdentities;

        public bool ShowPassword { get { return GetProperty<bool>(nameof(ShowPassword)); } set { SetProperty(nameof(ShowPassword), value); } }

        public string PasswordText { get { return GetProperty<string>(nameof(PasswordText)); } set { SetProperty(nameof(PasswordText), value); } }

        public string PrivateKeyFileName { get { return GetProperty<string>(nameof(PrivateKeyFileName)); } set { SetProperty(nameof(PrivateKeyFileName), value); } }

        public bool ImportSuccessful { get { return GetProperty<bool>(nameof(ImportSuccessful)); } set { SetProperty(nameof(ImportSuccessful), value); } }

        public ImportPrivateKeysViewModel(UserSettings userSettings, KnownIdentities knownIdentities)
        {
            _userSettings = userSettings;
            _knownIdentities = knownIdentities;

            InitializePropertyValues();
            BindPropertyChangedEvents();
            SubscribeToModelEvents();
        }

        public IAsyncAction ImportFile { get; private set; }

        private void InitializePropertyValues()
        {
            ImportFile = new AsyncDelegateAction<object>((o) => ImportFileActionAsync());
            ImportSuccessful = true;
            ShowPassword = _userSettings.DisplayDecryptPassphrase;
        }

        private void BindPropertyChangedEvents()
        {
            BindPropertyChangedInternal(nameof(ShowPassword), (bool show) => _userSettings.DisplayDecryptPassphrase = show);
        }

        private static void SubscribeToModelEvents()
        {
        }

        protected override Task<bool> ValidateAsync(string columnName)
        {
            return Task.FromResult(ValidateInternal(columnName));
        }

        private bool ValidateInternal(string columnName)
        {
            IDataStore privateKeyDataStore;
            switch (columnName)
            {
                case nameof(PrivateKeyFileName):
                    if (String.IsNullOrEmpty(PrivateKeyFileName))
                    {
                        return false;
                    }
                    privateKeyDataStore = New<IDataStore>(PrivateKeyFileName);
                    return privateKeyDataStore.IsAvailable;

                case nameof(PasswordText):
                    if (!ValidateInternal(nameof(PrivateKeyFileName)))
                    {
                        return false;
                    }
                    privateKeyDataStore = New<IDataStore>(PrivateKeyFileName);

                    if (String.IsNullOrEmpty(PasswordText))
                    {
                        return false;
                    }
                    LogOnIdentity identity = new LogOnIdentity(PasswordText);

                    EncryptedProperties properties = EncryptedProperties.Create(privateKeyDataStore, identity);
                    return properties.IsValid;

                default:
                    throw new ArgumentException("Cannot validate property.", columnName);
            }
        }

        private async Task ImportFileActionAsync()
        {
            IDataStore privateKeyData = New<IDataStore>(PrivateKeyFileName);
            Passphrase passphrase = new Passphrase(PasswordText);
            UserKeyPair keyPair;
            if (!UserKeyPair.TryLoad(privateKeyData.ToArray(), passphrase, out keyPair))
            {
                ImportSuccessful = false;
                return;
            }

            LogOnIdentity identity = new LogOnIdentity(keyPair.UserEmail, passphrase);
            AccountStorage store = new AccountStorage(New<LogOnIdentity, IAccountService>(identity));
            await store.ImportAsync(keyPair);
            ImportSuccessful = true;

            _userSettings.UserEmail = keyPair.UserEmail.Address;
            await _knownIdentities.SetDefaultEncryptionIdentity(new LogOnIdentity(await store.AllKeyPairsAsync(), passphrase));
        }
    }
}