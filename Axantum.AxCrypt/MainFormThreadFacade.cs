#region Coypright and License

/*
 * AxCrypt - Copyright 2012, Svante Seleborg, All Rights Reserved
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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Core.System;
using Axantum.AxCrypt.Core.UI;

namespace Axantum.AxCrypt
{
    /// <summary>
    /// This class acts as the intermediary between other threads and the UI thread. Code here is not
    /// assumed to execute on the UI thread, but may call back to the main form UI thread via Invoke.
    /// </summary>
    internal class MainFormThreadFacade
    {
        private AxCryptMainForm _mainForm;

        private IDictionary<BackgroundWorker, ProgressBar> _progressBars = new Dictionary<BackgroundWorker, ProgressBar>();

        public MainFormThreadFacade(AxCryptMainForm mainForm)
        {
            _mainForm = mainForm;
        }

        public void FormatTraceMessage(string message)
        {
            InvokeIfRequired(() =>
            {
                _mainForm.FormatTraceMessage(message);
            });
        }

        public void EncryptedFileManager_Changed(object sender, EventArgs e)
        {
            InvokeIfRequired(_mainForm.RestartTimer);
        }

        public void EncryptedFileManager_VersionChecked(object sender, VersionEventArgs e)
        {
            _mainForm.EncryptedFileManager.FileSystemState.LastVersionCheckUtc = AxCryptEnvironment.Current.UtcNow;
            _mainForm.EncryptedFileManager.FileSystemState.Save();
            InvokeIfRequired(() => { _mainForm.UpdateVersionStatus(e.VersionUpdateStatus, e.UpdateWebPageUrl, e.Version); });
        }

        public void DoBackgroundWork(string displayText, Action<WorkerArguments> action, RunWorkerCompletedEventHandler completedHandler)
        {
            BackgroundWorker worker = CreateWorker(
                (object sender, DoWorkEventArgs e) =>
                {
                    WorkerArguments arguments = (WorkerArguments)e.Argument;
                    try
                    {
                        action(arguments);
                    }
                    catch (OperationCanceledException)
                    {
                        e.Result = FileOperationStatus.Canceled;
                        return;
                    }
                    e.Result = arguments.Result;
                },
                completedHandler);
            ProgressContext progressContext = new ProgressContext(displayText, worker);
            progressContext.Progressing += BackgroundWorker_Progress;
            worker.RunWorkerAsync(new WorkerArguments(progressContext));
        }

        private BackgroundWorker CreateWorker(DoWorkEventHandler doWorkHandler, RunWorkerCompletedEventHandler completedHandler)
        {
            if (Logging.IsInfoEnabled)
            {
                Logging.Info("Creating BackgroundWorker");            //MLHIDE
            }

            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;

            ProgressBar progressBar = CreateProgressBar(worker);
            _progressBars.Add(worker, progressBar);

            RunWorkerCompletedEventHandler completedHandlerWrapper = null;
            completedHandlerWrapper = (object sender, RunWorkerCompletedEventArgs e) =>
            {
                try
                {
                    progressBar.Parent = null;
                    _progressBars.Remove(worker);
                    progressBar.Dispose();
                    if (completedHandler != null)
                    {
                        completedHandler(sender, e);
                    }
                }
                finally
                {
                    worker.DoWork -= doWorkHandler;
                    worker.RunWorkerCompleted -= completedHandlerWrapper;
                    worker.ProgressChanged -= worker_ProgressChanged;
                    worker.Dispose();
                }
                if (Logging.IsInfoEnabled)
                {
                    Logging.Info("Disposing BackgroundWorker");       //MLHIDE
                }
            };

            worker.DoWork += doWorkHandler;
            worker.RunWorkerCompleted += completedHandlerWrapper;
            worker.ProgressChanged += worker_ProgressChanged;

            return worker;
        }

        private ProgressBar CreateProgressBar(BackgroundWorker worker)
        {
            ProgressBar progressBar = new ProgressBar();
            progressBar.Minimum = 0;
            progressBar.Maximum = 100;
            _mainForm.ProgressPanel.Controls.Add(progressBar);
            progressBar.Dock = DockStyle.Fill;
            progressBar.Margin = new Padding(0);
            progressBar.MouseClick += new MouseEventHandler(progressBar_MouseClick);
            progressBar.Tag = worker;
            return progressBar;
        }

        private void progressBar_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
            {
                return;
            }
            _mainForm.ShowProgressContextMenu((ProgressBar)sender, e.Location);
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

        private void BackgroundWorker_Progress(object sender, ProgressEventArgs e)
        {
            BackgroundWorker worker = e.Context as BackgroundWorker;
            if (worker != null)
            {
                if (worker.CancellationPending)
                {
                    throw new OperationCanceledException();
                }
                worker.ReportProgress(e.Percent, worker);
                return;
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
            if (_mainForm.InvokeRequired)
            {
                _mainForm.BeginInvoke(action);
            }
            else
            {
                action();
            }
        }
    }
}