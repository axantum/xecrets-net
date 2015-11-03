using Axantum.AxCrypt.Api;
using Axantum.AxCrypt.Api.Model;
using Axantum.AxCrypt.Common;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Crypto.Asymmetric;
using Axantum.AxCrypt.Core.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.Service
{
    /// <summary>
    /// Implement account service functionality for a device, using a local and a remote service instance. This
    /// class determines the interaction and behavior when the services cooperate to provide a robust service
    /// despite possible remote outages.
    /// An instance operates on behalf an identity, or an anonymous one.
    /// </summary>
    public class DeviceAccountService : IAccountService
    {
        private IAccountService _localService;

        private IAccountService _remoteService;

        public DeviceAccountService(IAccountService localService, IAccountService remoteService)
        {
            _localService = localService;
            _remoteService = remoteService;
        }

        public bool HasAccounts
        {
            get
            {
                try
                {
                    return _remoteService.HasAccounts;
                }
                catch (OfflineApiException)
                {
                    return _localService.HasAccounts;
                }
            }
        }

        public LogOnIdentity Identity
        {
            get
            {
                return _remoteService.Identity;
            }
        }

        public SubscriptionLevel Level
        {
            get
            {
                try
                {
                    return _remoteService.Level;
                }
                catch (OfflineApiException)
                {
                    return _localService.Level;
                }
            }
        }

        public bool ChangePassphrase(Passphrase passphrase)
        {
            try
            {
                return _remoteService.ChangePassphrase(passphrase);
            }
            catch (OfflineApiException)
            {
                return _localService.ChangePassphrase(passphrase);
            }
        }

        public async Task<IList<UserKeyPair>> ListAsync()
        {
            IList<UserKeyPair> localKeys = await _localService.ListAsync().Free();
            try
            {
                IList<UserKeyPair> remoteKeys = await _remoteService.ListAsync().Free();
                IList<UserKeyPair> allKeys = remoteKeys.Union(localKeys).ToList();
                if (allKeys.Count() == 0)
                {
                    UserKeyPair currentKeyPair = await _remoteService.CurrentKeyPairAsync().Free();
                    if (currentKeyPair != null)
                    {
                        remoteKeys.Add(currentKeyPair);
                    }
                    allKeys = remoteKeys;
                }
                if (allKeys.Count() > remoteKeys.Count())
                {
                    await _remoteService.SaveAsync(allKeys).Free();
                }
                if (allKeys.Count() > localKeys.Count())
                {
                    await _localService.SaveAsync(allKeys).Free();
                }
                return allKeys;
            }
            catch (OfflineApiException)
            {
                return localKeys;
            }
        }

        public async Task<UserKeyPair> CurrentKeyPairAsync()
        {
            try
            {
                UserKeyPair currentUserKeyPair = await _remoteService.CurrentKeyPairAsync().Free();
                return currentUserKeyPair;
            }
            catch (OfflineApiException)
            {
                return null;
            }
        }

        public async Task PasswordResetAsync(string verificationCode)
        {
            try
            {
                await _remoteService.PasswordResetAsync(verificationCode).Free();
            }
            catch (OfflineApiException)
            {
                await _localService.PasswordResetAsync(verificationCode).Free();
            }
        }

        public async Task<UserPublicKey> PublicKeyAsync()
        {
            UserPublicKey publicKey = await _localService.PublicKeyAsync().Free();
            try
            {
                publicKey = await _remoteService.PublicKeyAsync().Free();
                New<KnownPublicKeys>().AddOrReplace(publicKey);
            }
            catch (OfflineApiException)
            {
            }
            return publicKey;
        }

        public async Task SaveAsync(IEnumerable<UserKeyPair> keyPairs)
        {
            try
            {
                await _remoteService.SaveAsync(keyPairs).Free();
            }
            catch (OfflineApiException)
            {
                await _localService.SaveAsync(keyPairs).Free();
            }
        }

        public async Task SignupAsync(string emailAddress)
        {
            try
            {
                await _remoteService.SignupAsync(emailAddress).Free();
            }
            catch (OfflineApiException)
            {
                await _localService.SignupAsync(emailAddress).Free();
            }
        }

        public async Task<AccountStatus> StatusAsync()
        {
            try
            {
                return await _remoteService.StatusAsync();
            }
            catch (OfflineApiException)
            {
                AccountStatus status = await _localService.StatusAsync();
                if (status == AccountStatus.NotFound)
                {
                    return AccountStatus.Offline;
                }
                return status;
            }
        }
    }
}