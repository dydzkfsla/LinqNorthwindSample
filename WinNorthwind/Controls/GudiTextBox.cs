using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinNorthwind.Controls
{
    public partial class GudiTextBox : TextBox
    {
        public enum InputType { Required, NonRequired }

        public InputType TextboxType 
        { 
            get; 
            set; 
        }
        public GudiTextBox()
        {
            InitializeComponent();

            TextboxType = InputType.NonRequired;
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
        }
    }
}
