using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
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

        public void ProgressManager_Progress(object sender, ProgressEventArgs e)
        {
            BackgroundWorker worker = e.Context as BackgroundWorker;
            if (worker == null)
            {
                return;
            }
            worker.ReportProgress(e.Percent, worker);
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