using Axantum.AxCrypt.Api;
using Axantum.AxCrypt.Api.Model;
using Axantum.AxCrypt.Core.Crypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                    return _remoteService.AccountAsync().Result.AccountKeys.Count() > 0;
                }
                catch (OfflineApiException)
                {
                    return _localService.AccountAsync().Result.AccountKeys.Count() > 0;
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

        public async Task<UserAccount> AccountAsync()
        {
            try
            {
                return await _remoteService.AccountAsync().ConfigureAwait(false);
            }
            catch (OfflineApiException)
            {
                return await _localService.AccountAsync().ConfigureAwait(false);
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
            try
            {
                return await _remoteService.ListAsync().ConfigureAwait(false);
            }
            catch (OfflineApiException)
            {
                return await _localService.ListAsync().ConfigureAwait(false);
            }
        }

        public async Task PasswordResetAsync(string verificationCode)
        {
            try
            {
                await _remoteService.PasswordResetAsync(verificationCode).ConfigureAwait(false);
            }
            catch (OfflineApiException)
            {
                await _localService.PasswordResetAsync(verificationCode).ConfigureAwait(false);
            }
        }

        public async Task SaveAsync(IEnumerable<UserKeyPair> keyPairs)
        {
            try
            {
                await _remoteService.SaveAsync(keyPairs).ConfigureAwait(false);
            }
            catch (OfflineApiException)
            {
                await _localService.SaveAsync(keyPairs).ConfigureAwait(false);
            }
        }

        public async Task SignupAsync(string emailAddress)
        {
            try
            {
                await _remoteService.SignupAsync(emailAddress).ConfigureAwait(false);
            }
            catch (OfflineApiException)
            {
                await _localService.SignupAsync(emailAddress).ConfigureAwait(false);
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