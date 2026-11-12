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

namespace Xecrets.Core.Public;

/// <summary>
/// A write-only stream that keeps the data in memory as long as it is classified as one of the requested categories
/// and does not exceed the size limit for that category. Otherwise all data, including what is already buffered, goes
/// to the file.
/// </summary>
/// <param name="originalFileName">The name of the file the data is from, used to classify it.</param>
/// <param name="inMemory">The size limit for each category to keep in memory. <see cref="FileCategory.Other"/> accepts
/// data of any category not otherwise requested.</param>
/// <param name="openFile">Opens the file for writing. Called at most once, and only if the data goes to the
/// file.</param>
public sealed class InMemoryOrFileStream(string originalFileName, IReadOnlyDictionary<FileCategory, long> inMemory, Func<Stream> openFile) : Stream
{
    private const int SampleSize = 4096;

    private readonly MemoryStream _bufferStream = new();

    private Stream? _fileStream;

    private bool _isClassified = inMemory.Count == 0;

    private FileCategory? _requested;

    /// <inheritdoc/>
    public override bool CanRead => false;

    /// <inheritdoc/>
    public override bool CanSeek => false;

    /// <inheritdoc/>
    public override bool CanWrite => true;

    /// <inheritdoc/>
    public override long Length => throw new NotSupportedException();

    /// <inheritdoc/>
    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    /// <summary>
    /// The category the data is classified as, wherever it was written. It is <see cref="FileCategory.Other"/> when
    /// no categories are requested, since the data is then not classified. Final after <see cref="Commit"/>.
    /// </summary>
    public FileCategory Category { get; private set; }

    /// <summary>
    /// The requested category the data is kept in memory as, or null if it was written to the file. Set by <see
    /// cref="Commit"/>.
    /// </summary>
    public FileCategory? InMemoryCategory { get; private set; }

    /// <summary>
    /// The data kept in memory, as it was written. Empty if it was written to the file. Set by <see cref="Commit"/>,
    /// and valid also after the stream is disposed.
    /// </summary>
    public ReadOnlyMemory<byte> Data { get; private set; }

    /// <summary>
    /// Commit the data after all of it has been written successfully, classifying it if that has not already been done,
    /// and writing it to the file if it is not to be kept in memory. The file is created even if there is no data. Text
    /// kept in memory is verified to be text in full, and is otherwise not classified as text.
    /// </summary>
    public void Commit()
    {
        Classify();
        VerifyText();

        if (_fileStream == null)
        {
            StartFileWriteIfNotMemory();
        }
        if (_fileStream != null)
        {
            return;
        }

        InMemoryCategory = _requested;
        Data = _bufferStream.GetBuffer().AsMemory(0, (int)_bufferStream.Length);
    }

    /// <inheritdoc/>
    public override void Write(byte[] buffer, int offset, int count)
    {
        Write(buffer.AsSpan(offset, count));
    }

    /// <inheritdoc/>
    public override void Write(ReadOnlySpan<byte> buffer)
    {
        if (_fileStream != null)
        {
            _fileStream.Write(buffer);
            return;
        }

        _bufferStream.Write(buffer);

        if (_bufferStream.Length >= SampleSize)
        {
            Classify();
        }

        if (_isClassified)
        {
            StartFileWriteIfNotMemory();
        }
    }

    /// <inheritdoc/>
    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        return WriteAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();
    }

    /// <inheritdoc/>
    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        if (_fileStream != null)
        {
            return _fileStream.WriteAsync(buffer, cancellationToken);
        }

        Write(buffer.Span);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc/>
    public override void Flush()
    {
        _fileStream?.Flush();
    }

    /// <inheritdoc/>
    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    /// <inheritdoc/>
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    /// <inheritdoc/>
    public override void SetLength(long value) => throw new NotSupportedException();

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _fileStream?.Dispose();
        }
        base.Dispose(disposing);
    }

    private ReadOnlySpan<byte> Buffer => _bufferStream.GetBuffer().AsSpan(0, (int)_bufferStream.Length);

    private void Classify()
    {
        if (_isClassified)
        {
            return;
        }

        _isClassified = true;

        SetCategory(FileTypeClassifier.Classify(originalFileName, Buffer));
    }

    /// <summary>
    /// The sample only tells how the data starts, so text that is all in memory is checked in full.
    /// </summary>
    private void VerifyText()
    {
        if (_fileStream != null || Category != FileCategory.Text || FileTypeClassifier.LooksLikeText(Buffer))
        {
            return;
        }

        SetCategory(FileCategory.Other);
    }

    private void SetCategory(FileCategory category)
    {
        Category = category;
        _requested = inMemory.ContainsKey(category)
            ? category
            : inMemory.ContainsKey(FileCategory.Other) ? FileCategory.Other : null;
    }

    private void StartFileWriteIfNotMemory()
    {
        if (_requested != null && _bufferStream.Length <= inMemory[_requested.Value])
        {
            return;
        }

        _fileStream = openFile();
        _fileStream.Write(Buffer);
    }
}
