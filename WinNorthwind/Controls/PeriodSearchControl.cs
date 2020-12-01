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
    public partial class PeriodSearchControl : UserControl
    {
        public event EventHandler ChangedPeriod;

        public string dtFrom 
        {
            get { return dtpFrom.Value.ToShortDateString(); }
            set { dtpFrom.Value = Convert.ToDateTime(value); } 
        }

        public string dtTo
        {
            get { return dtpTo.Value.ToShortDateString(); }
            set { dtpTo.Value = Convert.ToDateTime(value); }
        }

        public PeriodSearchControl()
        {
            InitializeComponent();
        }

        private void cboType_SelectedIndexChanged(object sender, EventArgs e)
        {
            dtpTo.Value = DateTime.Now;

            switch(cboType.Text)
            {
                case "3일":
                    dtpFrom.Value = DateTime.Now.AddDays(-3); break;
                case "1주일":
                    dtpFrom.Value = DateTime.Now.AddDays(-7); break;
                case "1개월":
                    dtpFrom.Value = DateTime.Now.AddMonths(-1); break;
                case "3개월":
                    dtpFrom.Value = DateTime.Now.AddMonths(-3); break;
                case "6개월":
                    dtpFrom.Value = DateTime.Now.AddMonths(-6); break;
            }
        }

        private void dtpTo_ValueChanged(object sender, EventArgs e)
        {
            if (ChangedPeriod != null) //등록된 이벤트핸들러가 있으면 널이 아님
            {
                ChangedPeriod(this, null);
            }
        }
    }
}
