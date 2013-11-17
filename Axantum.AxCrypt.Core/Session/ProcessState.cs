using Axantum.AxCrypt.Core.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Session
{
    public class ProcessState : IDisposable
    {
        private Dictionary<string, List<ILauncher>> _processState = new Dictionary<string, List<ILauncher>>();

        private readonly object _lock = new object();

        public void Add(ILauncher launcher, ActiveFile activeFile)
        {
            PurgeInactive();
            lock (_lock)
            {
                List<ILauncher> processes = ActiveProcesses(activeFile);
                if (processes == null)
                {
                    processes = new List<ILauncher>();
                    _processState[activeFile.EncryptedFileInfo.FullName] = processes;
                }
                processes.Add(launcher);
            }
        }

        public bool HasActiveProcess(ActiveFile activeFile)
        {
            lock (_lock)
            {
                List<ILauncher> processes = ActiveProcesses(activeFile);
                if (processes == null)
                {
                    return false;
                }
                foreach (ILauncher process in processes)
                {
                    if (!process.HasExited)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private List<ILauncher> ActiveProcesses(ActiveFile activeFile)
        {
            lock (_lock)
            {
                List<ILauncher> processes;
                if (!_processState.TryGetValue(activeFile.EncryptedFileInfo.FullName, out processes))
                {
                    return null;
                }
                return processes;
            }
        }

        private void PurgeInactive()
        {
            lock (_lock)
            {
                foreach (List<ILauncher> processes in _processState.Values)
                {
                    for (int i = 0; i < processes.Count; ++i)
                    {
                        if (!processes[i].HasExited)
                        {
                            continue;
                        }
                        processes[i].Dispose();
                        processes.RemoveAt(i);
                        --i;
                    }
                }
            }
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
            lock (_lock)
            {
                if (_processState == null)
                {
                    return;
                }
                foreach (List<ILauncher> processes in _processState.Values)
                {
                    foreach (ILauncher process in processes)
                    {
                        process.Dispose();
                    }
                }
                _processState = null;
            }
        }
    }
}