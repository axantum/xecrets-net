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

internal sealed class ProgressStream : Stream
{
    private readonly Stream _wrapped;

    private readonly IProgress<Progress> _progress;

    private bool _isDisposed;

    private ProgressStream(Stream wrapped, IProgress<Progress> progress)
    {
        _wrapped = wrapped;
        _progress = progress;

        _progress.Report(Progress.LevelStarted());
    }

    public static Stream Wrap(Stream stream, IProgress<Progress> progress) => new ProgressStream(stream, progress);

    public override bool CanRead => _wrapped.CanRead;

    public override bool CanSeek => _wrapped.CanSeek;

    public override bool CanWrite => _wrapped.CanWrite;

    public override long Length => _wrapped.Length;

    public override long Position { get => _wrapped.Position; set => _wrapped.Position = value; }

    public override void Flush()
    {
        _wrapped.Flush();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        int bytes = count > 0 ? _wrapped.Read(buffer, offset, count) : 0;
        _progress.Report(Progress.CountAdded(bytes));
        return bytes;
    }

    public override int Read(Span<byte> buffer)
    {
        int bytes = buffer.Length > 0 ? _wrapped.Read(buffer) : 0;
        _progress.Report(Progress.CountAdded(bytes));
        return bytes;
    }

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        int bytes = buffer.Length > 0 ? await _wrapped.ReadAsync(buffer, cancellationToken) : 0;
        _progress.Report(Progress.CountAdded(bytes));
        return bytes;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        return _wrapped.Seek(offset, origin);
    }

    public override void SetLength(long value)
    {
        _wrapped.SetLength(value);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        _wrapped.Write(buffer, offset, count);
        _progress.Report(Progress.CountAdded(count));
    }

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        _wrapped.Write(buffer);
        _progress.Report(Progress.CountAdded(buffer.Length));
    }

    public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        await _wrapped.WriteAsync(buffer, cancellationToken);
        _progress.Report(Progress.CountAdded(buffer.Length));
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && !_isDisposed)
        {
            _isDisposed = true;
            _progress.Report(Progress.LevelFinished());
        }
        base.Dispose(disposing);
        if (disposing)
        {
            _wrapped.Dispose();
        }
    }

    public override async ValueTask DisposeAsync()
    {
        if (!_isDisposed)
        {
            _isDisposed = true;
            _progress.Report(Progress.LevelFinished());
        }
        await _wrapped.DisposeAsync();
        await base.DisposeAsync();
    }
}
