using Axantum.AxCrypt.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.Runtime
{
    public abstract class UIThreadBase : IUIThread
    {
        protected SynchronizationContext Context { get; }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "SyncronizationContext")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "IUIThread")]
        protected UIThreadBase()
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

        public abstract void ExitApplication();

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public void SendTo(Action action)
        {
            if (IsOn)
            {
                action();
                return;
            }

            if (Blocked)
            {
#if DEBUG
                Debugger.Break();
#endif
                while (Blocked)
                {
                    New<ISleep>().Time(TimeSpan.FromMilliseconds(1));
                }
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
            if (IsOn)
            {
                await action();
                return;
            }

            if (Blocked)
            {
#if DEBUG
                Debugger.Break();
#endif
                while (Blocked)
                {
                    await Task.Delay(1);
                }
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