using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }
}
