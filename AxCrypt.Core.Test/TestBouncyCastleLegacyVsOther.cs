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

using System.Reflection;
using System.Security.Cryptography;
using System.Text;

using AxCrypt.Abstractions;
using AxCrypt.Api.Model;
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

using Xecrets.Net.Api.Implementation;
using Xecrets.Net.Core.Crypto.Asymmetric;
using Xecrets.Net.Core.Test.LegacyImplementation;
using Xecrets.Net.Core.Test.Properties;

using static AxCrypt.Abstractions.TypeResolve;

namespace Xecrets.Net.Core.Test;

[TestFixture(CryptoImplementation.Mono)]
[TestFixture(CryptoImplementation.WindowsDesktop)]
[TestFixture(CryptoImplementation.BouncyCastle)]
internal class TestBouncyCastleLegacyVsOther(CryptoImplementation cryptoImplementation)
{
    private SystemTextJsonStringSerializer _bcSerializer;

    private IStringSerializer _otherSerializer;

    [SetUp]
    public void SetUp()
    {
        SetUp(cryptoImplementation);
    }

    private void SetUp(CryptoImplementation cryptoImplementation)
    {
        SetupAssembly.AssemblySetup(cryptoImplementation);
        switch (cryptoImplementation)
        {
            case CryptoImplementation.Mono:
            case CryptoImplementation.WindowsDesktop:
                TypeMap.Register.Singleton<IPaddingHashFactory>(() => new BouncyCastlePaddingHashFactory());
                break;

            case CryptoImplementation.BouncyCastle:
            default:
                break;
        }

        _bcSerializer = new SystemTextJsonStringSerializer(JsonSourceGenerationContext.CreateJsonSerializerContext(new BouncyCastleAsymmetricFactory().GetConverters()));
        _otherSerializer = New<IStringSerializer>();
    }

    [Test]
    public void TestBcEncryptOtherDecrypt()
    {
        const string Secret = "A bouncing secret.";
        IAsymmetricKeyPair bcKeyPair = new BouncyCastleKeyPair(1024);
        byte[] bcEncrypted = bcKeyPair.PublicKey.Transform(Encoding.UTF8.GetBytes(Secret));

        IAsymmetricPrivateKey otherPrivateKey = New<IAsymmetricFactory>().CreatePrivateKey(bcKeyPair.PrivateKey.ToString());
        byte[] otherDecrypted = otherPrivateKey.Transform(bcEncrypted);
        string netSecret = Encoding.UTF8.GetString(otherDecrypted);

        Assert.That(Secret, Is.EqualTo(netSecret));
    }

    [Test]
    public void TestNetEncryptOtherDecrypt()
    {
        const string Secret = "A quick brown fox jumps over the lazy dog.";

        IAsymmetricKeyPair netKeyPair = new NetKeyPair(2048);
        byte[] netEncrypted = netKeyPair.PublicKey.Transform(Encoding.UTF8.GetBytes(Secret));

        IAsymmetricPrivateKey otherPrivateKey = New<IAsymmetricFactory>().CreatePrivateKey(netKeyPair.PrivateKey.ToString());
        byte[] otherDecrypted = otherPrivateKey.Transform(netEncrypted);
        string netSecret = Encoding.UTF8.GetString(otherDecrypted);

        Assert.That(Secret, Is.EqualTo(netSecret));
    }

    [Test]
    public void TestBcEncryptRawOtherDecryptRaw()
    {
        const string Secret = "A bouncing secret.";
        byte[] secretBytes = Encoding.UTF8.GetBytes(Secret);
        IAsymmetricKeyPair bcKeyPair = new BouncyCastleKeyPair(1024);
        byte[] bcEncrypted = bcKeyPair.PublicKey.TransformRaw(secretBytes, 1024 / 8);

        IAsymmetricPrivateKey otherPrivateKey = New<IAsymmetricFactory>().CreatePrivateKey(bcKeyPair.PrivateKey.ToString());
        byte[] otherDecrypted = otherPrivateKey.TransformRaw(bcEncrypted);
        byte[] otherSecret = new byte[Secret.Length];
        Array.Copy(otherDecrypted, 0, otherSecret, 0, otherSecret.Length);
        string otherSecretString = Encoding.UTF8.GetString(otherSecret);

        Assert.That(otherSecretString, Is.EqualTo(Secret));
    }

    [Test]
    public void TestOtherEncryptRawBcDecryptRaw()
    {
        const string Secret = "A bouncing secret.";
        byte[] secretBytes = Encoding.UTF8.GetBytes(Secret);
        IAsymmetricKeyPair otherKeyPair = New<IAsymmetricFactory>().CreateKeyPair(1024);
        byte[] otherEncrypted = otherKeyPair.PublicKey.TransformRaw(secretBytes, 1024 / 8);

        BouncyCastlePrivateKey bcPrivateKey = new(otherKeyPair.PrivateKey.ToString());
        byte[] otherDecrypted = bcPrivateKey.TransformRaw(otherEncrypted);
        byte[] otherSecret = new byte[Secret.Length];
        Array.Copy(otherDecrypted, 0, otherSecret, 0, otherSecret.Length);
        string otherSecretString = Encoding.UTF8.GetString(otherSecret);

        Assert.That(otherSecretString, Is.EqualTo(Secret));
    }

    [Test]
    public void TestNetDecryptOtherEncrypt()
    {
        const string somethingToHash = "Now is the time for all men to come to the aid of their country.";
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(somethingToHash));

        IAsymmetricKeyPair netKeyPair = new NetKeyPair(1024);

        byte[] netDecrypted = netKeyPair.PrivateKey.TransformRaw(hash);

        IAsymmetricPublicKey otherPublicKey = New<IAsymmetricFactory>().CreatePublicKey(netKeyPair.PublicKey.ToString());
        byte[] otherEncrypted = otherPublicKey.TransformRaw(netDecrypted, hash.Length);

        Assert.That(hash.SequenceEqual(otherEncrypted));
    }

    [Test]
    public void TestBcSerializeOtherDeserialize()
    {
        IAsymmetricKeyPair bcKeyPair = new BouncyCastleKeyPair(1024);
        string bcSerialized = _bcSerializer.Serialize(bcKeyPair);

        IAsymmetricKeyPair otherKeyPair = _otherSerializer.Deserialize<IAsymmetricKeyPair>(bcSerialized);
        string otherSerialized = _otherSerializer.Serialize(otherKeyPair);

        Assert.That(otherSerialized, Is.EqualTo(bcSerialized));
    }

    [Test]
    public void TestOtherSerializeBcDeserialize()
    {
        IAsymmetricKeyPair otherKeyPair = New<IAsymmetricFactory>().CreateKeyPair(1024);
        string otherSerialized = _otherSerializer.Serialize(otherKeyPair);

        IAsymmetricKeyPair bcKeyPair = _bcSerializer.Deserialize<IAsymmetricKeyPair>(otherSerialized);
        string bcSerialized = _bcSerializer.Serialize(bcKeyPair);

        Assert.That(bcSerialized, Is.EqualTo(otherSerialized));
    }

    [Test]
    public void TestBcTagOtherTag()
    {
        IAsymmetricKeyPair bcKeyPair = new BouncyCastleKeyPair(1024);
        IAsymmetricKeyPair otherKeyPair = _otherSerializer.Deserialize<IAsymmetricKeyPair>(_bcSerializer.Serialize(bcKeyPair));

        Assert.That(otherKeyPair.PublicKey.Tag, Is.EqualTo(bcKeyPair.PublicKey.Tag));
    }

    [Test]
    public void TestOtherTagBcTag()
    {
        IAsymmetricKeyPair otherKeyPair = New<IAsymmetricFactory>().CreateKeyPair(1024);
        IAsymmetricKeyPair bcKeyPair = _bcSerializer.Deserialize<IAsymmetricKeyPair>(_otherSerializer.Serialize(otherKeyPair));

        Assert.That(bcKeyPair.PublicKey.Tag, Is.EqualTo(otherKeyPair.PublicKey.Tag));
    }

    [Test]
    public void TestBcEncryptFileOtherDecrypt()
    {
        SetUp(CryptoImplementation.BouncyCastle);

        MemoryStream sourcePlaintextStream = new(Encoding.UTF8.GetBytes(Resources.david_copperfield));
        MemoryStream encryptedStream = new();

        EncryptedProperties encryptedProperties = new EncryptedProperties("DavidCopperfield.txt");
        EmailAddress emailAddress = EmailAddress.Parse("xecrets@axantum.com");
        IAsymmetricKeyPair keyPair = New<IAsymmetricFactory>().CreateKeyPair(1024);
        string keyPairJson = New<IStringSerializer>().Serialize(keyPair);
        UserKeyPair userKeyPair = new UserKeyPair(emailAddress, New<INow>().Utc, keyPair);
        Passphrase passphrase = new Passphrase("A Very Secret Password");
        LogOnIdentity logOnIdentity = new LogOnIdentity([userKeyPair], passphrase);
        EncryptionParameters encryptionParameters = new EncryptionParameters(Resolve.CryptoFactory.Preferred.CryptoId, logOnIdentity);

        AxCryptFile.Encrypt(sourcePlaintextStream, encryptedStream, encryptedProperties, encryptionParameters, AxCryptOptions.EncryptWithCompression, new ProgressContext());

        SetUp(cryptoImplementation);

        keyPair = New<IStringSerializer>().Deserialize<IAsymmetricKeyPair>(keyPairJson);
        userKeyPair = new UserKeyPair(emailAddress, New<INow>().Utc, keyPair);
        logOnIdentity = new LogOnIdentity([userKeyPair], passphrase);

        MemoryStream decryptedStream = new();
        encryptedStream = new MemoryStream(encryptedStream.ToArray());

        AxCryptFile.Decrypt(encryptedStream, decryptedStream, logOnIdentity, "Testing...", new ProgressContext());

        string decryptedPlaintext = Encoding.UTF8.GetString(decryptedStream.ToArray());

        Assert.That(decryptedPlaintext, Is.EqualTo(Resources.david_copperfield));

        decryptedStream = new();
        encryptedStream = new MemoryStream(encryptedStream.ToArray());
        logOnIdentity = new LogOnIdentity([userKeyPair], new Passphrase("The wrong password! Use the private key..."));

        AxCryptFile.Decrypt(encryptedStream, decryptedStream, logOnIdentity, "Testing...", new ProgressContext());

        decryptedPlaintext = Encoding.UTF8.GetString(decryptedStream.ToArray());

        Assert.That(decryptedPlaintext, Is.EqualTo(Resources.david_copperfield));
    }

    [Test]
    public void TestOtherEncryptFileBcDecrypt()
    {
        SetUp(cryptoImplementation);

        MemoryStream sourcePlaintextStream = new(Encoding.UTF8.GetBytes(Resources.david_copperfield));
        MemoryStream encryptedStream = new();

        EncryptedProperties encryptedProperties = new EncryptedProperties("DavidCopperfield.txt");
        EmailAddress emailAddress = EmailAddress.Parse("xecrets@axantum.com");
        IAsymmetricKeyPair keyPair = New<IAsymmetricFactory>().CreateKeyPair(1024);
        string keyPairJson = New<IStringSerializer>().Serialize(keyPair);
        UserKeyPair userKeyPair = new UserKeyPair(emailAddress, New<INow>().Utc, keyPair);
        Passphrase passphrase = new Passphrase("A Very Secret Password");
        LogOnIdentity logOnIdentity = new LogOnIdentity([userKeyPair], passphrase);
        EncryptionParameters encryptionParameters = new EncryptionParameters(Resolve.CryptoFactory.Preferred.CryptoId, logOnIdentity);

        AxCryptFile.Encrypt(sourcePlaintextStream, encryptedStream, encryptedProperties, encryptionParameters, AxCryptOptions.EncryptWithCompression, new ProgressContext());

        SetUp(CryptoImplementation.BouncyCastle);

        keyPair = New<IStringSerializer>().Deserialize<IAsymmetricKeyPair>(keyPairJson);
        userKeyPair = new UserKeyPair(emailAddress, New<INow>().Utc, keyPair);
        logOnIdentity = new LogOnIdentity([userKeyPair], passphrase);

        MemoryStream decryptedStream = new();
        encryptedStream = new MemoryStream(encryptedStream.ToArray());

        AxCryptFile.Decrypt(encryptedStream, decryptedStream, logOnIdentity, "Testing...", new ProgressContext());

        string decryptedPlaintext = Encoding.UTF8.GetString(decryptedStream.ToArray());

        Assert.That(decryptedPlaintext, Is.EqualTo(Resources.david_copperfield));

        decryptedStream = new();
        encryptedStream = new MemoryStream(encryptedStream.ToArray());
        logOnIdentity = new LogOnIdentity([userKeyPair], new Passphrase("The wrong password! Use the private key..."));

        AxCryptFile.Decrypt(encryptedStream, decryptedStream, logOnIdentity, "Testing...", new ProgressContext());

        decryptedPlaintext = Encoding.UTF8.GetString(decryptedStream.ToArray());

        Assert.That(decryptedPlaintext, Is.EqualTo(Resources.david_copperfield));
    }

    [Test]
    public void TestBcSignOtherVerify()
    {
        SetUp(CryptoImplementation.BouncyCastle);

        IAsymmetricKeyPair keyPair = New<IAsymmetricFactory>().CreateKeyPair(1024);

        byte[] signature = new Signer(keyPair.PrivateKey)
            .Sign("The quick brown fox jumps over the lazy dog", "someone@somewhere.net");

        SetUp(cryptoImplementation);

        Verifier verifier = new Verifier(New<IAsymmetricFactory>().CreatePublicKey(keyPair.PublicKey.ToString()));
        bool verified = verifier.Verify(signature, "The quick brown fox jumps over the lazy dog", "someone@somewhere.net");

        Assert.That(verified, Is.True);
    }

    [Test]
    public void TestOtherSignBcVerify()
    {
        SetUp(cryptoImplementation);

        IAsymmetricKeyPair keyPair = New<IAsymmetricFactory>().CreateKeyPair(1024);

        byte[] signature = new Signer(keyPair.PrivateKey)
            .Sign("The quick brown fox jumps over the lazy dog", "someone@somewhere.net");

        SetUp(CryptoImplementation.BouncyCastle);

        Verifier verifier = new Verifier(New<IAsymmetricFactory>().CreatePublicKey(keyPair.PublicKey.ToString()));
        bool verified = verifier.Verify(signature, "The quick brown fox jumps over the lazy dog", "someone@somewhere.net");

        Assert.That(verified, Is.True);
    }

    [Test]
    public void TestAxCryptEncryptedFile()
    {
        string accountsJson = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Xecrets.Net.Core.Test.resources.UserAccounts.txt")).ReadToEnd();

        UserAccounts userAccounts = New<IStringSerializer>().Deserialize<UserAccounts>(accountsJson);

        Passphrase passphrase = new("Xecrets Ez is Great!");
        UserKeyPair userKeyPair = userAccounts.Accounts[0].AccountKeys[0].ToUserKeyPair(passphrase);
        LogOnIdentity logOnIdentity = new([userKeyPair], passphrase);

        MemoryStream decryptedStream = new();
        MemoryStream encryptedStream = new();
        Assembly.GetExecutingAssembly().GetManifestResourceStream("Xecrets.Net.Core.Test.resources.david-copperfield-txt.axx").CopyTo(encryptedStream);
        encryptedStream.Position = 0;

        AxCryptFile.Decrypt(encryptedStream, decryptedStream, logOnIdentity, "Testing...", new ProgressContext());

        string decryptedPlaintext = Encoding.UTF8.GetString(decryptedStream.ToArray()).ReplaceLineEndings();

        Assert.That(decryptedPlaintext, Is.EqualTo(Resources.david_copperfield));

        decryptedStream = new();
        encryptedStream = new();
        Assembly.GetExecutingAssembly().GetManifestResourceStream("Xecrets.Net.Core.Test.resources.david-copperfield-txt.axx").CopyTo(encryptedStream);
        encryptedStream.Position = 0;
        logOnIdentity = new LogOnIdentity([userKeyPair], new Passphrase("The wrong password! Use the private key..."));

        AxCryptFile.Decrypt(encryptedStream, decryptedStream, logOnIdentity, "Testing...", new ProgressContext());

        decryptedPlaintext = Encoding.UTF8.GetString(decryptedStream.ToArray()).ReplaceLineEndings();

        Assert.That(decryptedPlaintext, Is.EqualTo(Resources.david_copperfield));
    }
}
