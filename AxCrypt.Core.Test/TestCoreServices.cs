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
 * You should have received a copy of the GNU General Public License along with Xecrets.Net.  If not, see
 * <https://www.gnu.org/licenses/>.
 *
 * The source repository can be found at https://github.com/axantum/xecrets-net please go there for more information,
 * suggestions and contributions. You may also visit https://www.axantum.com for more information about the author.
 */

#endregion Coypright and GPL License

using AxCrypt.Core.Header;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

using Xecrets.Core.Abstractions;
using Xecrets.Core.Models;
using Xecrets.Core.Public;

// ReSharper disable once CheckNamespace
namespace AxCrypt.Core.Test;

[TestFixture]
public sealed class TestCoreServices
{
    private static readonly ICoreServices CoreServices = CreateCoreServices();

    [Test]
    public async Task IsEncryptedAsyncRecognizesFormatGuidWithPartialReads()
    {
        TrackingStream stream = new([.. AxCrypt1Guid.GetBytes(), 0], maximumReadSize: 3);
        int openCount = 0;

        bool isEncrypted = await CoreServices.IsEncryptedAsync(() =>
        {
            openCount++;
            return Task.FromResult<Stream>(stream);
        });

        Assert.That(isEncrypted, Is.True);
        Assert.That(openCount, Is.EqualTo(1));
        Assert.That(stream.IsDisposed, Is.True);
        Assert.That(stream.MaximumRequestedReadSize, Is.LessThanOrEqualTo(AxCrypt1Guid.Length));
        Assert.That(stream.TotalBytesRead, Is.EqualTo(AxCrypt1Guid.Length));
    }

    [Test]
    public async Task IsEncryptedAsyncReturnsFalseForShortOrDifferentData()
    {
        TrackingStream shortStream = new(AxCrypt1Guid.GetBytes()[..^1]);
        TrackingStream differentStream = new(new byte[AxCrypt1Guid.Length]);

        Assert.That(await CoreServices.IsEncryptedAsync(() => Task.FromResult<Stream>(shortStream)), Is.False);
        Assert.That(await CoreServices.IsEncryptedAsync(() => Task.FromResult<Stream>(differentStream)), Is.False);
        Assert.That(shortStream.IsDisposed, Is.True);
        Assert.That(differentStream.IsDisposed, Is.True);
    }

    [Test]
    public void IsEncryptedAsyncPropagatesOpenFailure()
    {
        IOException exception = new("Cannot open stream.");

        Assert.ThrowsAsync<IOException>(async () => await CoreServices.IsEncryptedAsync(
            () => Task.FromException<Stream>(exception)));
    }

    [Test]
    public void IsEncryptedAsyncPropagatesReadFailure()
    {
        Assert.ThrowsAsync<IOException>(async () => await CoreServices.IsEncryptedAsync(
            () => Task.FromResult<Stream>(new ThrowingReadStream())));
    }

    [Test]
    public async Task ChangeKeyPairAsyncChangesEmailAndPassphraseAndKeepsKeys()
    {
        DateTimeOffset createdUtc = new(2026, 1, 2, 3, 4, 5, TimeSpan.Zero);
        KeyPair original = await CoreServices.CreateKeyPairAsync("old@example.com", "old", createdUtc);

        KeyPair changed = await CoreServices.ChangeKeyPairAsync(original, "new@example.com", "new");

        Assert.That(CoreServices.TryLoadKeyPair(changed.EncryptedBytes, ["old"], out _), Is.False);
        Assert.That(CoreServices.TryLoadKeyPair(changed.EncryptedBytes, ["new"], out LoadedKeyPair? loaded), Is.True);
        Assert.That(loaded!.KeyPair.Email, Is.EqualTo("new@example.com"));
        Assert.That(loaded.KeyPair.PublicKey.Email, Is.EqualTo("new@example.com"));
        Assert.That(loaded.KeyPair.PublicKey.Tag, Is.EqualTo(original.PublicKey.Tag));
        Assert.That(loaded.KeyPair.CreatedUtc, Is.EqualTo(original.CreatedUtc));
    }

    [Test]
    public async Task ChangeKeyPairAsyncRejectsInvalidEmail()
    {
        KeyPair original = await CoreServices.CreateKeyPairAsync("old@example.com", "old", DateTimeOffset.UtcNow);

        await Assert.ThrowsAsync<XecretsCoreException>(async () =>
            await CoreServices.ChangeKeyPairAsync(original, "not an email", "new"));
    }

    [Test]
    public async Task ReEncryptWithKeepsRecipientsOfAFileAndAddsNewRecipientsOnce()
    {
        DateTimeOffset createdUtc = new(2026, 1, 2, 3, 4, 5, TimeSpan.Zero);
        KeyPair recipient = await CoreServices.CreateKeyPairAsync("recipient@example.com", "recipient", createdUtc);
        KeyPair added = await CoreServices.CreateKeyPairAsync("added@example.com", "added", createdUtc);
        EncryptedWithParameters encryptedWith = await EncryptAndReadBackAsync(
            new EncryptedWithParameters("old", [recipient.PublicKey], []));

        EncryptedWithParameters reEncryptWith = encryptedWith.ReEncryptWith(
            "new",
            [recipient.PublicKey, added.PublicKey]);

        Assert.That(reEncryptWith.Passphrase, Is.EqualTo("new"));
        Assert.That(
            reEncryptWith.Recipients.Select(publicKey => publicKey.SerializedKey),
            Is.EqualTo([recipient.PublicKey.SerializedKey, added.PublicKey.SerializedKey]));
    }

    [Test]
    public void ReEncryptWithKeepsMasterKeys()
    {
        PublicKey masterKey = new("", "master key");
        EncryptedWithParameters encryptedWith = new("old", [], [masterKey]);

        EncryptedWithParameters reEncryptWith = encryptedWith.ReEncryptWith("new", []);

        Assert.That(reEncryptWith.MasterKeys, Is.EqualTo([masterKey]));
    }

    private static async Task<EncryptedWithParameters> EncryptAndReadBackAsync(EncryptedWithParameters parameters)
    {
        DateTime utcNow = DateTime.UtcNow;
        using MemoryStream encrypted = new();
        await CoreServices.EncryptAsync(
            new MemoryStream([1, 2, 3]),
            encrypted,
            new EncryptRequest(
                parameters.Passphrase,
                parameters.Recipients,
                parameters.MasterKeys,
                "a.txt",
                utcNow,
                utcNow,
                utcNow,
                true,
                new Progress<Progress>(_ => { })));
        encrypted.Position = 0;

        using IDecryptionSession session = await CoreServices.OpenDecryptionAsync(
            encrypted,
            new DecryptRequest([new Identity(parameters.Passphrase, [])], new Progress<Progress>(_ => { })));
        Assert.That(session.IsDecryptable, Is.True);
        return session.EncryptedWithParameters;
    }

    private static ICoreServices CreateCoreServices()
    {
        ServiceCollection services = new();
        services.AddXecretsCore();
        return services.BuildServiceProvider().GetRequiredService<ICoreServices>();
    }

    private sealed class TrackingStream(byte[] buffer, int maximumReadSize = int.MaxValue) : MemoryStream(buffer, writable: false)
    {
        public bool IsDisposed { get; private set; }

        public int MaximumRequestedReadSize { get; private set; }

        public int TotalBytesRead { get; private set; }

        public override async ValueTask<int> ReadAsync(Memory<byte> destination, CancellationToken cancellationToken = default)
        {
            MaximumRequestedReadSize = Math.Max(MaximumRequestedReadSize, destination.Length);
            int bytesRead = await base.ReadAsync(destination[..Math.Min(destination.Length, maximumReadSize)], cancellationToken);
            TotalBytesRead += bytesRead;
            return bytesRead;
        }

        protected override void Dispose(bool disposing)
        {
            IsDisposed = true;
            base.Dispose(disposing);
        }
    }

    private sealed class ThrowingReadStream : MemoryStream
    {
        public override ValueTask<int> ReadAsync(Memory<byte> destination, CancellationToken cancellationToken = default)
            => ValueTask.FromException<int>(new IOException("Cannot read stream."));
    }
}
