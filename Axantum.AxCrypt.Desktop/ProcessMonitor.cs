using Axantum.AxCrypt.Core.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Desktop
{
    public class ProcessMonitor : IDisposable
    {
        private DelayedAction _action;

        private HashSet<int> _processIds;

        private int _currentSessionId;

        public ProcessMonitor()
        {
            _action = new DelayedAction(New<IDelayTimer>(), TimeSpan.FromMilliseconds(500));
            _currentSessionId = Process.GetCurrentProcess().SessionId;
            _processIds = GetCurrentIds();
            _action.Action += CheckProcesses;
            _action.StartIdleTimer();
        }

        private void CheckProcesses(object sender, EventArgs e)
        {
            bool processHasExited = false;
            processHasExited = CheckForExitedProcesses();

            if (processHasExited)
            {
                ProcessHasExited();
            }
            _action.StartIdleTimer();
        }

        private bool CheckForExitedProcesses()
        {
            bool processHasExited;
            lock (_action)
            {
                HashSet<int> currentIds = GetCurrentIds();
                processHasExited = _processIds.Except(currentIds).Any();
                _processIds = currentIds;
            }

            return processHasExited;
        }

        private HashSet<int> GetCurrentIds()
        {
            return new HashSet<int>(Process.GetProcesses().Where(p => p.SessionId == _currentSessionId).Select(p => p.Id).ToList());
        }

        private static async void ProcessHasExited()
        {
            await Core.Resolve.SessionNotify.NotifyAsync(new Core.Session.SessionNotification(Core.Session.SessionNotificationType.SessionChange));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }
            if (_action != null)
            {
                _action.Dispose();
                _action = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}