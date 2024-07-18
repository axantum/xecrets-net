#region Coypright and License

/*
 * AxCrypt - Copyright 2023, All Rights Reserved
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

using AxCrypt.Abstractions;
using AxCrypt.Api;
using AxCrypt.Api.Model;
using AxCrypt.Api.Model.Secret;
using AxCrypt.Common;
using AxCrypt.Core.Crypto;
using AxCrypt.Core.Crypto.Asymmetric;
using AxCrypt.Core.IO;
using AxCrypt.Core.Session;
using AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Core.Service.Secrets
{
    public class LocalSecretsService : ISecretsService
    {
        private IDataContainer _workContainer;

        public static readonly string SecretsFileName = "AxSecrets.txt";

        public static readonly string SharedSecretsFileName = "AxSharedSecrets.txt";

        public LocalSecretsService(LogOnIdentity identity, IDataContainer workContainer)
        {
            if (identity == null)
            {
                throw new ArgumentNullException(nameof(identity));
            }
            if (workContainer == null)
            {
                throw new ArgumentNullException(nameof(workContainer));
            }

            Identity = identity;
            _workContainer = workContainer;
        }

        public ISecretsService Refresh()
        {
            return this;
        }

        private IDataStore AxSecretsStore
        {
            get
            {
                return _workContainer.FileItemInfo(SecretsFileName);
            }
        }

        private IDataStore AxSharedSecretsStore
        {
            get
            {
                return _workContainer.FileItemInfo(SharedSecretsFileName);
            }
        }

        public LogOnIdentity Identity
        {
            get;
        }

        /// <summary>
        /// Fetches the encrypted user secrets.
        /// </summary>
        /// <returns>
        /// The encrypted user secrets.
        /// </returns>
        public Task<EncryptedSecretApiModel> GetSecretsAsync()
        {
            if (Identity.UserEmail == EmailAddress.Empty)
            {
                throw new InvalidOperationException("The account service requires a user.");
            }
            EncryptedSecretApiModel userSecrets = LoadSecretsAsync();
            return Task.FromResult(userSecrets);
        }

        public async Task<bool> SaveSecretsAsync(EncryptedSecretApiModel secretsModel)
        {
            if (Identity.UserEmail == EmailAddress.Empty)
            {
                throw new InvalidOperationException("The account service requies a user.");
            }

            return await Task.Run(() => InternalSaveSecrets(secretsModel));
        }

        public Task<PasswordSuggestion> SuggestPasswordAsync(int level)
        {
            return Task.FromResult(new PasswordSuggestion("", 0));
        }

        public Task<IEnumerable<ShareSecretApiModel>> GetSharedWithSecretsAsync(SecretsListRequestOptions requestOptions)
        {
            if (Identity.UserEmail == EmailAddress.Empty)
            {
                throw new InvalidOperationException("The account service requires a user.");
            }
            IEnumerable<ShareSecretApiModel> userSecrets = LoadSharedSecretAsync();
            userSecrets = userSecrets.Where(us => us.SharedWith.Any(sw => sw.Deleted == null || sw.Visibility > New<INow>().Utc));
            return Task.FromResult(userSecrets);
        }

        public async Task<UserPublicKey> OtherPublicKeyAsync(EmailAddress email)
        {
            if (Identity.UserEmail == EmailAddress.Empty)
            {
                throw new InvalidOperationException("The account service requires a user.");
            }

            return await Task.Run(() =>
            {
                using (KnownPublicKeys knowPublicKeys = New<KnownPublicKeys>())
                {
                    UserPublicKey publicKey = knowPublicKeys.PublicKeys.Where(pk => pk.Email == email).FirstOrDefault()!;
                    return publicKey!;
                }
            }).Free();
        }

        public Task<bool> ShareSecretsAsync(ShareSecretApiModel sharedSecret)
        {
            return Task.FromResult(false);
        }

        public async Task<bool> UpdateSecretSharedWithAsync(ShareSecretApiModel sharedSecret)
        {
            if (Identity.UserEmail == EmailAddress.Empty)
            {
                throw new InvalidOperationException("The account service requies a user.");
            }

            return await Task.Run(() => InternalSaveSharedSecret(sharedSecret));
        }

        public Task<bool> DeleteSecretSharedAsync(ShareSecretApiModel sharedSecret)
        {
            return Task.FromResult(false);
        }

        private readonly long _maxAllowedSecretsCount = 10;

        public Task<long> GetFreeUserSecretsCount(string userEmail)
        {
            return Task.FromResult(_maxAllowedSecretsCount);
        }

        public Task<bool> InsertFreeUserSecretsAsync(string userEmail)
        {
            return Task.FromResult(false);
        }

        #region internal helper methods

        private EncryptedSecretApiModel LoadSecretsAsync()
        {
            if (!AxSecretsStore.IsAvailable)
            {
                return EncryptedSecretApiModel.Empty;
            }

            using (StreamReader reader = new StreamReader(AxSecretsStore.OpenRead()))
            {
                EncryptedSecretApiModel secretsModel = DeserializeFrom(reader);
                if (secretsModel == null)
                {
                    secretsModel = EncryptedSecretApiModel.Empty;
                }

                return secretsModel;
            }
        }

        private bool InternalSaveSecrets(EncryptedSecretApiModel secretsModel)
        {
            using (StreamWriter writer = new StreamWriter(AxSecretsStore.OpenWrite()))
            {
                SerializeTo(writer, secretsModel);
            }

            return true;
        }

        private IEnumerable<ShareSecretApiModel> LoadSharedSecretAsync()
        {
            if (!AxSharedSecretsStore.IsAvailable)
            {
                return new List<ShareSecretApiModel>();
            }

            using (StreamReader reader = new StreamReader(AxSharedSecretsStore.OpenRead()))
            {
                IEnumerable<ShareSecretApiModel> secretsModel = DeserializeSharedSecretsFrom(reader);
                if (secretsModel == null)
                {
                    secretsModel = new List<ShareSecretApiModel>();
                }

                return secretsModel;
            }
        }

        private bool InternalSaveSharedSecret(ShareSecretApiModel sharedSecretModel)
        {
            IList<ShareSecretApiModel> userSharedSecrets = LoadSharedSecretAsync().ToList();
            ShareSecretApiModel existingSharedSecretApiModel = userSharedSecrets.FirstOrDefault(uss => uss.SecretId == sharedSecretModel.SecretId)!;
            if (existingSharedSecretApiModel == null)
            {
                userSharedSecrets.Add(sharedSecretModel);
            }
            else
            {
                userSharedSecrets.Remove(existingSharedSecretApiModel);
                userSharedSecrets.Add(sharedSecretModel);
            }

            using (StreamWriter writer = new StreamWriter(AxSharedSecretsStore.OpenWrite()))
            {
                SerializeShareSecretsTo(writer, userSharedSecrets);
            }

            return true;
        }

        #endregion internal helper methods

        #region Text Serializers

        public static EncryptedSecretApiModel DeserializeFrom(TextReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            string serialized = reader.ReadToEnd();
            return New<IStringSerializer>().Deserialize<EncryptedSecretApiModel>(serialized)!;
        }

        public static IEnumerable<ShareSecretApiModel> DeserializeSharedSecretsFrom(TextReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            string serialized = reader.ReadToEnd();
            return New<IStringSerializer>().Deserialize<IEnumerable<ShareSecretApiModel>>(serialized)!;
        }

        public void SerializeTo(TextWriter writer, EncryptedSecretApiModel encryptedSecretsModel)
        {
            string serializedString = New<IStringSerializer>().Serialize(encryptedSecretsModel);
            WriteToFile(writer, serializedString);
        }

        public void SerializeShareSecretsTo(TextWriter writer, IEnumerable<ShareSecretApiModel> encryptedSharedSecretsModel)
        {
            string serializedString = New<IStringSerializer>().Serialize(encryptedSharedSecretsModel);
            WriteToFile(writer, serializedString);
        }

        private static void WriteToFile(TextWriter writer, string serializedString)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            writer.Write(serializedString);
        }

        #endregion Text Serializers
    }
}
