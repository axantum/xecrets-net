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
using AxCrypt.Core.IO;
using AxCrypt.Core.Runtime;
using AxCrypt.Mono;

using static AxCrypt.Abstractions.TypeResolve;

namespace Xecrets.Core.Desktop.Implementation;

internal class StandardIoDataStore : IStandardIoDataStore
{
    private const string StdinAlias = "-";

    private const string StdoutAlias = "+";

    private const string StdIoNameSeparator = ":";

    private readonly DataStore _wrapped;

    public bool IsStdin { get; }

    public bool IsStdout { get; }

    public bool IsStdIo => IsStdin || IsStdout;

    public bool IsNamedStdIo => IsStdIo && _aliasName.Length > 0;

    private readonly string _aliasName = string.Empty;

    public string AliasName => _aliasName.Length > 0 ? _aliasName : Name;

    public StandardIoDataStore(string path)
    {
        if (path.Length == 0)
        {
            throw new ArgumentException(@"Path is empty", nameof(path));
        }

        IsStdin = path == StdinAlias || path.StartsWith(StdinAlias + StdIoNameSeparator, StringComparison.InvariantCulture);
        IsStdout = path == StdoutAlias || path.StartsWith(StdoutAlias + StdIoNameSeparator, StringComparison.InvariantCulture);

        string[] nameAndAlias = path.Split(StdinAlias + StdIoNameSeparator);
        if (nameAndAlias.Length != 2)
        {
            nameAndAlias = path.Split(StdoutAlias + StdIoNameSeparator);
        }

        string adjustedPath = path;
        if (nameAndAlias.Length == 2)
        {
            adjustedPath = nameAndAlias[0];
            _aliasName = nameAndAlias[1];
        }

        if (IsStdIo)
        {
            adjustedPath = path[..1];
        }

        ValidatePath(adjustedPath);
        _wrapped = new DataStore(adjustedPath);

        ValidatePath(AliasName);
        return;

        static void ValidatePath(string path)
        {
            if (Path.GetFileName(path).Any(Path.GetInvalidFileNameChars().Contains))
            {
                throw new ArgumentException("{0} is not a valid filename.".Format(Path.GetFileName(path)));
            }

            if (path.Any(Path.GetInvalidPathChars().Contains))
            {
                throw new ArgumentException("{0} is not a valid path.".Format(path));
            }
        }
    }

    public bool IsWriteProtected
    {
        get => IsStdin || !IsStdout && _wrapped.IsWriteProtected;
        set
        {
            if (!IsStdIo)
            {
                _wrapped.IsWriteProtected = value;
            }
        }
    }

    public bool IsEncryptable => IsStdin || _wrapped.IsEncryptable;

    private readonly DateTime _utcNow = New<INow>().Utc;

    public DateTime CreationTimeUtc
    {
        get => IsStdIo ? _utcNow : _wrapped.CreationTimeUtc;
        set
        {
            if (!IsStdIo)
            {
                _wrapped.CreationTimeUtc = value;
            }
        }
    }

    public DateTime LastAccessTimeUtc
    {
        get => IsStdIo ? _utcNow : _wrapped.LastAccessTimeUtc;
        set
        {
            if (!IsStdIo)
            {
                _wrapped.LastAccessTimeUtc = value;
            }
        }
    }

    public DateTime LastWriteTimeUtc
    {
        get => IsStdIo ? _utcNow : _wrapped.LastWriteTimeUtc;
        set
        {
            if (!IsStdIo)
            {
                _wrapped.LastWriteTimeUtc = value;
            }
        }
    }

    public IDataContainer Container => !IsStdIo ? _wrapped.Container : throw new FileOperationException("Cannot get the parent container of a standard IO stream.", _wrapped.Name, ErrorStatus.InvalidPath);

    public bool IsAvailable => IsStdIo || _wrapped.IsAvailable;

    public bool IsFile => true;

    public bool IsFolder => false;

    public string Name => _wrapped.Name;

    public string FullName => _wrapped.FullName;

    public bool IsNetworkPath => !IsStdIo && _wrapped.IsNetworkPath;

    public void Delete()
    {
        if (!IsStdIo)
        {
            _wrapped.Delete();
        }
    }

    public bool IsLocked() => !IsStdIo && _wrapped.IsLocked();

    public long Length() => IsStdIo ? 0 : _wrapped.Length();

    public void MoveTo(string destinationFileName)
    {
        if (IsStdIo)
        {
            throw new FileOperationException("Cannot move a standard IO stream.", _wrapped.Name, ErrorStatus.InvalidPath);
        }
    }

    public Stream OpenRead()
    {
        if (IsStdout)
        {
            throw new FileOperationException("Cannot read the standard output stream.", _wrapped.Name, ErrorStatus.InvalidPath);
        }
        return IsStdin ? New<RewindableStdinStream>() : _wrapped.OpenRead();
    }

    public Stream OpenUpdate()
    {
        if (IsStdIo)
        {
            throw new FileOperationException("Cannot read/write a standard IO stream.", _wrapped.Name, ErrorStatus.InvalidPath);
        }
        return _wrapped.OpenUpdate();
    }

    public Stream OpenWrite()
    {
        if (IsStdin)
        {
            throw new FileOperationException("Cannot write the standard input stream.", _wrapped.Name, ErrorStatus.InvalidPath);
        }
        return IsStdout ? Console.OpenStandardOutput() : _wrapped.OpenWrite();
    }

    public void SetFileTimes(DateTime creationTimeUtc, DateTime lastAccessTimeUtc, DateTime lastWriteTimeUtc)
    {
        if (!IsStdIo)
        {
            _wrapped.SetFileTimes(creationTimeUtc, lastAccessTimeUtc, lastWriteTimeUtc);
        }
    }
}
