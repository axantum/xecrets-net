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

using System.Diagnostics.CodeAnalysis;

using AxCrypt.Core.IO;

using static System.IO.File;

namespace Xecrets.Core.Desktop.Implementation;

internal class FileVerify : IFileVerify
{
    public bool CanReadFromFile(IStandardIoDataStore store, [NotNullWhen(false)] out string? reason)
    {
        reason = null;
        if (store.IsStdIo)
        {
            if (store.IsStdout)
            {
                reason = "Stdout is write-only";
                return false;
            }
            return true;
        }
        if (!store.IsAvailable)
        {
            reason = "File does not exist";
            return false;
        }
        try
        {
            using (store.OpenRead())
            {
                return true;
            }
        }
        catch (Exception ex)
        {
            reason = ex.Message;
            return false;
        }
    }

    public bool CanWriteToFile(IStandardIoDataStore store)
    {
        if (store.IsStdIo)
        {
            return store.IsStdout;
        }

        bool wasExisting = Exists(store.FullName);

        bool canWrite = CanWrite();
        if (!wasExisting && canWrite)
        {
            Delete(store.FullName);
        }
        return canWrite;

        bool CanWrite() {
            try
            {
                using (OpenWrite(store.FullName))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }

    public bool CanWriteToFolder(IDataContainer container)
    {
        try
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(container.FullName);
            if (!directoryInfo.Exists)
            {
                return false;
            }
            string randomName = Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + ".tmp";
            string fullPath = Path.Combine(directoryInfo.FullName, randomName);
            FileInfo fileInfo = new FileInfo(fullPath);
            try
            {
                using (fileInfo.Create())
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
            finally
            {
                fileInfo.Delete();
            }
        }
        catch
        {
            return false;
        }
    }
}
