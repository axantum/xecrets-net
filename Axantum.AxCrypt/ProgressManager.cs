using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Axantum.AxCrypt.Core.UI;

namespace Axantum.AxCrypt
{
    public class ProgressManager
    {
        public event EventHandler<EventArgs> Progress;

        public ProgressContext Create(string displayText)
        {
            ProgressContext progress = new ProgressContext(displayText);
            progress.Progressing += new EventHandler<ProgressEventArgs>(progress_Progressing);

            return progress;
        }

        private void progress_Progressing(object sender, ProgressEventArgs e)
        {
            OnProgress();
        }

        private void OnProgress()
        {
            EventHandler<EventArgs> handler = Progress;
            if (handler != null)
            {
                handler(null, new EventArgs());
            }
        }
    }
}