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

namespace Xecrets.Core.Implementation;

internal sealed class DecryptionSession(IAxCryptDocument document)
    : IDecryptionSession
{
    // The IAxCryptDocument property PassphraseIsValid is misnamed,
    // it indicates whether the document has been successfully decrypted with the passphrase or a private key.
    public bool IsDecryptable => document.PassphraseIsValid;

    public string OriginalFileName => IsDecryptable ? document.FileName : string.Empty;

    public DateTime CreationTimeUtc => document.CreationTimeUtc;

    public DateTime LastAccessTimeUtc => document.LastAccessTimeUtc;

    public DateTime LastWriteTimeUtc => document.LastWriteTimeUtc;

    public EncryptedWithParameters EncryptedWithParameters =>
        IsDecryptable ? document.ExtractEncryptionCredentials() : EncryptedWithParameters.Empty;

    public Task DecryptAsync(Stream cleartext)
        => Task.Run(() =>
        {
            try
            {
                if (IsDecryptable)
                {
                    using Stream forwardOnlyClearText = ForwardOnlyStream.Wrap(cleartext);
                    document.DecryptTo(forwardOnlyClearText);
                }
            }
            catch (AxCryptException ex)
            {
                throw ex.ToXecretsCoreException();
            }
        });

    public void Dispose()
    {
        document.Dispose();
    }
}
