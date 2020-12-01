using NorthwindVO;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NorthwindDAC
{
    public class EmployeeDAC : ConnectionAccess
    {
        string strConn;
        SqlConnection conn;

        public EmployeeDAC()
        {
            strConn = this.ConnectionString;
            conn = new SqlConnection(strConn);
            conn.Open();
        }

        //==================================================
        public bool RegisterEmployee(EmployeeVO vo)
        {
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.Connection = conn;
                cmd.CommandText = @"insert into Employees
(LastName, FirstName, Title, BirthDate, HireDate, HomePhone, Photo, Notes) values (@LastName, @FirstName, @Title, @BirthDate, @HireDate, @HomePhone, @Photo, @Notes)";

                cmd.Parameters.AddWithValue("@LastName", vo.LastName);
                cmd.Parameters.AddWithValue("@FirstName", vo.FirstName);
                cmd.Parameters.AddWithValue("@Title", vo.Title);
                cmd.Parameters.AddWithValue("@BirthDate", vo.BirthDate);
                cmd.Parameters.AddWithValue("@HireDate", vo.HireDate);
                cmd.Parameters.AddWithValue("@HomePhone", vo.HomePhone);
                cmd.Parameters.AddWithValue("@Photo", vo.Photo);
                cmd.Parameters.AddWithValue("@Notes", vo.Notes);

                int iRowAffect = cmd.ExecuteNonQuery();
                conn.Close();

                return iRowAffect > 0;
            }
        }

        public List<EmployeeVO> GetAllEmployee()
        {
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.Connection = conn;
                cmd.CommandText = @"select EmployeeID, LastName, FirstName, Title, 
       convert(char(10),BirthDate, 23) BirthDate, 
	   convert(char(10),HireDate, 23) HireDate, HomePhone
from Employees";
                List<EmployeeVO> list = Helper.DataReaderMapToList<EmployeeVO>(cmd.ExecuteReader());
                conn.Close();
                return list;
            }
        }

        public EmployeeVO GetEmployeeInfo(int empID)
        {
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.Connection = conn;
                cmd.CommandText = @"select EmployeeID, LastName, FirstName, Title, 
       convert(char(10),BirthDate, 23) BirthDate, 
	   convert(char(10),HireDate, 23) HireDate, HomePhone, Photo, Notes
from Employees
where EmployeeID = @EmployeeID";
                cmd.Parameters.AddWithValue("@EmployeeID", empID);
                List<EmployeeVO> list = Helper.DataReaderMapToList<EmployeeVO>(cmd.ExecuteReader());
                conn.Close();

                return (list != null) ? list[0] : null;
            }
        }
    }
}
