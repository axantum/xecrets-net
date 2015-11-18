using Axantum.AxCrypt.Api.Model;
using Axantum.AxCrypt.Common;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Crypto.Asymmetric;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.UI;
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
                if (New<AxCryptOnlineState>().IsOnline)
                {
                    try
                    {
                        return _remoteService.HasAccounts;
                    }
                    catch (OfflineApiException)
                    {
                        New<AxCryptOnlineState>().IsOffline = true;
                    }
                }
                return _localService.HasAccounts;
            }
        }

        public LogOnIdentity Identity
        {
            get
            {
                return _remoteService.Identity;
            }
        }

        public async Task<SubscriptionLevel> LevelAsync()
        {
            if (New<AxCryptOnlineState>().IsOnline && Identity != LogOnIdentity.Empty)
            {
                try
                {
                    return await _remoteService.LevelAsync().Free();
                }
                catch (OfflineApiException)
                {
                    New<AxCryptOnlineState>().IsOffline = true;
                }
            }
            return await _localService.LevelAsync().Free();
        }

        public bool ChangePassphrase(Passphrase passphrase)
        {
            if (New<AxCryptOnlineState>().IsOnline)
            {
                try
                {
                    return _remoteService.ChangePassphrase(passphrase);
                }
                catch (OfflineApiException)
                {
                    New<AxCryptOnlineState>().IsOffline = true;
                }
            }
            return _localService.ChangePassphrase(passphrase);
        }

        public async Task<IList<UserKeyPair>> ListAsync()
        {
            IList<UserKeyPair> localKeys = await _localService.ListAsync().Free();
            if (New<AxCryptOnlineState>().IsOffline)
            {
                return localKeys;
            }

            try
            {
                IList<UserKeyPair> remoteKeys = new List<UserKeyPair>();
                try
                {
                    remoteKeys = await _remoteService.ListAsync().Free();
                }
                catch (PasswordException)
                {
                    if (localKeys.Count == 0)
                    {
                        throw;
                    }
                    return localKeys;
                }

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
                New<AxCryptOnlineState>().IsOffline = true;
            }
            return localKeys;
        }

        public async Task<UserKeyPair> CurrentKeyPairAsync()
        {
            if (New<AxCryptOnlineState>().IsOnline)
            {
                try
                {
                    UserKeyPair currentUserKeyPair = await _remoteService.CurrentKeyPairAsync().Free();
                    return currentUserKeyPair;
                }
                catch (OfflineApiException)
                {
                    New<AxCryptOnlineState>().IsOffline = true;
                }
            }
            return null;
        }

        public async Task PasswordResetAsync(string verificationCode)
        {
            if (New<AxCryptOnlineState>().IsOnline)
            {
                try
                {
                    await _remoteService.PasswordResetAsync(verificationCode).Free();
                }
                catch (OfflineApiException)
                {
                    New<AxCryptOnlineState>().IsOffline = true;
                }
            }
            await _localService.PasswordResetAsync(verificationCode).Free();
        }

        public async Task<UserPublicKey> OtherPublicKeyAsync(EmailAddress email)
        {
            UserPublicKey publicKey = await _localService.OtherPublicKeyAsync(email).Free();
            if (New<AxCryptOnlineState>().IsOnline)
            {
                try
                {
                    publicKey = await _remoteService.OtherPublicKeyAsync(email).Free();
                    using (KnownPublicKeys knowPublicKeys = New<KnownPublicKeys>())
                    {
                        knowPublicKeys.AddOrReplace(publicKey);
                    }
                }
                catch (OfflineApiException)
                {
                    New<AxCryptOnlineState>().IsOffline = true;
                }
            }
            return publicKey;
        }

        public async Task SaveAsync(IEnumerable<UserKeyPair> keyPairs)
        {
            if (New<AxCryptOnlineState>().IsOnline)
            {
                try
                {
                    await _remoteService.SaveAsync(keyPairs).Free();
                }
                catch (OfflineApiException)
                {
                    New<AxCryptOnlineState>().IsOffline = true;
                }
            }
            await _localService.SaveAsync(keyPairs).Free();
        }

        public async Task SignupAsync(EmailAddress email)
        {
            if (New<AxCryptOnlineState>().IsOnline)
            {
                try
                {
                    await _remoteService.SignupAsync(email).Free();
                }
                catch (OfflineApiException)
                {
                    New<AxCryptOnlineState>().IsOffline = true;
                }
            }
            await _localService.SignupAsync(email).Free();
        }

        public async Task<AccountStatus> StatusAsync(EmailAddress email)
        {
            if (New<AxCryptOnlineState>().IsOnline)
            {
                try
                {
                    return await _remoteService.StatusAsync(email);
                }
                catch (OfflineApiException)
                {
                    New<AxCryptOnlineState>().IsOffline = true;
                }
            }
            AccountStatus status = await _localService.StatusAsync(email);
            if (status == AccountStatus.NotFound)
            {
                return AccountStatus.Offline;
            }
            return status;
        }
    }
}