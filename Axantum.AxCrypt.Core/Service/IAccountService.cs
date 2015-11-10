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

using Axantum.AxCrypt.Api.Model;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Crypto.Asymmetric;
using Axantum.AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Core.Service
{
    /// <summary>
    /// The account service. Methods and properties to work with an account.
    /// </summary>
    public interface IAccountService
    {
        /// <summary>
        /// Gets a value indicating whether the service has any accounts at all.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has accounts; otherwise, <c>false</c>.
        /// </value>
        bool HasAccounts { get; }

        /// <summary>
        /// Gets the identity this instance works with.
        /// </summary>
        /// <value>
        /// The identity.
        /// </value>
        LogOnIdentity Identity { get; }

        /// <summary>
        /// Gets the subscription level.
        /// </summary>
        /// <value>
        /// The level.
        /// </value>
        SubscriptionLevel Level { get; }

        /// <summary>
        /// Gets the status of the account.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        Task<AccountStatus> StatusAsync(EmailAddress email);

        /// <summary>
        /// Changes the passphrase for the account.
        /// </summary>
        /// <param name="passphrase">The passphrase.</param>
        /// <returns>true if the passphrase was successfully changed.</returns>
        bool ChangePassphrase(Passphrase passphrase);

        /// <summary>
        /// Lists all UserKeyPairs available for the user, if any.
        /// </summary>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        Task<IList<UserKeyPair>> ListAsync();

        /// <summary>
        /// Ensures there is at least one key pair, and returns the currently active key pair of the user.
        /// </summary>
        /// <returns>The current key pair</returns>
        Task<UserKeyPair> CurrentKeyPairAsync();

        /// <summary>
        /// Ensures there is at least one key pair if possible, and returns the active public key of the user.
        /// </summary>
        /// <returns>The public key of the current key pair, or null if the service can't create other users.</returns>
        Task<UserPublicKey> OtherPublicKeyAsync(EmailAddress email);

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
        Task SignupAsync(EmailAddress email);

        /// <summary>
        /// Resets the password for the account.
        /// </summary>
        /// <param name="verificationCode">The verification code.</param>
        /// <returns></returns>
        Task PasswordResetAsync(string verificationCode);
    }
}