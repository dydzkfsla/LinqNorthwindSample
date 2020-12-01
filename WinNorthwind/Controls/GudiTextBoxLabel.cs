using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinNorthwind.Controls
{
    public partial class GudiTextBoxLabel : UserControl
    {
        public enum InputType { Required, NonRequired }

        private InputType type;
        public InputType TextboxType
        {
            get { return type; }
            set 
            { 
                type = value;
                if (type == InputType.Required)
                {
                    label6.Visible = true;
                    textBox1.BackColor = Color.AntiqueWhite;
                }
                else
                {
                    label6.Visible = false;
                    textBox1.BackColor = Color.White;
                }
            }
        }

        public GudiTextBoxLabel()
        {
            InitializeComponent();

            this.TextboxType = InputType.NonRequired;
        }
    }
}
