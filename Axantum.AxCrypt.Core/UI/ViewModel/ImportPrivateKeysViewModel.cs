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

using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Service;
using Axantum.AxCrypt.Core.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.UI.ViewModel
{
    public class ImportPrivateKeysViewModel : ViewModelBase
    {
        private IUserSettings _userSettings;

        private KnownIdentities _knownIdentities;

        public bool ShowPassphrase { get { return GetProperty<bool>(nameof(ShowPassphrase)); } set { SetProperty(nameof(ShowPassphrase), value); } }

        public string Passphrase { get { return GetProperty<string>(nameof(Passphrase)); } set { SetProperty(nameof(Passphrase), value); } }

        public string PrivateKeyFileName { get { return GetProperty<string>(nameof(PrivateKeyFileName)); } set { SetProperty(nameof(PrivateKeyFileName), value); } }

        public bool ImportSuccessful { get { return GetProperty<bool>(nameof(ImportSuccessful)); } set { SetProperty(nameof(ImportSuccessful), value); } }

        public ImportPrivateKeysViewModel(IUserSettings userSettings, KnownIdentities knownIdentities)
        {
            _userSettings = userSettings;
            _knownIdentities = knownIdentities;

            InitializePropertyValues();
            BindPropertyChangedEvents();
            SubscribeToModelEvents();
        }

        public IAction ImportFile { get; private set; }

        private void InitializePropertyValues()
        {
            ImportFile = new DelegateAction<object>((o) => ImportFileAction());
            ImportSuccessful = true;
            ShowPassphrase = _userSettings.DisplayDecryptPassphrase;
        }

        private void BindPropertyChangedEvents()
        {
            BindPropertyChangedInternal(nameof(ShowPassphrase), (bool show) => _userSettings.DisplayDecryptPassphrase = show);
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

                case nameof(Passphrase):
                    if (!ValidateInternal(nameof(PrivateKeyFileName)))
                    {
                        return false;
                    }
                    privateKeyDataStore = New<IDataStore>(PrivateKeyFileName);

                    if (String.IsNullOrEmpty(Passphrase))
                    {
                        return false;
                    }
                    LogOnIdentity identity = new LogOnIdentity(Passphrase);

                    EncryptedProperties properties = EncryptedProperties.Create(privateKeyDataStore, identity);
                    return properties.IsValid;

                default:
                    throw new ArgumentException("Cannot validate property.", columnName);
            }
        }

        private void ImportFileAction()
        {
            IDataStore privateKeyData = New<IDataStore>(PrivateKeyFileName);
            Passphrase passphrase = new Passphrase(Passphrase);
            UserKeyPair keyPair;
            if (!UserKeyPair.TryLoad(privateKeyData.ToArray(), passphrase, out keyPair))
            {
                ImportSuccessful = false;
                return;
            }

            AccountStorage store = new AccountStorage(New<LogOnIdentity, IAccountService>(_knownIdentities.DefaultEncryptionIdentity));
            store.ImportAsync(keyPair).Wait();
            ImportSuccessful = true;

            _userSettings.UserEmail = keyPair.UserEmail.Address;
            _knownIdentities.DefaultEncryptionIdentity = new LogOnIdentity(store.ActiveKeyPairAsync().Result, passphrase);
        }
    }
}