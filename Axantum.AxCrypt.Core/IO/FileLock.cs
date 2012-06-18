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
using System.Collections.Specialized;
using Axantum.AxCrypt.Core.System;

namespace Axantum.AxCrypt.Core.IO
{
    public class FileLock : IDisposable
    {
        private static StringCollection _lockedFiles = new StringCollection();

        private string _fullPath;

        private FileLock(string fullPath)
        {
            _fullPath = fullPath;
        }

        public static FileLock Lock(IRuntimeFileInfo fileInfo)
        {
            if (fileInfo == null)
            {
                throw new ArgumentNullException("fileInfo");
            }
            lock (_lockedFiles)
            {
                if (IsLocked(fileInfo))
                {
                    return null;
                }
                _lockedFiles.Add(fileInfo.FullName);
                if (Logging.IsInfoEnabled)
                {
                    Logging.Info("Locking file '{0}'.".InvariantFormat(fileInfo.FullName));
                }
                return new FileLock(fileInfo.FullName);
            }
        }

        public static bool IsLocked(params IRuntimeFileInfo[] fileInfoParameters)
        {
            foreach (IRuntimeFileInfo fileInfo in fileInfoParameters)
            {
                if (fileInfo == null)
                {
                    throw new ArgumentNullException("fileInfoParameters");
                }
                lock (_lockedFiles)
                {
                    if (_lockedFiles.Contains(fileInfo.FullName))
                    {
                        if (Logging.IsInfoEnabled)
                        {
                            Logging.Info("File '{0}' was found to be locked.".InvariantFormat(fileInfo.FullName));
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            lock (_lockedFiles)
            {
                if (_fullPath == null)
                {
                    return;
                }
                _lockedFiles.Remove(_fullPath);
                if (Logging.IsInfoEnabled)
                {
                    Logging.Info("Unlocking file '{0}'.".InvariantFormat(_fullPath));
                }
                _fullPath = null;
            }
        }
    }
}