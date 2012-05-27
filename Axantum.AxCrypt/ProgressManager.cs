using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Axantum.AxCrypt.Core.UI;

namespace Axantum.AxCrypt
{
    public class ProgressManager
    {
        public event EventHandler<ProgressEventArgs> Progress;

        public ProgressContext Create(string displayText, object context)
        {
            ProgressContext progress = new ProgressContext(displayText, context);
            progress.Progressing += new EventHandler<ProgressEventArgs>(progress_Progressing);

            return progress;
        }

        public ProgressContext Create(string displayText)
        {
            return Create(displayText, null);
        }

        private void progress_Progressing(object sender, ProgressEventArgs e)
        {
            OnProgress(e);
        }

        protected virtual void OnProgress(ProgressEventArgs e)
        {
            EventHandler<ProgressEventArgs> handler = Progress;
            if (handler != null)
            {
                handler(null, e);
            }
        }
    }
}