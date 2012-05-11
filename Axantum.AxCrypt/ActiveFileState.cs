using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt
{
    public static class ActiveFileState
    {
        private static IList<ActiveFile> _activeFiles = new List<ActiveFile>();

        public static void AddActiveFile(ActiveFile activeFile)
        {
            lock (_activeFiles)
            {
                _activeFiles.Add(activeFile);
            }
        }

        public static void CheckActiveFileStatus()
        {
            lock (_activeFiles)
            {
                IList<ActiveFile> activeFiles = new List<ActiveFile>();

                foreach (ActiveFile activeFile in _activeFiles)
                {
                    ActiveFile updatedActiveFile = CheckActiveFileStatus(activeFile);
                    if (updatedActiveFile != null)
                    {
                        activeFiles.Add(activeFile);
                    }
                }
                _activeFiles = activeFiles;
            }
        }

        private static ActiveFile CheckActiveFileStatus(ActiveFile activeFile)
        {
            return activeFile;
        }
    }
}