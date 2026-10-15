#region Xecrets Cli Copyright and GPL License notice

/*
 * Xecrets Cli - Changes and additions Copyright © 2022-2025, Svante Seleborg, All Rights Reserved.
 *
 * This code file is part of Xecrets Cli, but is derived from AxCrypt as licensed under GPL v3 or later.
 *
 * The changes and additions are separately copyrighted and only licensed under GPL v3 or later as detailed below,
 * unless explicitly licensed otherwise. If you use any part of these changes and additions in your software,
 * please see https://www.gnu.org/licenses/ for details of what this means for you.
 *
 * Warning: If you are using the original AxCrypt code under a non-GPL v3 or later license, these changes and additions
 * are not included in that license. If you use these changes under those circumstances, all your code becomes subject to
 * the GPL v3 or later license, according to the principle of strong copyleft as applied to GPL v3 or later.
 *
 * Xecrets Cli is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * Xecrets Cli is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied
 * warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License along with Xecrets Cli. If not, see
 * https://www.gnu.org/licenses/.
 *
 * The source repository can be found at https://github.com/axantum/xecrets-net please go there for more information,
 * suggestions and contributions, as well for commit history detailing changes and additions that fall under the strong
 * copyleft provisions mentioned above. You may also visit https://www.axantum.com for more information about the author.
*/

#endregion Xecrets Cli Copyright and GPL License notice

using System.Text;

using AxCrypt.Abstractions;
using AxCrypt.Core;
using AxCrypt.Core.Crypto;
using AxCrypt.Core.Crypto.Asymmetric;
using AxCrypt.Core.Extensions;
using AxCrypt.Core.Service;
using AxCrypt.Core.Session;
using AxCrypt.Core.Test;
using AxCrypt.Core.UI;
using AxCrypt.Fake;

using NUnit.Framework;

using Xecrets.Net.Core.Test.Properties;

using static AxCrypt.Abstractions.TypeResolve;

namespace Xecrets.Net.Core.Test;

[TestFixture(CryptoImplementation.Mono)]
[TestFixture(CryptoImplementation.WindowsDesktop)]
[TestFixture(CryptoImplementation.BouncyCastle)]
internal class TestMultipleMasterKeys(CryptoImplementation cryptoImplementation)
{
    private static readonly EmailAddress _ownerEmail = EmailAddress.Parse("owner@axantum.com");

    private static readonly EmailAddress _masterEmail = EmailAddress.Parse("master@axantum.com");

    private static readonly Passphrase _ownerPassphrase = new("A Very Secret Password");

    private static readonly Passphrase _wrongPassphrase = new("The wrong password! Use the private key...");

    [SetUp]
    public void SetUp()
    {
        SetupAssembly.AssemblySetup(cryptoImplementation);
    }

    [TearDown]
    public void TearDown()
    {
        TypeMap.Register.Clear();
    }

    [Test]
    public async Task TestDecryptWithEitherMasterKey()
    {
        IAsymmetricKeyPair masterKeyPair1 = New<IAsymmetricFactory>().CreateKeyPair(1024);
        IAsymmetricKeyPair masterKeyPair2 = New<IAsymmetricFactory>().CreateKeyPair(1024);

        byte[] encrypted = await EncryptWithMasterKeysAsync([masterKeyPair1.PublicKey, masterKeyPair2.PublicKey]);

        Assert.That(Decrypt(encrypted, new LogOnIdentity(_ownerPassphrase)), Is.EqualTo(Resources.david_copperfield));
        Assert.That(Decrypt(encrypted, MasterIdentity(masterKeyPair1)), Is.EqualTo(Resources.david_copperfield));
        Assert.That(Decrypt(encrypted, MasterIdentity(masterKeyPair2)), Is.EqualTo(Resources.david_copperfield));
    }

    [Test]
    public async Task TestMasterKeysReadBackFromHeaders()
    {
        IAsymmetricKeyPair masterKeyPair1 = New<IAsymmetricFactory>().CreateKeyPair(1024);
        IAsymmetricKeyPair masterKeyPair2 = New<IAsymmetricFactory>().CreateKeyPair(1024);

        byte[] encrypted = await EncryptWithMasterKeysAsync([masterKeyPair1.PublicKey, masterKeyPair2.PublicKey]);

        using IAxCryptDocument document = LoadDocument(encrypted, MasterIdentity(masterKeyPair2));
        Assert.That(document.PassphraseIsValid, Is.True);

        List<IAsymmetricPublicKey> masterKeys = [.. document.AsymmetricMasterKeys];
        Assert.That(masterKeys, Has.Count.EqualTo(2));
        Assert.That(masterKeys[0].ToString(), Is.EqualTo(masterKeyPair1.PublicKey.ToString()));
        Assert.That(masterKeys[1].ToString(), Is.EqualTo(masterKeyPair2.PublicKey.ToString()));
        Assert.That(document.AsymmetricMasterKey?.ToString(), Is.EqualTo(masterKeyPair1.PublicKey.ToString()));
    }

    [Test]
    public void TestSingleMasterKeyUnchanged()
    {
        IAsymmetricKeyPair masterKeyPair = New<IAsymmetricFactory>().CreateKeyPair(1024);

        EncryptionParameters encryptionParameters = new(Resolve.CryptoFactory.Preferred.CryptoId, OwnerIdentity())
        {
            MasterPublicKey = masterKeyPair.PublicKey,
        };
        byte[] encrypted = Encrypt(encryptionParameters);

        using (IAxCryptDocument document = LoadDocument(encrypted, MasterIdentity(masterKeyPair)))
        {
            Assert.That(document.PassphraseIsValid, Is.True);
            Assert.That(document.AsymmetricMasterKey?.ToString(), Is.EqualTo(masterKeyPair.PublicKey.ToString()));
            Assert.That(document.AsymmetricMasterKeys, Is.Empty);
        }

        Assert.That(Decrypt(encrypted, MasterIdentity(masterKeyPair)), Is.EqualTo(Resources.david_copperfield));
    }

    internal static async Task<byte[]> EncryptWithMasterKeysAsync(IEnumerable<IAsymmetricPublicKey> masterPublicKeys)
    {
        EncryptionParameters encryptionParameters = new(Resolve.CryptoFactory.Preferred.CryptoId, OwnerIdentity());
        await encryptionParameters.AddMasterPublicKeyAsync(masterPublicKeys);

        return Encrypt(encryptionParameters);
    }

    internal static LogOnIdentity MasterIdentity(IAsymmetricKeyPair masterKeyPair)
    {
        return new LogOnIdentity([new UserKeyPair(_masterEmail, New<INow>().Utc, masterKeyPair)], _wrongPassphrase);
    }

    internal static IAxCryptDocument LoadDocument(byte[] encrypted, LogOnIdentity identity)
    {
        return New<AxCryptFactory>().CreateDocument(identity.DecryptionParameters(), new MemoryStream(encrypted));
    }

    internal static string Decrypt(byte[] encrypted, LogOnIdentity identity)
    {
        MemoryStream decryptedStream = new();
        AxCryptFile.Decrypt(new MemoryStream(encrypted), decryptedStream, identity, "Testing...", new ProgressContext());

        return Encoding.UTF8.GetString(decryptedStream.ToArray());
    }

    private static LogOnIdentity OwnerIdentity()
    {
        IAsymmetricKeyPair ownerKeyPair = New<IAsymmetricFactory>().CreateKeyPair(1024);
        return new LogOnIdentity([new UserKeyPair(_ownerEmail, New<INow>().Utc, ownerKeyPair)], _ownerPassphrase);
    }

    private static byte[] Encrypt(EncryptionParameters encryptionParameters)
    {
        MemoryStream sourcePlaintextStream = new(Encoding.UTF8.GetBytes(Resources.david_copperfield));
        MemoryStream encryptedStream = new();

        AxCryptFile.Encrypt(sourcePlaintextStream, encryptedStream, new EncryptedProperties("DavidCopperfield.txt"), encryptionParameters, AxCryptOptions.EncryptWithCompression, new ProgressContext());

        return encryptedStream.ToArray();
    }
}
