using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.UI
{
    public class CancelContext : IProgressContext
    {
        private IProgressContext _progress;

        public CancelContext(IProgressContext progress)
        {
            _progress = progress.Progress;
        }

        public IProgressContext Progress
        {
            get
            {
                return _progress.Progress;
            }
        }

        public long TotalCount { get; set; }

        public long CurrentCount { get; set; }

        public bool IsCancelled { get; set; }

        public void AddCount(long count)
        {
            ThrowIfCancelled();

            CurrentCount += count;
            _progress.AddCount(count);
        }

        private void ThrowIfCancelled()
        {
            if (!Cancel)
            {
                return;
            }
            if (!IsCancelled)
            {
                _progress.AddCount(TotalCount - CurrentCount);
                CurrentCount = TotalCount;
                IsCancelled = true;
                throw new OperationCanceledException("Operation canceled on request.");
            }
        }

        public void NotifyLevelStart()
        {
            ThrowIfCancelled();
            _progress.NotifyLevelStart();
        }

        public void NotifyLevelFinished()
        {
            ThrowIfCancelled();
            _progress.NotifyLevelFinished();
        }

        public bool Cancel
        {
            get
            {
                return _progress.Cancel;
            }
            set
            {
                _progress.Cancel = value;
            }
        }

        public void RemoveCount(long totalCount, long progressCount)
        {
            _progress.RemoveCount(totalCount, progressCount);
        }

        public void AddTotal(long count)
        {
            ThrowIfCancelled();

            TotalCount += count;
            _progress.AddTotal(count);
        }

        public bool AllItemsConfirmed
        {
            get
            {
                return _progress.AllItemsConfirmed;
            }
            set
            {
                _progress.AllItemsConfirmed = value;
            }
        }

        public event EventHandler<ProgressEventArgs> Progressing
        {
            add
            {
                _progress.Progressing += value;
            }
            remove
            {
                _progress.Progressing -= value;
            }
        }
    }
}