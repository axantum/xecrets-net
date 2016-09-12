using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Forms.Style
{
    public class Styling
    {
        private static readonly Color _buttonBackgroundColor = Color.FromArgb(134, 185, 110);

        private static readonly Color _buttonForegroundColor = Color.White;

        private static readonly Color _buttonBorderColor = Color.FromArgb(106, 157, 83);

        private static readonly Color _buttonMouseOverColor = Color.FromArgb(232, 232, 232);

        public static Color WarningColor { get; } = Color.FromArgb(194, 145, 12);

        public static Color ErrorColor { get; } = Color.FromArgb(234, 95, 76);

        public static Color OkColor { get; } = Color.FromArgb(134, 185, 110);

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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "It is natural since it handles many types, but it's all linear in a switch statement, so not really complex.")]
        private void StyleInternal(Control control)
        {
            FontLoader fontLoader = New<FontLoader>();

            if (control is Form)
            {
                Form form = (Form)control;
                form.Font = fontLoader.ContentText ?? form.Font;
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
                    groupBox.Font = fontLoader.PromptText ?? groupBox.Font;
                    break;

                case "System.Windows.Forms.TextBox":
                    TextBox textBox = (TextBox)control;
                    textBox.Font = fontLoader.ContentText ?? textBox.Font;
                    break;

                case "System.Windows.Forms.CheckBox":
                    CheckBox checkBox = (CheckBox)control;
                    checkBox.Font = fontLoader.ContentText ?? checkBox.Font;
                    break;

                case "System.Windows.Forms.Panel":
                    Panel panel = (Panel)control;
                    panel.Font = fontLoader.ContentText ?? panel.Font;
                    break;

                case "System.Windows.Forms.MenuStrip":
                    MenuStrip menuStrip = (MenuStrip)control;
                    menuStrip.Font = fontLoader.ContentText ?? menuStrip.Font;
                    break;

                case "System.Windows.Forms.ContextMenuStrip":
                    ContextMenuStrip contextMenuStrip = (ContextMenuStrip)control;
                    contextMenuStrip.Font = fontLoader.ContentText ?? contextMenuStrip.Font;
                    break;

                case "System.Windows.Forms.ToolStrip":
                    ToolStrip toolStrip = (ToolStrip)control;
                    toolStrip.Font = fontLoader.ContentText ?? toolStrip.Font;
                    toolStrip.Renderer = new AxCryptToolStripProfessionalRenderer();
                    break;
            }

            foreach (Control childControl in control.Controls)
            {
                Style(childControl);
            }
        }

        public static void RestoreWindowWithFocus(Form form)
        {
            if (form == null)
            {
                throw new ArgumentNullException(nameof(form));
            }

            form.Show();
            form.WindowState = FormWindowState.Normal;
            form.Activate();
            form.Focus();
            form.BringToFront();

            foreach (Form owned in form.OwnedForms)
            {
                RestoreWindowWithFocus(owned);
            }
        }
    }
}