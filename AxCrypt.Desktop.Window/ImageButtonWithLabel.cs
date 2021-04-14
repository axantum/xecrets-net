using System;
using System.Drawing;
using System.Windows.Forms;

namespace AxCrypt.Desktop.Window
{
    public partial class ImageButtonWithLabel : TableLayoutPanel
    {
        private Color _toolStripBackColor = Color.FromArgb(96, 120, 82);

        public ImageButtonWithLabel()
        {
            InitializeComponent();
        }

        private void _toolTipDraw(object sender, DrawToolTipEventArgs e)
        {
            e.DrawBackground();
            e.DrawBorder();
            e.DrawText();
        }

        public Image Image
        {
            get
            {
                return _button.Image;
            }
            set
            {
                _button.Image = value;
            }
        }

        public new string Text
        {
            get
            {
                return _buttonTitle.Text;
            }
            set
            {
                _buttonTitle.Text = value;
            }
        }

        public string ToolTipText
        {
            get
            {
                return _button.ToolTipText;
            }
            set
            {
                _button.ToolTipText = value;
            }
        }

        public Padding ButtonTitleMargin
        {
            get
            {
                return _buttonTitle.Margin;
            }
            set
            {
                _buttonTitle.Margin = value;
            }
        }

        public Color ImageTransparentColor
        {
            get
            {
                return _button.ImageTransparentColor;
            }
            set
            {
                _button.ImageTransparentColor = value;
            }
        }

        public Color ToolStripeBackColor
        {
            get
            {
                return this._button.BackColor;
            }
            set
            {
                this._toolStrip.BackColor = value;
                this._button.BackColor = value;
            }
        }

        public new EventHandler Click
        {
            get
            {
                return null;
            }
            set
            {
                this._button.Click += value;
            }
        }

        public new bool Enabled
        {
            get
            {
                return this._toolStrip.Enabled;
            }
            set
            {
                this._toolStrip.Enabled = value;
                if (!value)
                {
                    this._toolStrip.BackColor = Color.FromArgb(232, 232, 232);
                    this._button.BackColor = Color.FromArgb(232, 232, 232);
                }
                else
                {
                    this._toolStrip.BackColor = _toolStripBackColor;
                    this._button.BackColor = _toolStripBackColor;
                }
            }
        }

        public string KnownFolderFullName { get; set; }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}