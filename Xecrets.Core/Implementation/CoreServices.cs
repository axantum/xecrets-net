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

using System.Diagnostics.CodeAnalysis;

using AxCrypt.Abstractions;
using AxCrypt.Api.Model;
using AxCrypt.Core;
using AxCrypt.Core.Crypto;
using AxCrypt.Core.Crypto.Asymmetric;
using AxCrypt.Core.Extensions;
using AxCrypt.Core.Header;
using AxCrypt.Core.IO;
using AxCrypt.Core.Reader;
using AxCrypt.Core.Runtime;
using AxCrypt.Core.Service;
using AxCrypt.Core.UI;

using static AxCrypt.Abstractions.TypeResolve;

using KeyPair = Xecrets.Core.Models.KeyPair;

namespace Xecrets.Core.Implementation;

internal sealed class CoreServices : ICoreServices
{
    public Task EncryptAsync(Stream cleartext, Stream encrypted, EncryptRequest request)
        => Task.Run(async () =>
        {
            try
            {
                Passphrase passphrase = Passphrase.Create(request.Passphrase);
                EncryptionParameters encryptionParameters = new(new V2Aes256CryptoFactory().CryptoId, passphrase);
                encryptionParameters.AddOrReplace(request.Recipients.Select(Extensions.ToUserPublicKey));

                IAsymmetricPublicKey[] masterKeys = [.. request.MasterKeys.Select(Extensions.ToAsymmetricPublicKey)];
                if (masterKeys.Length > 0)
                {
                    encryptionParameters.MasterPublicKey = masterKeys[0];
                    await encryptionParameters.AddMasterPublicKeyAsync(masterKeys);
                }

                using IAxCryptDocument document = New<AxCryptFactory>().CreateDocument(encryptionParameters);
                document.FileName = request.OriginalFileName;
                document.CreationTimeUtc = request.CreationTimeUtc;
                document.LastAccessTimeUtc = request.LastAccessTimeUtc;
                document.LastWriteTimeUtc = request.LastWriteTimeUtc;
                await using Stream progressCleartext = ProgressStream.Wrap(
                    ForwardOnlyStream.Wrap(cleartext, leaveOpen: true), request.Progress);
                await using Stream encryptedStream = ForwardOnlyStream.Wrap(encrypted, leaveOpen: true);
                document.EncryptTo(progressCleartext, encryptedStream,
                    request.Compress ? AxCryptOptions.EncryptWithCompression : AxCryptOptions.EncryptWithoutCompression);
            }
            catch (AxCryptException ex)
            {
                throw ex.ToXecretsCoreException();
            }
        });

    public Task<IDecryptionSession> OpenDecryptionAsync(Stream encrypted, DecryptRequest request)
        => Task.Run<IDecryptionSession>(() =>
        {
            Stream? progressEncrypted = null;
            try
            {
                progressEncrypted = ProgressStream.Wrap(
                    ForwardOnlyStream.Wrap(encrypted, leaveOpen: true), request.Progress);
                IAxCryptDocument document = CreateDocument(request.Identities.ToLogOnIdentities(),
                    progressEncrypted);
                IDecryptionSession session = new DecryptionSession(document);
                return session;
            }
            catch (OperationCanceledException)
            {
                progressEncrypted?.Dispose();
                throw;
            }
            catch (AxCryptException ex)
            {
                progressEncrypted?.Dispose();
                throw ex.ToXecretsCoreException();
            }
            catch
            {
                progressEncrypted?.Dispose();
                throw;
            }
        });

    public Task<KeyPair> CreateKeyPairAsync(string email, string passphrase, DateTimeOffset createdUtc)
        => Task.Run(() =>
        {
            try
            {
                EmailAddress emailAddress = EmailAddress.Parse(email);
                IAsymmetricKeyPair keyPair = Resolve.AsymmetricFactory.CreateKeyPair(4096);
                UserKeyPair userKeyPair = new(emailAddress, createdUtc.UtcDateTime, keyPair);
                return userKeyPair.ToKeyPair(Passphrase.Create(passphrase));
            }
            catch (FormatException ex)
            {
                throw new Public.XecretsCoreException($"The email address '{email}' is not valid.", Public.ErrorCode.Exception, ex);
            }
            catch (AxCryptException ex)
            {
                throw ex.ToXecretsCoreException();
            }
        });

    public bool TryLoadKeyPair(ReadOnlyMemory<byte> encryptedKeyPair,
        IReadOnlyList<string> passphrases, [NotNullWhen(true)] out LoadedKeyPair? loadedKeyPair)
    {
        loadedKeyPair = null;
        try
        {
            for (int i = 0; i < passphrases.Count; i++)
            {
                if (passphrases[i].Length == 0)
                {
                    continue;
                }
                Passphrase passphrase = Passphrase.Create(passphrases[i]);
                if (!UserKeyPair.TryLoad(encryptedKeyPair.ToArray(), passphrase, out UserKeyPair? userKeyPair) ||
                    userKeyPair == null)
                {
                    continue;
                }

                loadedKeyPair = new LoadedKeyPair(
                    userKeyPair.ToKeyPair(encryptedKeyPair.ToArray()), i);
                return true;
            }

            return false;
        }
        catch (AxCryptException ex)
        {
            throw ex.ToXecretsCoreException();
        }
    }

    public string ExportPublicKey(PublicKey publicKey)
    {
        return publicKey.SerializedKey;
    }

    public PublicKey? ImportPublicKey(string serializedPublicKey)
    {
        try
        {
            UserPublicKey? userPublicKey = New<IStringSerializer>().Deserialize<UserPublicKey>(serializedPublicKey);
            return userPublicKey?.ToPublicKey();
        }
        catch (AxCryptException ex)
        {
            throw ex.ToXecretsCoreException();
        }
    }

    public PrivateKeyImportResult ImportPrivateKeys(string serializedAccounts,
        PrivateKeyImportRequest request)
    {
        try
        {
            UserAccounts userAccounts = New<IStringSerializer>().Deserialize<UserAccounts>(serializedAccounts) ??
                throw new InvalidDataException("The private key account data could not be deserialized.");

            UserAccounts? reEncryptedAccounts =
                ReEncryptAccounts(request, userAccounts, out List<UserKeyPair> loadedKeyPairs);
            string? reEncryptedJson = null;
            if (reEncryptedAccounts != null)
            {
                reEncryptedJson = ReferenceEquals(userAccounts, reEncryptedAccounts)
                    ? serializedAccounts
                    : New<IStringSerializer>().Serialize(reEncryptedAccounts);
            }

            return new PrivateKeyImportResult(
                [.. loadedKeyPairs.Select(k => k.ToKeyPair(Passphrase.Empty))], reEncryptedJson);
        }
        catch (AxCryptException ex)
        {
            throw ex.ToXecretsCoreException();
        }
    }

    public bool TryParseEmail(string email, [NotNullWhen(true)] out string? address) => New<IEmailParser>().TryParse(email, out address);

    private static IAxCryptDocument CreateDocument(IEnumerable<LogOnIdentity> identities, Stream fromStream)
    {
        Headers headers = new();
        LookAheadStream lookAheadStream = new(fromStream);
        if (lookAheadStream.IsEmpty(16))
        {
            throw new FileFormatException("The stream contains no data, it's length is zero.",
                ErrorStatus.ZeroLengthFile);
        }

        AxCryptReaderBase reader = headers.CreateReader(lookAheadStream);
        bool isLegacyV1 = reader is V1AxCryptReader;
        IAxCryptDocument document = AxCryptReaderBase.Document(reader);

        foreach (DecryptionParameter decryptionParameter in DecryptionParameters(identities, isLegacyV1))
        {
            if (decryptionParameter.Passphrase != null &&
                document.Load(decryptionParameter.Passphrase, decryptionParameter.CryptoId, headers) ||
                decryptionParameter.PrivateKey != null &&
                document.Load(decryptionParameter.PrivateKey, decryptionParameter.CryptoId, headers))
            {
                document.DecryptionParameter = decryptionParameter;
                return document;
            }
        }

        return document;
    }

    private static DecryptionParameter[] DecryptionParameters(IEnumerable<LogOnIdentity> identities, bool isLegacyV1)
    {
        List<DecryptionParameter> decryptionParameters = [];
        foreach (LogOnIdentity identity in identities)
        {
            decryptionParameters.AddRange(DecryptionParameters(isLegacyV1, identity.Passphrase, identity.PrivateKeys));
        }

        List<DecryptionParameter> passwordsFirst =
            [.. decryptionParameters.Where(dp => dp.Passphrase != null && dp.Passphrase != Passphrase.Empty)];
        passwordsFirst.AddRange(decryptionParameters.Where(dp =>
            dp.Passphrase == null || dp.Passphrase == Passphrase.Empty));

        Guid[] cryptoIds = [.. Resolve.CryptoFactory.OrderedIds];
        return [.. passwordsFirst.OrderBy(dp => Array.IndexOf(cryptoIds, dp.CryptoId))];
    }

    private static IEnumerable<DecryptionParameter> DecryptionParameters(bool isLegacyV1, Passphrase passphrase,
        IEnumerable<IAsymmetricPrivateKey?> privateKeys)
    {
        Guid[] cryptoIds = isLegacyV1
            ? [new V1Aes128CryptoFactory().CryptoId]
            : [.. Resolve.CryptoFactory.OrderedIds.Where(id => id != new V1Aes128CryptoFactory().CryptoId)];

        Passphrase[] passphrases = passphrase == Passphrase.Empty ? [] : [passphrase];
        return DecryptionParameter.CreateAll(passphrases, privateKeys, cryptoIds);
    }


    private static UserAccounts? ReEncryptAccounts(PrivateKeyImportRequest request, UserAccounts userAccounts,
        out List<UserKeyPair> decryptedKeyPairs)
    {
        decryptedKeyPairs = [];
        if (userAccounts.Accounts.Count == 0)
        {
            return userAccounts;
        }

        if (string.IsNullOrEmpty(request.ReEncryptPassphrase))
        {
            return null;
        }

        Passphrase reEncryptionPassphrase = Passphrase.Create(request.ReEncryptPassphrase);
        List<Passphrase> passphrases =
            [.. request.Passphrases.Where(p => p != request.ReEncryptPassphrase).Select(Passphrase.Create)];
        string userEmail = string.IsNullOrEmpty(request.UserEmail)
            ? userAccounts.Accounts.First().UserName
            : request.UserEmail;

        List<AccountKey> nonDecryptableAccountKeys = [];
        bool statusChanged = userAccounts.Accounts.Any(a => a.UserName != userEmail);
        foreach (AccountKey key in userAccounts.Accounts.Select(a => a.AccountKeys).SelectMany(a => a))
        {
            statusChanged |= key.User != userEmail;
            if (TryDecryptKey(key, [reEncryptionPassphrase], out UserKeyPair? userKeyPair))
            {
                decryptedKeyPairs.Add(userKeyPair!);
                statusChanged |= key.Status != PrivateKeyStatus.PassphraseKnown;
                continue;
            }

            if (TryDecryptKey(key, passphrases, out userKeyPair))
            {
                decryptedKeyPairs.Add(userKeyPair!);
                statusChanged = true;
                continue;
            }

            nonDecryptableAccountKeys.Add(key);
            statusChanged |= key.Status != PrivateKeyStatus.PassphraseUnknown;
        }

        decryptedKeyPairs = [.. decryptedKeyPairs.OrderByDescending(k => k.Timestamp)];
        if (!statusChanged)
        {
            return userAccounts;
        }

        // This turned out to be the easiest way to avoid writing these fields to the JSON file.
        // Modifying the serialization to exclude empty strings was non-trivial, because of constraints
        // when using compile-time source generation for trimmer friendly serialization.
        UserAccount reEncryptedAccount = new(userEmail) { Tag = null!, Signature = null! };
        foreach (UserKeyPair keyPair in decryptedKeyPairs)
        {
            reEncryptedAccount.AccountKeys.Add(keyPair.ToAccountKey(reEncryptionPassphrase));
        }

        foreach (AccountKey key in nonDecryptableAccountKeys)
        {
            key.Status = PrivateKeyStatus.PassphraseUnknown;
            key.User = userEmail;
            reEncryptedAccount.AccountKeys.Add(key);
        }

        return new UserAccounts { Accounts = [reEncryptedAccount] };
    }

    private static bool TryDecryptKey(AccountKey key, List<Passphrase> passphrases, out UserKeyPair? userKeyPair)
    {
        userKeyPair = null;
        for (int i = 0; i < passphrases.Count; i++)
        {
            userKeyPair = key.ToUserKeyPair(passphrases[i]);
            if (userKeyPair == null)
            {
                continue;
            }

            if (i > 0)
            {
                passphrases.Insert(0, passphrases[i]);
                passphrases.RemoveAt(i + 1);
            }

            return true;
        }

        return false;
    }
}
