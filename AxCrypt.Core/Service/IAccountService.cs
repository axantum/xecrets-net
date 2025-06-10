using System;
using System.Linq;

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

using AxCrypt.Api.Model;
using AxCrypt.Core.Crypto;
using AxCrypt.Core.Crypto.Asymmetric;
using AxCrypt.Core.UI;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using AxCrypt.Api.Model.Groups;

namespace AxCrypt.Core.Service
{
    /// <summary>
    /// The account service. Methods and properties to work with an account.
    /// </summary>
    public interface IAccountService
    {
        /// <summary>
        /// Refresh all values by ensuring flushing any caches etc.
        /// </summary>
        IAccountService Refresh();

        /// <summary>
        /// Gets a value indicating whether the service has any accounts at all.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has accounts; otherwise, <c>false</c>.
        /// </value>
        Task<bool> HasAccountsAsync();

        /// <summary>
        /// Gets the identity this instance works with.
        /// </summary>
        /// <value>
        /// The identity.
        /// </value>
        LogOnIdentity Identity { get; }

        /// <summary>
        /// Gets the status of the account.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        Task<AccountStatus> StatusAsync(EmailAddress email);

        /// <summary>
        /// Gets the offers used by the account.
        /// </summary>
        /// <returns>A combination of flags for offers used.</returns>
        Task<Offers> OffersAsync();

        /// <summary>
        /// Start a premium trial now.
        /// </summary>
        /// <returns></returns>
        Task StartPremiumTrialAsync();

        /// <summary>
        /// Changes the passphrase for the account.
        /// </summary>
        /// <param name="passphrase">The passphrase.</param>
        /// <returns>true if the passphrase was successfully changed.</returns>
        Task<bool> ChangePassphraseAsync(Passphrase passphrase);

        /// <summary>
        /// Fetches the user user account.
        /// </summary>
        /// <returns>The complete user account information. If none is available, a default empty UserAccount is returned.</returns>
        Task<UserAccount> AccountAsync();

        /// <summary>
        /// Lists all UserKeyPairs available for the user with the provided Identity, if any. This may filter out
        /// key pairs stored in the account, but where the private key is not decryptable with Identity.
        /// </summary>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        Task<IList<UserKeyPair>> ListAsync();

        /// <summary>
        /// Ensures there is at least one key pair, and returns the currently active decrypted key pair of the user.
        /// </summary>
        /// <returns>
        /// Will always return a current key pair.
        /// </returns>
        /// <remarks>
        /// This cannot be used to determine if a user has a key pair, as one will be created if there is none, or if
        /// none can be decrypted with the given Identity.
        /// </remarks>
        Task<UserKeyPair> CurrentKeyPairAsync();

        /// <summary>
        /// Ensures there is at least one key pair if possible, and returns the active public key of the user.
        /// </summary>
        /// <returns>The public key of the current key pair, or null if the service can't create other users.</returns>
        Task<UserPublicKey> OtherPublicKeyAsync(EmailAddress email);

        /// <summary>
        /// Ensures there is at least one key pair if possible, and returns the active public key of the user.
        /// </summary>
        /// <returns>The public key of the current key pair, or null if the service can't create other users.</returns>
        Task<UserPublicKey> OtherUserInvitePublicKeyAsync(EmailAddress email, CustomMessageParameters customParameters);

        /// <summary>
        /// Saves the account, merging keys with existing keys if necessary.
        /// </summary>
        /// <param name="account">The account.</param>
        /// <returns></returns>
        Task SaveAsync(UserAccount account);

        /// <summary>
        /// Saves the specified key pairs.
        /// </summary>
        /// <param name="keyPairs">The key pairs.</param>
        Task SaveAsync(IEnumerable<UserKeyPair> keyPairs);

        /// <summary>
        /// Signs a user up for AxCrypt, creating the account and sending verification link and code
        /// in an email to the provided email address in Identity.
        /// </summary>
        /// <returns></returns>
        Task SignupAsync(EmailAddress email, CultureInfo culture);

        /// <summary>
        /// Resets the password for the account.
        /// </summary>
        /// <param name="verificationCode">The verification code.</param>
        /// <returns></returns>
        Task PasswordResetAsync(string verificationCode);

        /// <summary>
        /// Sends user feedback about application.
        /// </summary>
        /// <param name="subject">The feedback subject.</param>
        /// <param name="message">The user message</param>
        /// <returns></returns>
        Task SendFeedbackAsync(string subject, string message);

        /// <summary>
        /// Creates a subscription for the AppStore InAppPurchase transaction.
        /// </summary>
        /// <param name="skTransactions">The appstore IAP transaction(s).</param>
        /// <returns>
        ///  /// <c>true</c> if subscription created; otherwise, <c>false</c>.
        ///  </returns>
        Task<bool> CreateSubscriptionAsync(StoreKitTransaction[] skTransactions);

        /// <summary>
        /// Get the in app purchase settings.
        /// </summary>
        /// <returns>The InAppPurchase product ids and discount information, or null if the service can't connect.</returns>
        Task<PurchaseSettings> GetInAppPurchaseSettingsAsync();

        /// <summary>
        /// Get a user auto renewal subscription status.
        /// </summary
        /// <value>
        /// <c>true</c> if auto renewal enabled; otherwise, <c>false</c>.
        /// </value>
        Task<bool> AutoRenewalStatusAsync();

        /// <summary>
        /// Deletes the user account.
        /// </summary>
        /// <returns>
        ///  /// <c>true</c> if user account deleted; otherwise, <c>false</c>.
        /// </returns>
        Task<bool> DeleteUserAsync();

        /// <summary>
        /// List all the user membered groups public/private keys.
        /// </summary>
        /// <returns>
        ///  /// List of groups public & private keys.
        /// </returns>
        Task<IEnumerable<GroupKeyPairApiModel>> ListMembershipGroupsAsync();
    }
}