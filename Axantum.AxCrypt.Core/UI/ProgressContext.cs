using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.UI
{
    public class ProgressContext
    {
        private object _context;

        public ProgressContext(string displayText, object context)
            : this()
        {
            _context = context;
            DisplayText = displayText;
        }

        public ProgressContext()
        {
            Max = -1;
        }

        public event EventHandler<ProgressEventArgs> Progressing;

        public string DisplayText { get; set; }

        public long Max { get; set; }

        private bool _done = false;

        private long _current = 0;

        public long Current
        {
            get
            {
                return _current;
            }
            set
            {
                _current = value;
                if (_current == Max)
                {
                    _done = true;
                }
                ProgressEventArgs e;
                e = new ProgressEventArgs(Percent, _context);
                OnProgressing(e);
            }
        }

        public void Finished()
        {
            Current = Max;
        }

        protected virtual void OnProgressing(ProgressEventArgs e)
        {
            if (_done)
            {
                return;
            }
            EventHandler<ProgressEventArgs> handler = Progressing;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public int Percent
        {
            get
            {
                if (_done)
                {
                    return 100;
                }
                if (Max >= 0)
                {
                    long current100 = _current * 100;
                    return (int)(current100 / Max);
                }
                return 0;
            }
        }
    }
}