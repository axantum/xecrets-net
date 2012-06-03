using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using Axantum.AxCrypt.Core;

namespace Axantum.AxCrypt
{
    public class FileLock : IDisposable
    {
        private static StringCollection _lockedFiles = new StringCollection();

        private string _fullPath;

        private FileLock(string fullPath)
        {
            _fullPath = fullPath;
        }

        public static FileLock Lock(string fullPath)
        {
            if (fullPath == null)
            {
                throw new ArgumentNullException("fullPath");
            }
            lock (_lockedFiles)
            {
                if (IsLocked(fullPath))
                {
                    return null;
                }
                _lockedFiles.Add(fullPath);
                if (Logging.IsInfoEnabled)
                {
                    Logging.Info("Locking file '{0}'.".InvariantFormat(fullPath));
                }
                return new FileLock(fullPath);
            }
        }

        public static bool IsLocked(string fullPath)
        {
            if (fullPath == null)
            {
                throw new ArgumentNullException("fullPath");
            }
            lock (_lockedFiles)
            {
                return _lockedFiles.Contains(fullPath);
            }
        }

        public void Dispose()
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