using Axantum.AxCrypt.Core.UI.ViewModel;
using Axantum.AxCrypt.Forms.Style;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Axantum.AxCrypt
{
    public partial class PasswordStrengthMeter : ProgressBar
    {
        private PasswordStrengthMeterViewModel _viewModel = new PasswordStrengthMeterViewModel(100);

        public PasswordStrengthMeter()
        {
            InitializeComponent();

            SetStyle(ControlStyles.UserPaint, true);
            Minimum = 0;
            Maximum = 100;
        }

        public event EventHandler MeterChanged;

        public bool IsAcceptable
        {
            get
            {
                return _viewModel.PasswordStrength > PasswordStrength.Unacceptable;
            }
        }

        public async Task MeterAsync(string candidate)
        {
            await Task.Run(() =>
            {
                _viewModel.PasswordCandidate = candidate;
            });

            if (Value != _viewModel.PercentStrength)
            {
                Value = _viewModel.PercentStrength;
                OnMeterChanged();
            }
        }

        protected virtual void OnMeterChanged()
        {
            EventHandler handler = MeterChanged;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Rectangle rec = e.ClipRectangle;

            rec.Width = (int)(rec.Width * ((double)Value / Maximum)) - 4;
            if (ProgressBarRenderer.IsSupported)
            {
                ProgressBarRenderer.DrawHorizontalBar(e.Graphics, e.ClipRectangle);
            }
            rec.Height = rec.Height - 4;

            using (SolidBrush brush = new SolidBrush(Color()))
            {
                e.Graphics.FillRectangle(brush, 2, 2, rec.Width, rec.Height);
            }
        }

        private Color Color()
        {
            switch (_viewModel.PasswordStrength)
            {
                case PasswordStrength.Unacceptable:
                case PasswordStrength.Bad:
                    return Styling.ErrorColor;

                case PasswordStrength.Weak:
                    return Styling.WarningColor;

                case PasswordStrength.Strong:
                    return Styling.OkColor;
            }
            throw new InvalidOperationException("Unexpected PasswordStrength level.");
        }
    }
}