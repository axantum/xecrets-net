using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Core.Runtime
{
    public abstract class UIThreadBase : IUIThread
    {
        protected SynchronizationContext Context { get; }

        public UIThreadBase()
        {
            if (SynchronizationContext.Current == null)
            {
                throw new InvalidOperationException($"{nameof(IUIThread)} must have a SyncronizationContext.Current set when initialized.");
            }
            Context = SynchronizationContext.Current;
        }

        public bool Blocked { get; set; }

        public abstract bool IsOn { get; }

        public abstract void Yield();

        public abstract void Exit();

        public void SendTo(Action action)
        {
            if (Blocked)
            {
                throw new InvalidOperationException("Can't invoke synchronously on UI thread when it's already blocked.");
            }

            Exception exception = null;
            Context.Send((state) =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
            }
            , null);
            HandleException(exception);
        }

        public async Task SendToAsync(Func<Task> action)
        {
            if (Blocked)
            {
                throw new InvalidOperationException("Can't invoke synchronously on UI thread when it's already blocked.");
            }

            TaskCompletionSource<Exception> completion = new TaskCompletionSource<Exception>();
            Context.Send(async (state) =>
            {
                try
                {
                    await action();
                }
                catch (Exception ex)
                {
                    completion.SetResult(ex);
                    return;
                }
                completion.SetResult(null);
            }, null);
            HandleException(await completion.Task);
        }

        public void PostTo(Action action)
        {
            Context.Post((state) => action(), null);
        }

        private static void HandleException(Exception exception)
        {
            if (exception is AxCryptException)
            {
                throw exception;
            }
            if (exception != null)
            {
                throw new InvalidOperationException("Exception on UI Thread", exception);
            }
        }
    }
}