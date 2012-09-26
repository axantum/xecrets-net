#region Coypright and License

/*
 * AxCrypt - Copyright 2012, Svante Seleborg, All Rights Reserved
 *
 * This file is part of AxCrypt.
 *
 * AxCrypt is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * AxCrypt is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with AxCrypt.  If not, see <http://www.gnu.org/licenses/>.
 *
 * The source is maintained at http://bitbucket.org/axantum/axcrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axantum.com for more information about the author.
*/

#endregion Coypright and License

using System;
using System.IO;
using Axantum.AxCrypt.Core.IO;

namespace Axantum.AxCrypt.Core.System
{
    public interface IRuntimeEnvironment
    {
        event EventHandler<EventArgs> FileChanged;

        void NotifyFileChanged();

        bool IsLittleEndian { get; }

        byte[] GetRandomBytes(int count);

        IRuntimeFileInfo FileInfo(string path);

        string AxCryptExtension { get; }

        Platform Platform { get; }

        int StreamBufferSize { get; }

        IFileWatcher FileWatcher(string path);

        IRuntimeFileInfo TemporaryDirectoryInfo { get; }

        DateTime UtcNow { get; }

        ILauncher Launch(string path);

        ITiming StartTiming();

        IWebCaller CreateWebCaller();

        ILogging Log { get; }
    }
}