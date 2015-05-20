#region Coypright and License

/*
 * AxCrypt - Copyright 2014, Svante Seleborg, All Rights Reserved
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
 * The source is maintained at http://bitbucket.org/axantum/axcrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axantum.com for more information about the author.
*/

#endregion Coypright and License

using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.UI;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace Axantum.AxCrypt.Forms
{
    /// <summary>
    /// Background thread operations with progress bar support
    /// </summary>
    public class ProgressBackground : Component, IProgressBackground
    {
        private long _workerCount = 0;

        public ProgressBackground(IContainer container)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }

            container.Add(this);
        }

        /// <summary>
        /// Raised when a new progress bar has been created. This is typically a good time
        /// to add it to a container control. This is raised on the original thread, typically
        /// the GUI thread.
        /// </summary>
        public event EventHandler<ControlEventArgs> ProgressBarCreated;

        protected virtual void OnProgressBarCreated(ControlEventArgs e)
        {
            EventHandler<ControlEventArgs> handler = ProgressBarCreated;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Raised when a progress bar is clicked. Use to display a context menu
        /// or other information. This is raised on the original thread, typically the
        /// GUI thread.
        /// </summary>
        public event EventHandler<MouseEventArgs> ProgressBarClicked;

        protected virtual void OnProgressBarClicked(object sender, MouseEventArgs e)
        {
            EventHandler<MouseEventArgs> handler = ProgressBarClicked;
            if (handler != null)
            {
                handler(sender, e);
            }
        }

        /// <summary>
        /// Perform a background operation with support for progress bars and cancel.
        /// </summary>
        /// <param name="workFunction">A 'work' delegate, taking a ProgressContext and return a FileOperationStatus. Executed on a background thread. Not the calling/GUI thread.</param>
        /// <param name="complete">A 'complete' delegate, taking the final status. Executed on the GUI thread.</param>
        public void Work(Func<IProgressContext, FileOperationContext> workFunction, Action<FileOperationContext> complete)
        {
            Resolve.UIThread.RunOnUIThread(() =>
            {
                BackgroundWorkWithProgressOnUIThread(workFunction, complete);
            });
        }

        private void BackgroundWorkWithProgressOnUIThread(Func<IProgressContext, FileOperationContext> work, Action<FileOperationContext> complete)
        {
            IProgressContext progress = new CancelProgressContext(new ProgressContext());
            ProgressBar progressBar = CreateProgressBar(progress);
            OnProgressBarCreated(new ControlEventArgs(progressBar));
            progress.Progressing += (object sender, ProgressEventArgs e) =>
            {
                progressBar.Value = e.Percent;
            };
            IThreadWorker threadWorker = Resolve.Portable.ThreadWorker(progress, false);
            threadWorker.Work += (object sender, ThreadWorkerEventArgs e) =>
            {
                e.Result = work(e.Progress);
            };
            threadWorker.Completing += (object sender, ThreadWorkerEventArgs e) =>
            {
                try
                {
                    complete(e.Result);
                    progressBar.Parent = null;
                }
                finally
                {
                    progressBar.Dispose();
                }
            };
            threadWorker.Completed += (object sender, ThreadWorkerEventArgs e) =>
            {
                IDisposable disposable = sender as IDisposable;
                if (disposable != null)
                {
                    disposable.Dispose();
                }
                Interlocked.Decrement(ref _workerCount);
                OnWorkStatusChanged();
            };
            Interlocked.Increment(ref _workerCount);
            OnWorkStatusChanged();
            threadWorker.Run();
        }

        private ProgressBar CreateProgressBar(IProgressContext progress)
        {
            ProgressBar progressBar = new ProgressBar();
            progressBar.Minimum = 0;
            progressBar.Maximum = 100;
            progressBar.Dock = DockStyle.Fill;
            progressBar.Margin = new Padding(0);
            progressBar.MouseClick += new MouseEventHandler(progressBar_MouseClick);
            progressBar.Tag = progress;

            return progressBar;
        }

        private void progressBar_MouseClick(object sender, MouseEventArgs e)
        {
            OnProgressBarClicked(sender, e);
        }

        /// <summary>
        /// Wait for all operations to complete.
        /// </summary>
        public void WaitForIdle()
        {
            while (Interlocked.Read(ref _workerCount) > 0)
            {
                Application.DoEvents();
            }
        }

        public event EventHandler WorkStatusChanged;

        protected virtual void OnWorkStatusChanged()
        {
            EventHandler handler = WorkStatusChanged;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }

        public bool Busy
        {
            get
            {
                return _workerCount > 0;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                WaitForIdle();
            }

            base.Dispose(disposing);
        }
    }
}