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
