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

using AxCrypt.Core.Extensions;
using AxCrypt.Core.IO;

namespace Xecrets.Core.Desktop.Implementation;

internal sealed class FileWrapper(IDataStore wrapped) : IFile
{
    public IDataStore Wrapped { get; } = wrapped;

    public bool IsStdout => Wrapped is IStandardIoDataStore { IsStdout: true };

    public bool IsStdin => Wrapped is IStandardIoDataStore { IsStdin: true };

    public bool IsStdIo => Wrapped is IStandardIoDataStore { IsStdIo: true };

    public bool IsNamedStdIo => Wrapped is IStandardIoDataStore { IsNamedStdIo: true };

    public bool IsEncryptable => Wrapped is IStandardIoDataStore standardIoFile ? standardIoFile.IsEncryptable : Wrapped.IsEncryptable;

    public bool IsAvailable => IsStdIo || Wrapped.IsAvailable;

    public string AliasName => Wrapped is IStandardIoDataStore standardIoFile ? standardIoFile.AliasName : Wrapped.Name;

    public string Name => Wrapped.Name;

    public string FullName => Wrapped.FullName;

    public DateTime CreationTimeUtc => Wrapped.CreationTimeUtc;

    public DateTime LastAccessTimeUtc => Wrapped.LastAccessTimeUtc;

    public DateTime LastWriteTimeUtc => Wrapped.LastWriteTimeUtc;

    public IFolder ParentFolder => new FolderWrapper(Wrapped.Container);

    public Stream OpenRead() => Wrapped.OpenRead();

    public Stream OpenWrite() => Wrapped.OpenWrite();

    public long Length => Wrapped.Length();

    public void SetFileTimes(DateTime creationTimeUtc, DateTime lastAccessTimeUtc, DateTime lastWriteTimeUtc) =>
        Wrapped.SetFileTimes(creationTimeUtc, lastAccessTimeUtc, lastWriteTimeUtc);

    public void DeleteIfAvailable()
    {
        if (Wrapped.IsAvailable)
        {
            Wrapped.IsWriteProtected = false;
            Wrapped.Delete();
        }
    }

    public byte[] ReadAllBytes() => Wrapped.ToArray();
}
