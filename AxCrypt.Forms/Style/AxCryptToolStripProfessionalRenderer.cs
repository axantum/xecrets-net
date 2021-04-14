using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace AxCrypt.Forms.Style
{
    /// <summary>
    /// Tweaking the ToolStrip renderer to get rid of gradient and ugly border artifact.
    /// </summary>
    internal class AxCryptToolStripProfessionalRenderer : ToolStripProfessionalRenderer
    {
        public AxCryptToolStripProfessionalRenderer()
            : base(new AxCryptProfessionalColorTable())
        {
        }

        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
        }

        protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs e)
        {
            if (!e.Item.Selected)
            {
                base.OnRenderButtonBackground(e);
                return;
            }

            Pen pen = new Pen(Color.FromArgb(91, 108, 78), 5);
            SolidBrush solidBrush = new SolidBrush(Color.FromArgb(91, 108, 78));
            Rectangle rectangle = new Rectangle(0, 0, e.Item.Size.Width - 1, e.Item.Size.Height - 1);

            if (e.Item.BackColor == System.Drawing.Color.FromArgb(232, 232, 232))
            {
                pen = new Pen(Color.FromArgb(232, 232, 232), 5);
                solidBrush = new SolidBrush(Color.FromArgb(232, 232, 232));
            }
            if (e.Item.BackColor == System.Drawing.Color.FromArgb(27, 39, 32))
            {
                pen = new Pen(Color.FromArgb(47, 59, 52), 5);
                solidBrush = new SolidBrush(Color.FromArgb(47, 59, 52));
            }

            e.Graphics.FillRectangle(solidBrush, rectangle);
            e.Graphics.DrawRectangle(pen, rectangle);
        }
    }
}