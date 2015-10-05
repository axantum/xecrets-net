#region Coypright and License

/*
 * AxCrypt - Copyright 2015, Svante Seleborg, All Rights Reserved
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

using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Session;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.UI.ViewModel
{
    public class ImportPrivateKeysViewModel : ViewModelBase
    {
        private UserAsymmetricKeysStore _keysStore;

        private IUserSettings _userSettings;

        private KnownIdentities _knownIdentities;

        public bool ShowPassphrase { get { return GetProperty<bool>("ShowPassphrase"); } set { SetProperty("ShowPassphrase", value); } }

        public string Passphrase { get { return GetProperty<string>("Passphrase"); } set { SetProperty("Passphrase", value); } }

        public string PrivateKeyFileName { get { return GetProperty<string>("PrivateKeyFileName"); } set { SetProperty("PrivateKeyFileName", value); } }

        public bool ImportSuccessful { get { return GetProperty<bool>("ImportSuccessful"); } set { SetProperty("ImportSuccessful", value); } }

        public ImportPrivateKeysViewModel(IUserSettings userSettings, KnownIdentities knownIdentities)
        {
            _keysStore = new UserAsymmetricKeysStore(Resolve.WorkFolder.FileInfo, knownIdentities.DefaultEncryptionIdentity.UserEmail, knownIdentities.DefaultEncryptionIdentity.Passphrase);
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
            BindPropertyChangedInternal("ShowPassphrase", (bool show) => _userSettings.DisplayDecryptPassphrase = show);
        }

        private static void SubscribeToModelEvents()
        {
        }

        public override string this[string columnName]
        {
            get
            {
                string error = base[columnName];
                if (String.IsNullOrEmpty(error))
                {
                    error = Validate(columnName);
                }

                return error;
            }
        }

        private string Validate(string columnName)
        {
            if (ValidateInternal(columnName))
            {
                return String.Empty;
            }
            return ValidationError.ToString(CultureInfo.InvariantCulture);
        }

        private bool ValidateInternal(string columnName)
        {
            IDataStore privateKeyDataStore;
            switch (columnName)
            {
                case "PrivateKeyFileName":
                    if (String.IsNullOrEmpty(PrivateKeyFileName))
                    {
                        return false;
                    }
                    privateKeyDataStore = TypeMap.Resolve.New<IDataStore>(PrivateKeyFileName);
                    return privateKeyDataStore.IsAvailable;

                case "Passphrase":
                    if (!ValidateInternal("PrivateKeyFileName"))
                    {
                        return false;
                    }
                    privateKeyDataStore = TypeMap.Resolve.New<IDataStore>(PrivateKeyFileName);

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
            IDataStore privateKeyData = TypeMap.Resolve.New<IDataStore>(PrivateKeyFileName);
            Passphrase passphrase = new Passphrase(Passphrase);
            UserKeyPair keyPair;
            if (!UserKeyPair.TryLoad(privateKeyData.ToArray(), passphrase, out keyPair))
            {
                ImportSuccessful = false;
                return;
            }

            _keysStore.Import(keyPair, passphrase);
            ImportSuccessful = true;

            _userSettings.UserEmail = keyPair.UserEmail.Address;
            _knownIdentities.DefaultEncryptionIdentity = new LogOnIdentity(_keysStore.UserKeyPair, passphrase);
        }
    }
}