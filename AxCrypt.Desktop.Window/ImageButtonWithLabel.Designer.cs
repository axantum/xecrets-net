
using System;
using System.Drawing;
using System.Windows.Forms;

namespace AxCrypt.Desktop.Window
{
    partial class ImageButtonWithLabel
    {
        private System.ComponentModel.IContainer components = null;

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            _toolStrip = new System.Windows.Forms.ToolStrip();
            _button = new System.Windows.Forms.ToolStripButton();
            _buttonTitle = new System.Windows.Forms.Label();
            _toolTip = new System.Windows.Forms.ToolTip();

            this._toolStrip.SuspendLayout();
            this.SuspendLayout();

            // 
            // _toolStrip
            //
            this._toolStrip.AllowDrop = true;
            this._toolStrip.AllowMerge = false;
            this._toolStrip.Anchor = System.Windows.Forms.AnchorStyles.None;
            this._toolStrip.AutoSize = false;
            this._toolStrip.BackColor = System.Drawing.Color.Transparent;
            this._toolStrip.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this._toolStrip.CanOverflow = false;
            this._toolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this._toolStrip.GripMargin = new System.Windows.Forms.Padding(0);
            this._toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this._toolStrip.ImageScalingSize = new System.Drawing.Size(35, 35);
            this._toolStrip.Dock = DockStyle.None;
            this._toolStrip.Items.AddRange(new ToolStripItem[] {
            this._button});
            this._toolStrip.Location = new System.Drawing.Point(0);
            this._toolStrip.Margin = new System.Windows.Forms.Padding(0);
            this._toolStrip.Name = "_toolStrip";
            this._toolStrip.Padding = new System.Windows.Forms.Padding(0);
            this._toolStrip.Size = new Size(50, 40);
            // 
            // _button
            //
            this._button.AutoSize = false;
            this._button.DisplayStyle = ToolStripItemDisplayStyle.Image;
            this._button.ImageTransparentColor = System.Drawing.Color.FromArgb(96, 120, 82);
            this._button.Margin = new Padding(0, 0, 0, 0);
            this._button.Name = "_customToolstripButton";
            this._button.Size = new Size(58, 54);
            this._button.ImageAlign = ContentAlignment.MiddleCenter;
            // 
            // _buttonTitle
            //
            this._buttonTitle.ForeColor = Color.White;
            this._buttonTitle.Font = new Font("Microsoft Sans Serif", 6.75F, FontStyle.Regular);
            this._buttonTitle.BackColor = Color.Transparent;
            this._buttonTitle.Visible = true;
            this._buttonTitle.TextAlign = ContentAlignment.TopCenter;
            this._buttonTitle.Anchor = AnchorStyles.Top;
            this._buttonTitle.AutoSize = true;
            this._buttonTitle.ImeMode = ImeMode.NoControl;
            // 
            // _toolStrip
            //
            _toolTip.BackColor = Color.FromArgb(241, 241, 241);
            _toolTip.ForeColor = Color.Black;
            _toolTip.IsBalloon = true;
            _toolTip.Draw += _toolTipDraw;
            // 
            // ImageButtonWithLabel
            //
            this.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left) | AnchorStyles.Right | AnchorStyles.Left)));
            this.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.ColumnCount = 1;
            this.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.Controls.Add(this._toolStrip, 0, 0);
            this.Controls.Add(this._buttonTitle, 0, 1);
            this.Location = new System.Drawing.Point(0, 0);
            this.Margin = new Padding(0);
            this.RowCount = 2;
            this.RowStyles.Add(new RowStyle());
            this.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.Size = new Size(50, 60);
            this.MinimumSize = new Size(50, 60);
            this.BackColor = Color.Transparent;
            this._toolStrip.ResumeLayout(false);
            this._toolStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private ToolStrip _toolStrip;
        private ToolStripButton _button;
        private Label _buttonTitle;
        private ToolTip _toolTip;
    }
}
