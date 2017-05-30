using Axantum.AxCrypt.Core.UI;
using Axantum.AxCrypt.Core.UI.ViewModel;
using Axantum.AxCrypt.Forms.Style;
using AxCrypt.Content;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Axantum.AxCrypt
{
    public partial class PasswordStrengthMeter : Control
    {
        private PasswordStrengthMeterViewModel _viewModel = new PasswordStrengthMeterViewModel();
        Label helpText = new Label();

        private ToolTip _toolTip = new ToolTip();

        public PasswordStrengthMeter()
        {
            InitializeComponent();
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();

            if (DesignMode)
            {
                return;
            }

            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
            helpText.Location = new Point(0, this.Height / 3);
            this.Controls.Add(helpText);

            _viewModel.BindPropertyChanged(nameof(PasswordStrengthMeterViewModel.PasswordStrength), (PasswordStrength strength) =>
            {
                _toolTip.SetToolTip(this, _viewModel.StrengthTip);

            });

            _viewModel.BindPropertyChanged(nameof(PasswordStrengthMeterViewModel.PasswordCandidate), (string candidate) =>
            {
                if (!string.IsNullOrEmpty(_viewModel.PasswordCandidate))
                {
                    using (SolidBrush brush = new SolidBrush(this.ForeColor))
                    {
                        switch (_viewModel.PasswordStrength)
                        {
                            case PasswordStrength.Unacceptable:
                                helpText.Text = Texts.PasswordStrengthUnacceptableName;
                                break;

                            case PasswordStrength.Bad:
                                helpText.Text = Texts.PasswordStrengthBadName;
                                break;

                            case PasswordStrength.Weak:
                                helpText.Text = Texts.PasswordStrengthWeakName;
                                return;

                            case PasswordStrength.Strong:
                                helpText.Text = Texts.PasswordStrengthStrongName;
                                return;
                        }
                    }
                }
                else
                {
                    helpText.Text = "";
                }
            });

        }

        public event EventHandler MeterChanged;

        public bool IsAcceptable
        {
            get
            {
                return _viewModel.PasswordStrength > PasswordStrength.Unacceptable;
            }
        }

        public string StrengthTip
        {
            get
            {
                return _viewModel.StrengthTip;
            }
        }

        public async Task MeterAsync(string candidate)
        {
            await Task.Run(() =>
            {
                _viewModel.PasswordCandidate = candidate;
            });

            Invalidate();
            OnMeterChanged();
        }

        protected virtual void OnMeterChanged()
        {
            MeterChanged?.Invoke(this, new EventArgs());
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException(nameof(e));
            }

            base.OnPaint(e);

            Rectangle progressBarRec = e.ClipRectangle;

            progressBarRec.Height = (progressBarRec.Height) / 3;

            if (ProgressBarRenderer.IsSupported)
            {
                ProgressBarRenderer.DrawHorizontalBar(e.Graphics, progressBarRec);
            }

            Rectangle rec = e.ClipRectangle;

            rec.Width = (int)(rec.Width * ((double)_viewModel.PercentStrength / 100)) - 4;

            rec.Height = (rec.Height / 3) - 4;

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