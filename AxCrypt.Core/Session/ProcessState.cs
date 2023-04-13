#region Coypright and License

/*
 * AxCrypt - Copyright 2016, Svante Seleborg, All Rights Reserved
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
 * The source is maintained at http://bitbucket.org/AxCrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using AxCrypt.Core.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace AxCrypt.Core.Session
{
    public class ProcessState : IDisposable
    {
        [AllowNull]
        private Dictionary<string, List<ILauncher>> _processState = new Dictionary<string, List<ILauncher>>();

        private readonly object _lock = new object();

        public void Add(ILauncher launcher, ActiveFile activeFile)
        {
            if (activeFile == null)
            {
                throw new ArgumentNullException(nameof(activeFile));
            }

            PurgeInactive();
            lock (_lock)
            {
                List<ILauncher>? processes = ActiveProcesses(activeFile);
                if (processes == null)
                {
                    processes = new List<ILauncher>();
                    _processState[activeFile.EncryptedFileInfo.FullName] = processes;
                }
                processes.Add(launcher);
            }
        }

        public IEnumerable<ILauncher> ActiveLaunchersFor(string appName)
        {
            lock (_lock)
            {
                return _processState.Values.SelectMany(v => v).Where(l => l.Name == appName).ToList();    
            }
        }

        public bool HasActiveProcess(ActiveFile activeFile)
        {
            lock (_lock)
            {
                List<ILauncher>? processes = ActiveProcesses(activeFile);
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

        private List<ILauncher>? ActiveProcesses(ActiveFile activeFile)
        {
            lock (_lock)
            {
                if (!_processState.TryGetValue(activeFile.EncryptedFileInfo.FullName, out List<ILauncher>? processes))
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
