using NorthwindVO;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NorthwindDAC
{
    public class CommonDAC : ConnectionAccess, IDisposable
    {
        string strConn;
        SqlConnection conn;

        public CommonDAC()
        {
            strConn = this.ConnectionString;
            conn = new SqlConnection(strConn);
            conn.Open();
        }

        public void Dispose()
        {
            if (conn != null)
            {
                conn.Close();
            }
        }

        //-----------------------------------------------

        public List<ComboItemVO> GetCodeInfoByCodeTypes(string[] types)
        {            
            string type = string.Join("','", types);
            string sql = @"select Code, CodeName, Gubun 
from VW_NorthwindCode 
where Gubun in ('" + type + "')";

            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                SqlDataReader reader = cmd.ExecuteReader();
                List<ComboItemVO> list = Helper.DataReaderMapToList<ComboItemVO>(reader);
                return list;
            }
        }
    }
}
