using NorthwindVO;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinNorthwind.Utils
{
    class CommonUtil
    {
        public static void ComboBinding(ComboBox cbo, List<ComboItemVO> list, string gubun, bool blankItem=true, string blankText="")
        {
            var codeList = (from item in list
                            where item.Gubun.Equals(gubun)
                            select item).ToList();

            if (blankItem)
            {
                ComboItemVO blank = new ComboItemVO 
                                        { Code = "", CodeName = blankText };
                codeList.Insert(0, blank);
            }
            cbo.DisplayMember = "CodeName";
            cbo.ValueMember = "Code";
            cbo.DataSource = codeList;
        }

        #region 데이터그리드뷰
        public static void SetInitGridView(DataGridView dgv)
        {
            dgv.AutoGenerateColumns = false;
            dgv.AllowUserToAddRows = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        }

        public static void AddGridTextColumn(
                            DataGridView dgv,
                            string headerText,
                            string dataPropertyName,
                            int colWidth = 100,
                            bool visibility = true,
                            DataGridViewContentAlignment textAlign = DataGridViewContentAlignment.MiddleLeft)
        {
            DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn();
            col.Name = dataPropertyName;
            col.HeaderText = headerText;
            col.DataPropertyName = dataPropertyName;
            col.Width = colWidth;
            col.DefaultCellStyle.Alignment = textAlign;
            col.Visible = visibility;
            col.ReadOnly = true;

            dgv.Columns.Add(col);
        }

        #endregion

        public static byte[] ImageToByte(Image img)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }

    }
}
