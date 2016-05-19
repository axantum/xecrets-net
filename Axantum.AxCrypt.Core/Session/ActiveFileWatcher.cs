using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Portable;
using System;
using System.Collections.Generic;
using System.Linq;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.Session
{
    public class ActiveFileWatcher : IDisposable
    {
        private Dictionary<string, IFileWatcher> _activeFileFolderWatchers = new Dictionary<string, IFileWatcher>();

        public ActiveFileWatcher()
        {
        }

        public void Add(IDataItem file)
        {
            if (file == null)
            {
                throw new ArgumentNullException("file");
            }

            string folder = Resolve.Portable.Path().GetDirectoryName(file.FullName);
            lock (_activeFileFolderWatchers)
            {
                if (_activeFileFolderWatchers.ContainsKey(folder))
                {
                    return;
                }
                if (CheckIsSubfolderOfWatched(folder))
                {
                    return;
                }
                IFileWatcher fileWatcher = New<IFileWatcher>(folder);
                CheckIfHasWatchedSubfolders(fileWatcher, folder);
                fileWatcher.FileChanged += HandleActiveFileFolderChangedEvent;
                _activeFileFolderWatchers.Add(folder, fileWatcher);
            }
        }

        private bool CheckIsSubfolderOfWatched(string folder)
        {
            foreach (string key in _activeFileFolderWatchers.Keys.ToList())
            {
                string keyFolder = key + Resolve.Portable.Path().DirectorySeparatorChar.ToString();
                if (!folder.StartsWith(keyFolder))
                {
                    continue;
                }

                _activeFileFolderWatchers[key].IncludeSubdirectories = true;
                return true;
            }
            return false;
        }

        private void CheckIfHasWatchedSubfolders(IFileWatcher fileWatcher, string folder)
        {
            folder = folder + Resolve.Portable.Path().DirectorySeparatorChar.ToString();
            bool hasWatchedSubFolders = false;
            foreach (string key in _activeFileFolderWatchers.Keys.ToList())
            {
                if (key.StartsWith(folder))
                {
                    IFileWatcher subWatcher = _activeFileFolderWatchers[key];
                    _activeFileFolderWatchers.Remove(key);
                    subWatcher.Dispose();
                    hasWatchedSubFolders = true;
                }
            }
            if (hasWatchedSubFolders)
            {
                fileWatcher.IncludeSubdirectories = true;
            }
        }

        private void HandleActiveFileFolderChangedEvent(object sender, FileWatcherEventArgs e)
        {
            Resolve.SessionNotify.Notify(new SessionNotification(SessionNotificationType.UpdateActiveFiles, e.FullNames));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeInternal();
            }
        }

        private void DisposeInternal()
        {
            lock (_activeFileFolderWatchers)
            {
                foreach (IFileWatcher fileWatcher in _activeFileFolderWatchers.Values)
                {
                    fileWatcher.Dispose();
                }
                _activeFileFolderWatchers.Clear();
            }
        }
    }
}