using Axantum.AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Axantum.AxCrypt.Forms.Implementation
{
    public class ProgressBackgroundComponent : Component, IProgressBackground
    {
        private ProgressBackground _progressBackground = new ProgressBackground();

        public ProgressBackgroundComponent(IContainer container)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }

            container.Add(this);

            _progressBackground.OperationStarted += (sender, e) =>
            {
                ProgressBar progressBar = CreateProgressBar(e.ProgressContext);
                e.State = progressBar;
                OnProgressBarCreated(new ControlEventArgs(progressBar));
            };

            _progressBackground.OperationCompleted += (sender, e) =>
            {
                ProgressBar progressBar = e.State as ProgressBar;
                progressBar?.Dispose();
            };
        }

        /// <summary>
        /// Raised when a new progress bar has been created. This is typically a good time
        /// to add it to a container control. This is raised on the original thread, typically
        /// the GUI thread.
        /// </summary>
        public event EventHandler<ControlEventArgs> ProgressBarCreated;

        protected virtual void OnProgressBarCreated(ControlEventArgs e)
        {
            ProgressBarCreated?.Invoke(this, e);
        }

        /// <summary>
        /// Raised when a progress bar is clicked. Use to display a context menu
        /// or other information. This is raised on the original thread, typically the
        /// GUI thread.
        /// </summary>
        public event EventHandler<MouseEventArgs> ProgressBarClicked;

        protected virtual void OnProgressBarClicked(object sender, MouseEventArgs e)
        {
            ProgressBarClicked?.Invoke(sender, e);
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
            progress.Progressing += (ss, ee) =>
            {
                progressBar.Value = ee.Percent;
            };

            return progressBar;
        }

        private void progressBar_MouseClick(object sender, MouseEventArgs e)
        {
            OnProgressBarClicked(sender, e);
        }

        public bool Busy
        {
            get
            {
                return _progressBackground.Busy;
            }
        }

        public void WaitForIdle()
        {
            _progressBackground.WaitForIdle();
        }

        public Task WorkAsync(string name, Func<IProgressContext, Task<FileOperationContext>> workFunctionAsync, Func<FileOperationContext, Task> completeAsync, IProgressContext progress)
        {
            return _progressBackground.WorkAsync(name, workFunctionAsync, completeAsync, progress);
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