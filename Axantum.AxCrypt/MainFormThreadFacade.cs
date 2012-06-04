using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Axantum.AxCrypt.Core.UI;

namespace Axantum.AxCrypt
{
    /// <summary>
    /// This class acts as the intermediary between other threads and the UI thread. Code here is not
    /// assumed to execute on the UI thread, but may call back to the main form UI thread via Invoke.
    /// </summary>
    internal class MainFormThreadFacade
    {
        private AxCryptMainForm MainForm { get; set; }

        private IDictionary<BackgroundWorker, ProgressBar> _progressBars = new Dictionary<BackgroundWorker, ProgressBar>();

        private ProgressManager ProgressManager { get; set; }

        public MainFormThreadFacade(AxCryptMainForm mainForm)
        {
            MainForm = mainForm;

            ProgressManager = new ProgressManager();
            ProgressManager.Progress += new EventHandler<ProgressEventArgs>(ProgressManager_Progress);
        }

        public void FormatTraceMessage(string message)
        {
            InvokeIfRequired(() =>
            {
                MainForm.FormatTraceMessage(message);
            });
        }

        public void EncryptedFileManager_Changed(object sender, EventArgs e)
        {
            InvokeIfRequired(MainForm.RestartTimer);
        }

        public void DoBackgroundWork(string displayText, RunWorkerCompletedEventHandler completedHandler, Action<WorkerArguments> action)
        {
            BackgroundWorker worker = CreateWorker(completedHandler);
            worker.DoWork += (object sender, DoWorkEventArgs e) =>
            {
                WorkerArguments arguments = (WorkerArguments)e.Argument;
                action(arguments);
                e.Result = arguments.Result;
            };
            worker.RunWorkerAsync(new WorkerArguments(ProgressManager.Create(displayText, worker)));
        }

        private BackgroundWorker CreateWorker(RunWorkerCompletedEventHandler completedHandler)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            ProgressBar progressBar = CreateProgressBar();
            _progressBars.Add(worker, progressBar);
            worker.RunWorkerCompleted += (object sender, RunWorkerCompletedEventArgs e) =>
            {
                progressBar.Parent = null;
                _progressBars.Remove(worker);
                progressBar.Dispose();
                if (completedHandler != null)
                {
                    MainForm.BeginInvoke((Action)(() => { completedHandler(sender, e); }));
                }
                worker.Dispose();
            };
            worker.ProgressChanged += new ProgressChangedEventHandler(worker_ProgressChanged);

            return worker;
        }

        private ProgressBar CreateProgressBar()
        {
            ProgressBar progressBar = new ProgressBar();
            progressBar.Minimum = 0;
            progressBar.Maximum = 100;
            MainForm.ProgressPanel.Controls.Add(progressBar);
            progressBar.Dock = DockStyle.Fill;
            progressBar.Margin = new Padding(0);
            return progressBar;
        }

        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            BackgroundWorker worker = (BackgroundWorker)e.UserState;
            ProgressBar progressBar = _progressBars[worker];
            progressBar.Value = e.ProgressPercentage;
        }

        public void WaitForBackgroundIdle()
        {
            while (_progressBars.Count > 0)
            {
                Application.DoEvents();
            }
        }

        private void ProgressManager_Progress(object sender, ProgressEventArgs e)
        {
            BackgroundWorker worker = e.Context as BackgroundWorker;
            if (worker != null)
            {
                worker.ReportProgress(e.Percent, worker);
            }
            ProgressBar progressBar = e.Context as ProgressBar;
            if (progressBar != null)
            {
                progressBar.Value = e.Percent;
                if (progressBar.Value == 100)
                {
                    progressBar.Parent = null;
                    progressBar.Dispose();
                }
            }
            return;
        }

        private void InvokeIfRequired(Action action)
        {
            if (MainForm.InvokeRequired)
            {
                MainForm.BeginInvoke(action);
            }
            else
            {
                action();
            }
        }
    }
}