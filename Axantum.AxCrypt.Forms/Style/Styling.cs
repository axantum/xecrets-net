using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Forms.Style;
using Axantum.AxCrypt.Forms.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Axantum.AxCrypt.Forms.Style
{
    public class Styling
    {
        private Color _buttonBackgroundColor = Color.FromArgb(134, 185, 110);

        private Color _buttonForegroundColor = Color.White;

        private Color _buttonBorderColor = Color.FromArgb(106, 157, 83);

        private Color _buttonMouseOverColor = Color.FromArgb(232, 232, 232);

        private Icon _icon;

        public Styling(Icon icon)
        {
            _icon = icon;
        }

        public void Style(params Control[] controls)
        {
            if (controls == null)
            {
                throw new ArgumentNullException("controls");
            }

            foreach (Control control in controls)
            {
                StyleInternal(control);
            }
        }

        private void StyleInternal(Control control)
        {
            FontLoader fontLoader = TypeMap.Resolve.Singleton<FontLoader>();

            if (control is Form)
            {
                Form form = (Form)control;
                form.Font = fontLoader.ContentText;
                form.Icon = _icon;
            }

            switch (control.GetType().ToString())
            {
                case "System.Windows.Forms.Button":
                    Button button = (Button)control;
                    button.BackColor = _buttonBackgroundColor;
                    button.ForeColor = _buttonForegroundColor;
                    button.FlatStyle = FlatStyle.Flat;
                    button.FlatAppearance.BorderColor = _buttonBorderColor;
                    button.FlatAppearance.MouseOverBackColor = _buttonMouseOverColor;
                    button.FlatAppearance.BorderSize = 1;
                    break;

                case "System.Windows.Forms.GroupBox":
                    GroupBox groupBox = (GroupBox)control;
                    groupBox.Font = fontLoader.PromptText;
                    break;

                case "System.Windows.Forms.TextBox":
                    TextBox textBox = (TextBox)control;
                    textBox.Font = fontLoader.ContentText;
                    break;

                case "System.Windows.Forms.CheckBox":
                    CheckBox checkBox = (CheckBox)control;
                    checkBox.Font = fontLoader.ContentText;
                    break;

                case "System.Windows.Forms.Panel":
                    Panel panel = (Panel)control;
                    panel.Font = fontLoader.ContentText;
                    break;

                case "System.Windows.Forms.MenuStrip":
                    MenuStrip menuStrip = (MenuStrip)control;
                    menuStrip.Font = fontLoader.ContentText;
                    break;

                case "System.Windows.Forms.ContextMenuStrip":
                    ContextMenuStrip contextMenuStrip = (ContextMenuStrip)control;
                    contextMenuStrip.Font = fontLoader.ContentText;
                    break;

                case "System.Windows.Forms.ToolStrip":
                    ToolStrip toolStrip = (ToolStrip)control;
                    toolStrip.Font = fontLoader.ContentText;
                    break;
            }

            foreach (Control childControl in control.Controls)
            {
                Style(childControl);
            }
        }
    }
}