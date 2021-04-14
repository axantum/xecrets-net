using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace AxCrypt.Forms.Style
{
    /// <summary>
    /// No gradient color table
    /// </summary>
    internal class AxCryptProfessionalColorTable : ProfessionalColorTable
    {
        public override Color ToolStripGradientBegin
        {
            get { return SystemColors.Control; }
        }

        public override Color ToolStripGradientMiddle
        {
            get { return SystemColors.Control; }
        }

        public override Color ToolStripGradientEnd
        {
            get { return SystemColors.Control; }
        }

        public override Color ButtonSelectedGradientBegin
        {
            get { return System.Drawing.Color.FromArgb(96, 120, 82); }
        }

        public override Color ToolStripBorder
        {
            get { return System.Drawing.Color.FromArgb(91, 108, 78); }
        }
    }
}