using AxCrypt.Core.Runtime;
using AxCrypt.Core.Session;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Mono
{
    public class ProcessMonitor : IDisposable
    {
        private DelayedAction _action;

        private readonly object _lock = new object();

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

        private async void CheckProcesses(object sender, EventArgs e)
        {
            bool processHasExited = false;
            processHasExited = CheckForExitedProcesses();

            if (processHasExited)
            {
                await ProcessHasExited();
            }
            _action?.StartIdleTimer();
        }

        private bool CheckForExitedProcesses()
        {
            bool processHasExited;
            lock (_lock)
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

        private static async Task ProcessHasExited()
        {
            await New<SessionNotify>().NotifyAsync(new SessionNotification(SessionNotificationType.SessionChange));
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