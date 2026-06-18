#region Coypright and GPL License

/*
 * Xecrets.Net - Copyright © 2022-2026, Svante Seleborg, All Rights Reserved.
 *
 * This code file is part of Xecrets.Net, parts of which in turn are derived from AxCrypt as licensed under GPL v3 or later.
 * 
 * However, this code is not derived from AxCrypt and is separately copyrighted and only licensed as follows unless
 * explicitly licensed otherwise. If you use any part of this code in your software, please see https://www.gnu.org/licenses/
 * for details of what this means for you.
 *
 * Xecrets.Net is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * Xecrets.Net is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied
 * warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License along with Xecrets.Net.  If not, see <https://www.gnu.org/licenses/>.
 *
 * The source repository can be found at https://github.com/axantum/xecrets-net please go there for more information,
 * suggestions and contributions. You may also visit https://www.axantum.com for more information about the author.
 */

#endregion Coypright and GPL License

using AxCrypt.Abstractions;
using AxCrypt.Core;
using AxCrypt.Core.Crypto;
using AxCrypt.Core.Crypto.Asymmetric;
using AxCrypt.Core.Service;

using static AxCrypt.Abstractions.TypeResolve;

using Xecrets.Core.Public;

namespace Xecrets.Core;

internal static class Extensions
{
    internal static XecretsCoreException ToXecretsCoreException(this AxCryptException exception) =>
        new XecretsCoreException(exception.Message, exception.ErrorStatus.ToErrorCode(), exception);

    private static ErrorCode ToErrorCode(this ErrorStatus errorStatus)
    {
        return errorStatus switch
        {
            ErrorStatus.Success => ErrorCode.Success,
            ErrorStatus.UnspecifiedError => ErrorCode.UnspecifiedError,
            ErrorStatus.FileAlreadyExists => ErrorCode.FileAlreadyExists,
            ErrorStatus.FileDoesNotExist => ErrorCode.FileDoesNotExist,
            ErrorStatus.CannotWriteDestination => ErrorCode.CannotWriteDestination,
            ErrorStatus.CannotStartApplication => ErrorCode.CannotStartApplication,
            ErrorStatus.InconsistentState => ErrorCode.InconsistentState,
            ErrorStatus.InvalidKey => ErrorCode.InvalidKey,
            ErrorStatus.Canceled => ErrorCode.Canceled,
            ErrorStatus.Exception => ErrorCode.Exception,
            ErrorStatus.Unknown => ErrorCode.Unknown,
            ErrorStatus.InvalidPath => ErrorCode.InvalidPath,
            ErrorStatus.Working => ErrorCode.Working,
            ErrorStatus.Aborted => ErrorCode.Aborted,
            ErrorStatus.FileAlreadyEncrypted => ErrorCode.FileAlreadyEncrypted,
            ErrorStatus.FolderAlreadyWatched => ErrorCode.FolderAlreadyWatched,
            ErrorStatus.FileLocked => ErrorCode.FileLocked,
            ErrorStatus.MagicGuidMissing => ErrorCode.MagicGuidMissing,
            ErrorStatus.InternalError => ErrorCode.InternalError,
            ErrorStatus.EndOfStream => ErrorCode.EndOfStream,
            ErrorStatus.TooNewFileFormatVersion => ErrorCode.TooNewFileFormatVersion,
            ErrorStatus.TooOldFileFormatVersion => ErrorCode.TooOldFileFormatVersion,
            ErrorStatus.FileFormatError => ErrorCode.FileFormatError,
            ErrorStatus.HmacValidationError => ErrorCode.HmacValidationError,
            ErrorStatus.DataError => ErrorCode.DataError,
            ErrorStatus.FileExists => ErrorCode.FileExists,
            ErrorStatus.CryptographicError => ErrorCode.CryptographicError,
            ErrorStatus.ApiError => ErrorCode.ApiError,
            ErrorStatus.ApiHttpResponseError => ErrorCode.ApiHttpResponseError,
            ErrorStatus.ApiUnauthorizedError => ErrorCode.ApiUnauthorizedError,
            ErrorStatus.ApiOffline => ErrorCode.ApiOffline,
            ErrorStatus.WrongPassword => ErrorCode.WrongPassword,
            ErrorStatus.Exit => ErrorCode.Exit,
            ErrorStatus.BadApiRequest => ErrorCode.BadApiRequest,
            ErrorStatus.FileWriteProtected => ErrorCode.FileWriteProtected,
            ErrorStatus.WrongFileExtensionError => ErrorCode.WrongFileExtensionError,
            ErrorStatus.InvalidBlockLength => ErrorCode.InvalidBlockLength,
            ErrorStatus.UnexpectedEndOfFile => ErrorCode.UnexpectedEndOfFile,
            ErrorStatus.UnexpectedHeaderBlockType => ErrorCode.UnexpectedHeaderBlockType,
            ErrorStatus.ZeroLengthFile => ErrorCode.ZeroLengthFile,
            _ => ErrorCode.Unknown,
        };
    }

    internal static IEnumerable<LogOnIdentity> ToLogOnIdentities(this IEnumerable<Identity> identities) =>
        identities.Select(identity => new LogOnIdentity(identity.KeyPairs.Select(ToUserKeyPair), Passphrase.Create(identity.Passphrase)));

    internal static EncryptedWithParameters ExtractEncryptionCredentials(this IAxCryptDocument document)
    {
        List<PublicKey> masterKeys = [];
        if (document.AsymmetricMasterKey != null)
        {
            masterKeys.Add(document.AsymmetricMasterKey.ToPublicKey());
        }

        masterKeys.AddRange(document.AsymmetricMasterKeys.Select(ToPublicKey));

        return new EncryptedWithParameters(
            document.DecryptionParameter?.Passphrase?.Text ?? string.Empty,
            [.. document.AsymmetricRecipients.Select(ToPublicKey)],
            masterKeys);
    }

    extension(UserKeyPair userKeyPair)
    {
        internal KeyPair ToKeyPair(Passphrase encryptionPassphrase)
        {
            byte[] encryptedBytes = encryptionPassphrase == Passphrase.Empty ? [] : userKeyPair.ToArray(encryptionPassphrase);
            return userKeyPair.ToKeyPair(encryptedBytes);
        }

        internal KeyPair ToKeyPair(byte[] encryptedBytes)
        {
            return new KeyPair(
                userKeyPair.UserEmail.Address,
                New<IStringSerializer>().Serialize(userKeyPair),
                new UserPublicKey(userKeyPair.UserEmail, userKeyPair.KeyPair.PublicKey).ToPublicKey(),
                userKeyPair.Timestamp,
                encryptedBytes);
        }
    }

    internal static PublicKey ToPublicKey(this UserPublicKey publicKey) =>
        new PublicKey(publicKey.Email.Address, New<IStringSerializer>().Serialize(publicKey), publicKey.PublicKey.Tag);

    private static PublicKey ToPublicKey(this IAsymmetricPublicKey publicKey) =>
        new PublicKey(string.Empty, New<IStringSerializer>().Serialize(publicKey), publicKey.Tag);

    private static UserKeyPair ToUserKeyPair(this KeyPair keyPair) =>
        New<IStringSerializer>().Deserialize<UserKeyPair>(keyPair.SerializedKeyPair)
               ?? throw new InvalidDataException("The key pair could not be deserialized.");

    extension(PublicKey publicKey)
    {
        internal UserPublicKey ToUserPublicKey() =>
            New<IStringSerializer>().Deserialize<UserPublicKey>(publicKey.SerializedKey)
                   ?? throw new InvalidDataException("The public key could not be deserialized.");

        internal IAsymmetricPublicKey ToAsymmetricPublicKey() =>
            New<IStringSerializer>().Deserialize<IAsymmetricPublicKey>(publicKey.SerializedKey)
                   ?? throw new InvalidDataException("The asymmetric public key could not be deserialized.");
    }
}
