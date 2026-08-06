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

namespace Xecrets.Core.Implementation;

/// <summary>
/// A stream wrapper intended to ensure that streams passed into Xecrets.Core do not require
/// Length or Position capabilities, so they are pure streams. If a consumer accesses Length
/// or Position an exception is thrown.
/// </summary>
/// <param name="wrapped">The stream to wrap.</param>
internal sealed class ForwardOnlyStream(Stream wrapped, bool leaveOpen) : Stream
{
    public override bool CanRead => wrapped.CanRead;

    public override bool CanSeek => false;

    public override bool CanWrite => wrapped.CanWrite;

    public override long Length => throw new NotSupportedException("Length is not supported on forward-only streams.");

    public override long Position
    {
        get => throw new NotSupportedException("Position is not supported on forward-only streams.");
        set => throw new NotSupportedException("Position is not supported on forward-only streams.");
    }

    public static Stream Wrap(Stream stream, bool leaveOpen = false) =>
        stream is ForwardOnlyStream && !leaveOpen ? stream : new ForwardOnlyStream(stream, leaveOpen);

    public override void Flush() => wrapped.Flush();

    public override int Read(byte[] buffer, int offset, int count) => wrapped.Read(buffer, offset, count);

    public override int Read(Span<byte> buffer) => wrapped.Read(buffer);

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) => await wrapped.ReadAsync(buffer, cancellationToken);

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException("Seek is not supported on forward-only streams.");

    public override void SetLength(long value) => wrapped.SetLength(value);

    public override void Write(byte[] buffer, int offset, int count) => wrapped.Write(buffer, offset, count);

    public override void Write(ReadOnlySpan<byte> buffer) => wrapped.Write(buffer);

    public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default) => await wrapped.WriteAsync(buffer, cancellationToken);

    protected override void Dispose(bool disposing)
    {
        if (disposing && !leaveOpen)
        {
            wrapped.Dispose();
        }
        base.Dispose(disposing);
    }

    public override async ValueTask DisposeAsync()
    {
        if (!leaveOpen)
        {
            await wrapped.DisposeAsync();
        }
        await base.DisposeAsync();
    }
}
