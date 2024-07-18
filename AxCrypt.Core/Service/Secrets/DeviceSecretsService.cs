using AxCrypt.Api;
using AxCrypt.Api.Model;
using AxCrypt.Api.Model.Secret;
using AxCrypt.Common;
using AxCrypt.Core.Crypto;
using AxCrypt.Core.Crypto.Asymmetric;
using AxCrypt.Core.Extensions;
using AxCrypt.Core.Session;
using AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Core.Service.Secrets
{
    /// <summary>
    /// Implement secrets service functionality for a device, using a local and a remote service instance. This
    /// class determines the interaction and behavior when the services cooperate to provide a robust service
    /// despite possible remote outages.
    /// An instance operates on behalf an identity, or an anonymous one.
    /// </summary>
    public class DeviceSecretsService : ISecretsService
    {
        private ISecretsService _localService;

        private ISecretsService _remoteService;

        public DeviceSecretsService(ISecretsService localService, ISecretsService remoteService)
        {
            _localService = localService;
            _remoteService = remoteService;
        }

        public ISecretsService Refresh()
        {
            return this;
        }

        public LogOnIdentity Identity
        {
            get
            {
                return _remoteService.Identity;
            }
        }

        /// <summary>
        /// Fetches the encrypted user secrets.
        /// </summary>
        /// <returns>
        /// The encrypted user secrets.
        /// </returns>
        public async Task<EncryptedSecretApiModel> GetSecretsAsync()
        {
            EncryptedSecretApiModel localUserSecrets = await _localService.GetSecretsAsync().Free();
            if (!New<AxCryptOnlineState>().IsOnline || Identity == LogOnIdentity.Empty)
            {
                return localUserSecrets;
            }

            try
            {
                EncryptedSecretApiModel remoteSecrets = await _remoteService.GetSecretsAsync().Free();
                if (remoteSecrets == null)
                {
                    return localUserSecrets;
                }

                if (localUserSecrets != remoteSecrets)
                {
                    await _localService.SaveSecretsAsync(remoteSecrets).Free();
                }

                return remoteSecrets;
            }
            catch (ApiException aex)
            {
                await aex.HandleApiExceptionAsync();
            }

            return localUserSecrets;
        }

        public async Task<bool> SaveSecretsAsync(EncryptedSecretApiModel encSecrets)
        {
            if (New<AxCryptOnlineState>().IsOnline)
            {
                try
                {
                    return await _remoteService.SaveSecretsAsync(encSecrets).Free();
                }
                catch (ApiException aex)
                {
                    await aex.HandleApiExceptionAsync();
                }
            }

            return await _localService.SaveSecretsAsync(encSecrets).Free();
        }

        public async Task<IEnumerable<ShareSecretApiModel>> GetSharedWithSecretsAsync(SecretsListRequestOptions requestOptions)
        {
            IEnumerable<ShareSecretApiModel> localUserSecrets = await _localService.GetSharedWithSecretsAsync(requestOptions).Free();
            if (!New<AxCryptOnlineState>().IsOnline || Identity == LogOnIdentity.Empty)
            {
                return localUserSecrets;
            }

            try
            {
                IEnumerable<ShareSecretApiModel> remoteSecrets = await _remoteService.GetSharedWithSecretsAsync(requestOptions).Free();
                if (remoteSecrets == null)
                {
                    return localUserSecrets;
                }

                if (localUserSecrets != remoteSecrets)
                {
                    await _localService.ShareSecretsAsync(remoteSecrets.LastOrDefault()).Free();
                }

                return remoteSecrets;
            }
            catch (ApiException aex)
            {
                await aex.HandleApiExceptionAsync();
            }

            return localUserSecrets;
        }

        public async Task<UserPublicKey> OtherPublicKeyAsync(EmailAddress email)
        {
            return await OtherUserPublicKeysAsync(() => _localService.OtherPublicKeyAsync(email), () => _remoteService.OtherPublicKeyAsync(email)).Free();
        }

        private async Task<UserPublicKey> OtherUserPublicKeysAsync(Func<Task<UserPublicKey>> localServiceOtherUserPublicKey, Func<Task<UserPublicKey>> remoteServiceOtherUserPublicKey)
        {
            UserPublicKey publicKey = await localServiceOtherUserPublicKey().Free();
            if (New<AxCryptOnlineState>().IsOffline)
            {
                return NonNullPublicKey(publicKey);
            }

            try
            {
                publicKey = await remoteServiceOtherUserPublicKey().Free();
                using (KnownPublicKeys knownPublicKeys = New<KnownPublicKeys>())
                {
                    knownPublicKeys.AddOrReplace(publicKey);
                }
            }
            catch (ApiException aex)
            {
                await aex.HandleApiExceptionAsync();
            }

            return publicKey;
        }

        private static UserPublicKey NonNullPublicKey(UserPublicKey publicKey)
        {
            if (publicKey != null)
            {
                return publicKey;
            }
            throw new OfflineApiException("Can't find other non-cached public key when offline.");
        }

        public async Task<bool> ShareSecretsAsync(ShareSecretApiModel encSecrets)
        {
            if (New<AxCryptOnlineState>().IsOnline)
            {
                try
                {
                    return await _remoteService.ShareSecretsAsync(encSecrets).Free();
                }
                catch (ApiException aex)
                {
                    await aex.HandleApiExceptionAsync();
                }
            }

            return await _localService.ShareSecretsAsync(encSecrets).Free();
        }

        public async Task<bool> UpdateSecretSharedWithAsync(ShareSecretApiModel encSecrets)
        {
            if (New<AxCryptOnlineState>().IsOnline)
            {
                try
                {
                    return await _remoteService.UpdateSecretSharedWithAsync(encSecrets).Free();
                }
                catch (ApiException aex)
                {
                    await aex.HandleApiExceptionAsync();
                }
            }

            return await _localService.UpdateSecretSharedWithAsync(encSecrets).Free();
        }

        public async Task<bool> DeleteSecretSharedAsync(ShareSecretApiModel encSecrets)
        {
            if (New<AxCryptOnlineState>().IsOnline)
            {
                try
                {
                    return await _remoteService.DeleteSecretSharedAsync(encSecrets).Free();
                }
                catch (ApiException aex)
                {
                    await aex.HandleApiExceptionAsync();
                }
            }

            return await _localService.DeleteSecretSharedAsync(encSecrets).Free();
        }

        public async Task<PasswordSuggestion> SuggestPasswordAsync(int level)
        {
            if (New<AxCryptOnlineState>().IsOnline)
            {
                try
                {
                    return await _remoteService.SuggestPasswordAsync(level).Free();
                }
                catch (ApiException aex)
                {
                    await aex.HandleApiExceptionAsync();
                }
            }

            return await _localService.SuggestPasswordAsync(level).Free();
        }

        private readonly long _maxAllowedSecretsCount = 10;

        public async Task<long> GetFreeUserSecretsCount(string userEmail)
        {
            if (New<AxCryptOnlineState>().IsOnline)
            {
                try
                {
                    return await _remoteService.GetFreeUserSecretsCount(userEmail).Free();
                }
                catch (ApiException aex)
                {
                    await aex.HandleApiExceptionAsync();
                }
            }

            return _maxAllowedSecretsCount;
        }

        public async Task<bool> InsertFreeUserSecretsAsync(string userEmail)
        {
            if (New<AxCryptOnlineState>().IsOnline)
            {
                try
                {
                    return await _remoteService.InsertFreeUserSecretsAsync(userEmail).Free();
                }
                catch (ApiException aex)
                {
                    await aex.HandleApiExceptionAsync();
                }
            }

            return false;
        }
    }
}