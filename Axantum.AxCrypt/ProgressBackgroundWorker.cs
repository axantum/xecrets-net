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
using System.Drawing;
using System.Windows.Forms;
using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Core.System;
using Axantum.AxCrypt.Core.UI;
using Axantum.AxCrypt.Properties;

namespace Axantum.AxCrypt
{
    /// <summary>
    /// Background thread operations with progress bar support
    /// </summary>
    internal class ProgressBackgroundWorker
    {
        private AxCryptMainForm _mainForm;

        private IDictionary<BackgroundWorker, ProgressBar> _progressBars = new Dictionary<BackgroundWorker, ProgressBar>();

        public ProgressBackgroundWorker(AxCryptMainForm mainForm)
        {
            _mainForm = mainForm;
        }

        public void BackgroundWorkWithProgress(string displayText, Func<ProgressContext, FileOperationStatus> work, Action<FileOperationStatus> complete)
        {
            ThreadWorker worker = new ThreadWorker(displayText, work, complete);
            worker.Prepare += (object sender, ThreadWorkerEventArgs e) =>
            {
                ProgressBar progressBar = CreateProgressBar(e.Worker);
                _progressBars.Add(e.Worker, progressBar);
            };
            worker.Completed += (object sender, ThreadWorkerEventArgs e) =>
            {
                ProgressBar progressBar = _progressBars[e.Worker];
                progressBar.Parent = null;
                _progressBars.Remove(e.Worker);
                progressBar.Dispose();
            };
            worker.Progress += (object sender, ThreadWorkerEventArgs e) =>
            {
                ProgressBar progressBar = _progressBars[e.Worker];
                progressBar.Value = e.ProgressPercentage;
            };

            worker.Run();
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

        public void WaitForBackgroundIdle()
        {
            while (_progressBars.Count > 0)
            {
                Application.DoEvents();
            }
        }
    }
}