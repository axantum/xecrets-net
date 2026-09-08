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

namespace Xecrets.Core.Abstractions;

/// <summary>
/// The core interface for encryption and decryption operations, key management, and related functionality.
/// </summary>
/// <remarks>
/// Cancellation should be handled by IProgressContext implementations, throwing OperationCanceledException.
/// The IProgressContext instance is provided in the EncryptRequest or DecryptRequest.
/// </remarks>
public interface ICoreServices
{
    Task<bool> IsEncryptedAsync(Func<Task<Stream>> openReadAsync);

    Task EncryptAsync(Stream cleartext, Stream encrypted, EncryptRequest request);

    Task<IDecryptionSession> OpenDecryptionAsync(Stream encrypted, DecryptRequest request);

    Task<KeyPair> CreateKeyPairAsync(string email, string passphrase, DateTimeOffset createdUtc);

    bool TryLoadKeyPair(ReadOnlyMemory<byte> encryptedKeyPair, IReadOnlyList<string> passphrases, [NotNullWhen(true)] out LoadedKeyPair? loadedKeyPair);

    string ExportPublicKey(PublicKey publicKey);

    PublicKey? ImportPublicKey(string serializedPublicKey);

    PrivateKeyImportResult ImportPrivateKeys(string serializedAccounts, PrivateKeyImportRequest request);

    bool TryParseEmail(string email, [NotNullWhen(true)] out string? address);
}
