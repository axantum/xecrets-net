using System;
using System.Threading;
using System.Threading.Tasks;

namespace AxCrypt.Common
{
    public sealed class AsyncLock : IDisposable
    {
        private SemaphoreSlim? _semaphore = new SemaphoreSlim(1, 1);

        private readonly Task<IDisposable?> _releaser;

        public AsyncLock()
        {
            _releaser = Task.FromResult((IDisposable?)new Releaser(this));
        }

        public Task<IDisposable?> LockAsync()
        {
            Task wait = _semaphore?.WaitAsync() ?? throw new InvalidOperationException("Internal program error, calling LockAsync on a disposed instance.");
            if (wait.IsCompleted)
            {
                return _releaser;
            }
            return wait.ContinueWith(
                (task, state) =>
                {
                    return (IDisposable?)state;
                },
                _releaser.Result, CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);
        }

        public void Dispose()
        {
            if (_semaphore != null)
            {
                _semaphore.Dispose();
            }
            _semaphore = null;
        }

        private sealed class Releaser : IDisposable
        {
            private readonly AsyncLock m_toRelease;

            internal Releaser(AsyncLock toRelease)
            {
                m_toRelease = toRelease;
            }

            public void Dispose()
            {
                _ = m_toRelease._semaphore?.Release();
            }
        }
    }
}
