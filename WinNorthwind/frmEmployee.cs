using NorthwindVO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinNorthwind.Controls;
using WinNorthwind.Services;
using WinNorthwind.Utils;

namespace WinNorthwind
{
    public partial class frmEmployee : Form
    {
        List<EmployeeVO> allList;

        public frmEmployee()
        {
            InitializeComponent();
        }

        private void frmEmployee_Load(object sender, EventArgs e)
        {
            CommonUtil.SetInitGridView(dgvEmp);
            CommonUtil.AddGridTextColumn(dgvEmp, "사원번호", "EmployeeID");
            CommonUtil.AddGridTextColumn(dgvEmp, "사원명", "FirstName");
            CommonUtil.AddGridTextColumn(dgvEmp, "사원명", "LastName");
            CommonUtil.AddGridTextColumn(dgvEmp, "직책", "Title", 150);
            CommonUtil.AddGridTextColumn(dgvEmp, "출생일", "BirthDate");
            CommonUtil.AddGridTextColumn(dgvEmp, "입사일", "HireDate");
            CommonUtil.AddGridTextColumn(dgvEmp, "전화번호", "HomePhone", 130);

            InitControl();
            LoadData();
        }

        private void LoadData()
        {
            EmployeeService service = new EmployeeService();
            allList = service.GetAllEmployee();
            dgvEmp.DataSource = allList;
        }

        private void InitControl()
        {
            tabControl1.SelectedTab = tabPage2;
            //tabControl1.SelectedIndex = 1;

            dtpBirthDate.Value = DateTime.Now.AddYears(-25);
            picEmp.ImageLocation = "images/noimage.png";

            rdoName.Checked = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                picEmp.ImageLocation = openFileDialog1.FileName;
            }
        }

        private List<R> ValidateEmployee<T, R>(T pnl, Func<Control, R> func) where T : Panel
        {
            List<R> r = new List<R>();
            foreach (Control ctrl in pnl.Controls)
            {
                r.Add(func(ctrl));
            }
            return r;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //유효성체크
            List<string> errMsg = ValidateEmployee<TabPage, string>(tabPage1, (ctrl) =>
            {
               string err = string.Empty;
               if (ctrl is TextBox textBox)
               {                    
                   if (textBox.Text.Trim() == string.Empty)
                   {
                            err += $"- {textBox.Tag}을 입력해 주십시오.\n";
                   }
               }
                return err;
            });


            string Err = string.Empty;
            errMsg.ForEach((er) =>
            {
                Err += er;
            });

            if( !(Err == string.Empty))
            {
                MessageBox.Show(Err);
                return;
            }

            //VO 객체 생성
            EmployeeVO emp = new EmployeeVO
            {
                FirstName = txtFirstName.Text.Trim(),
                LastName = txtLastName.Text.Trim(),
                Title = txtTitle.Text.Trim(),
                BirthDate = dtpBirthDate.Value.ToShortDateString(),
                HireDate = dtpHireDate.Value.ToShortDateString(),
                HomePhone = txtHomePhone.Text.Trim(),
                Notes = txtNotes.Text.Trim()
            };
            //PictureBox 이미지를 byte[] 변환해서 저장
            FileStream fs = new FileStream(picEmp.ImageLocation, FileMode.Open, FileAccess.Read);
            byte[] bImage = new byte[fs.Length];
            fs.Read(bImage, 0, (int)fs.Length);
            emp.Photo = bImage;

            //Service에 VO 전달해서 DB에 저장
            EmployeeService service = new EmployeeService();
            bool bResult = service.RegisterEmployee(emp);

            //저장 결과 처리
            if (bResult)
                MessageBox.Show("사원이 등록되었습니다.");
            else
                MessageBox.Show("등록 중 오류가 발생하였습니다.");
        }


        private void button3_Click(object sender, EventArgs e)
        {
            if (! string.IsNullOrEmpty(txtKeyword.Text))
            {
                List<EmployeeVO> list;
                string query = txtKeyword.Text.Trim();

                if (rdoName.Checked)
                {
                    list = (from emp in allList
                            where emp.FirstName.Contains(query)||emp.LastName.Contains(query)
                            select emp).ToList();
                }
                else if(rdoTitle.Checked)
                {
                    list = (from emp in allList
                            where emp.Title.Contains(query)
                            select emp).ToList();
                }
                else //if (rdoHireDateYear.Checked)
                {
                    list = (from emp in allList
                            where emp.HireDate.Substring(0,4) == query
                            select emp).ToList();
                }

                dgvEmp.DataSource = list;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            txtKeyword.Text = "";
            rdoName.Checked = true;
            dgvEmp.DataSource = allList;
        }

        private void dgvEmp_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            int empID = Convert.ToInt32(dgvEmp[0, e.RowIndex].Value);

            EmployeeService service = new EmployeeService();
            EmployeeVO emp = service.GetEmployeeInfo(empID);

            lblEmployeeID.Text = emp.EmployeeID.ToString();
            txtFirstName2.Text = emp.FirstName.Trim();
            txtLastName2.Text = emp.LastName.Trim();
            txtTitle2.Text = emp.Title.Trim();
            dtpBirthDate2.Value = Convert.ToDateTime(emp.BirthDate);
            dtpHiredDate2.Value = Convert.ToDateTime(emp.HireDate);
            txtHomePhone2.Text = emp.HomePhone ?? "";
            txtNotes2.Text = emp.Notes.Trim();

            //byte[] => PictureBox image

            //MemoryStream ms = new MemoryStream(emp.Photo);            
            //picEmp2.Image = Image.FromStream(ms);

            //picEmp2.Image = new Bitmap(new MemoryStream(emp.Photo));

            //Image.FromStream(ms) 에서 오류가 발생
            //정상적인 이미지 포맷이 아니거나, 적절한 메타정보가 없어서 오류가 발생하는 경우 있다

            TypeConverter tc = TypeDescriptor.GetConverter(typeof(Bitmap));
            picEmp2.Image = (Bitmap)(tc.ConvertFrom(emp.Photo));
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                picEmp2.ImageLocation = openFileDialog1.FileName;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            //유효성체크
            if (lblEmployeeID.Text.Length < 1)
                return;

            //VO객체 서비스전달해서 DB 데이터 수정
            EmployeeVO emp = new EmployeeVO
            {
                EmployeeID = int.Parse(lblEmployeeID.Text),
                FirstName = txtFirstName2.Text,
                LastName = txtLastName2.Text,
                Title = txtTitle2.Text,
                BirthDate = dtpBirthDate2.Value.ToShortDateString(),
                HireDate = dtpHiredDate2.Value.ToShortDateString(),
                HomePhone = txtHomePhone2.Text,
                Photo = CommonUtil.ImageToByte(picEmp2.Image),
                Notes = txtNotes2.Text
            };

            //메세지, 그리드뷰 재조회
        }
    }
}
