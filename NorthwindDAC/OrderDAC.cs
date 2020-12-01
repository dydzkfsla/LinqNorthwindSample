using NorthwindVO;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace NorthwindDAC
{
    public class OrderDAC : ConnectionAccess
    {
        string strConn;
        SqlConnection conn;

        public OrderDAC()
        {
            strConn = this.ConnectionString;
            conn = new SqlConnection(strConn);
            conn.Open();
        }

        //==================================================

        public List<ProductInfoVO> GetProductAllList()
        {
            string sql = "select ProductID, ProductName, CategoryID, QuantityPerUnit, UnitPrice, UnitsOnOrder from Products";
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                SqlDataReader reader = cmd.ExecuteReader();
                List<ProductInfoVO> list = Helper.DataReaderMapToList<ProductInfoVO>(reader);

                conn.Close();
                return list;
            }
        }        

        /// <summary>
        /// 신규 주문등록
        /// </summary>
        /// <param name="order">주문정보</param>
        /// <param name="ordDetails">주문상세내역</param>
        /// <returns>주문등록 성공여부</returns>
        public bool RegisterOrder(OrderVO order, List<OrderDetailVO> ordDetails)
        {
            using(SqlCommand cmd = new SqlCommand())
            {                
                SqlTransaction trans = conn.BeginTransaction();
             
                cmd.Connection = conn;
                cmd.Transaction = trans;
                try
                {
                    //Order 테이블에 1건 insert
                    cmd.CommandText = @"insert into Orders(CustomerID, EmployeeID, OrderDate, RequiredDate) values(@CustomerID, @EmployeeID, @OrderDate, @RequiredDate); select @@IDENTITY";
                   
                    cmd.Parameters.AddWithValue("@CustomerID", order.CustomerID);
                    cmd.Parameters.AddWithValue("@EmployeeID", order.EmployeeID);
                    cmd.Parameters.AddWithValue("@OrderDate", DateTime.Now.ToShortDateString());
                    cmd.Parameters.AddWithValue("@RequiredDate", order.RequiredDate);

                    int orderID = Convert.ToInt32(cmd.ExecuteScalar());

                    //Order Detail에 여러건 insert
                    cmd.CommandText = "insert into [Order Details] (OrderID, ProductID, UnitPrice, Quantity) values(@OrderID, @ProductID, @UnitPrice, @Quantity)";

                    cmd.Parameters.AddWithValue("@OrderID", orderID);
                    cmd.Parameters.Add("@ProductID", SqlDbType.Int);
                    cmd.Parameters.Add("@UnitPrice", SqlDbType.Money);
                    cmd.Parameters.Add("@Quantity", SqlDbType.SmallInt);

                    foreach (OrderDetailVO detail in ordDetails)
                    {
                        cmd.Parameters["@ProductID"].Value = detail.ProductID;
                        cmd.Parameters["@UnitPrice"].Value = detail.UnitPrice;
                        cmd.Parameters["@Quantity"].Value = detail.Quantity;

                        cmd.ExecuteNonQuery();
                    }

                    trans.Commit();
                    conn.Close();
                    return true;
                }
                catch(Exception err)
                {
                    string msg = err.Message;
                    trans.Rollback();
                    conn.Close();
                    return false;
                }
            }
        }

        /// <summary>
        /// 검색조건에 따른 주문정보 조회
        /// </summary>
        /// <param name="custID">거래처ID</param>
        /// <param name="empID">담당직원ID</param>
        /// <param name="dtFrom">주문일자 From</param>
        /// <param name="dtTo">주문일자 To</param>
        /// <returns>주문정보 목록</returns>
        public List<OrderVO> GetOrderSearchList(string custID, int empID, string dtFrom, string dtTo)
        {
            //StringBuilder sb = new StringBuilder();
            //sb.Append(@" select OrderID, O.CustomerID, O.EmployeeID, 
            //                    Convert(varchar(10), OrderDate, 23) OrderDate,
            //                    Convert(varchar(10), RequiredDate, 23) RequiredDate,
            //                    C.CompanyName,
            //                    concat(E.FirstName, ' ', E.LastName) EmployeeName,
            //                    Convert(varchar(10), ShippedDate, 23) ShippedDate,
            //                    S.CompanyName ShipCompanyName,
            //                    case when Freight = 0.00 then null else Freight end Freight,            ShipVia
            //            from Orders O inner join Customers C on o.CustomerID = C.CustomerID
            //                          inner join Employees E on o.EmployeeID = E.EmployeeID
            //                          left outer join Shippers S on o.ShipVia = S.ShipperID ");

            //sb.Append(" where O.OrderDate between cast(@dtFrom as datetime) and cast(@dtTo as datetime)");
            //sb.Append(" and O.CustomerID = ISNULL(@custID, O.CustomerID) ");
            //sb.Append(" and O.EmployeeID = ISNULL(@empID, O.EmployeeID) ");

            //if (!string.IsNullOrEmpty(custID))
            //    sb.Append(" and O.CustomerID = @custID");
            //if (empID > 0)
            //    sb.Append(" and O.EmployeeID = @empID");

            using (SqlCommand cmd = new SqlCommand())
            {
                //cmd.CommandText = sb.ToString();
                cmd.CommandText = "SP_GetOrderSearchList";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection = conn;

                cmd.Parameters.AddWithValue("@dtFrom", dtFrom);
                cmd.Parameters.AddWithValue("@dtTo", dtTo);
                cmd.Parameters.AddWithValue("@custID",
                    (string.IsNullOrEmpty(custID)) ? DBNull.Value : (object)custID);
                cmd.Parameters.AddWithValue("@empID", 
                    (empID == 0)? DBNull.Value : (object)empID);

                List<OrderVO> list = Helper.DataReaderMapToList<OrderVO>(cmd.ExecuteReader());
                conn.Close();
                return list;
            }
        }

        public List<OrderDetailVO> GetOrderDetailList(int orderID)
        {
            string sql = @"select OrderID, CategoryName, OD.ProductID, ProductName, OD.UnitPrice, Quantity
from [Order Details] OD inner join Products P on OD.ProductID = P.ProductID
      inner join Categories C on P.CategoryID = C.CategoryID
where OrderID = @orderID";

            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@orderID", orderID);

                List<OrderDetailVO> list = Helper.DataReaderMapToList<OrderDetailVO>(cmd.ExecuteReader());
                conn.Close();
                return list;
            }
        }

        public decimal GetOrderAmount(int orderID)
        {

            //--INPUT, OUTPUT 파라미터 모두 실행시 DAC 에서 프로시져로 전달해야한다.
            //--INPUT 파라미터는 선언과 동시에 값을 전달해야만 한다.
            //--OUTPUT 파라미터는 선언만 하고, 값은 주지 않고, 
            //--ExecuteNonQuery() 호출한 이후에 Value값을 조회할 수 있다.

            //--Return => 프로시져 처리결과를 반환할때 사용(1, 0이면 성공, -1 살패, -9:

            //--팀프로젝트 개발 중 프로시져를 써야 할 때
            //--1.버튼 1개에 여러개의 쿼리문을 동시에 실행해야할 때
            //--(프로시져 하나를 생성해서, 여러개의 쿼리문을 실행시킨다)
            //--2.여러개의 문장은 select, insert, update, delete 모두 섞어서 구현가능

            using (SqlCommand cmd = new SqlCommand("SP_GetOrderAmount", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@OrderID", orderID);

                SqlParameter outParam = new SqlParameter("@Amount", SqlDbType.Decimal);
                outParam.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(outParam);

                cmd.ExecuteNonQuery();

                return Convert.ToDecimal(outParam.Value);
            }
        }

        //public bool UpdateShipInfo(int shipId, string shipDate, decimal freight, int orderID)
        public bool UpdateShipInfo(OrderVO order)
        {
            string sql = @"update Orders 
                              set ShipVia=@shipId, ShippedDate=@shipDate, Freight=@freight
                            where OrderID = @orderID";
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@shipId", order.ShipVia);
                cmd.Parameters.AddWithValue("@shipDate", order.ShippedDate);
                cmd.Parameters.AddWithValue("@freight", order.Freight);
                cmd.Parameters.AddWithValue("@orderID", order.OrderID);

                int iRowAffect = cmd.ExecuteNonQuery();
                conn.Close();

                if (iRowAffect > 0)
                    return true;
                else
                    return false;
            }
        }

        public bool DeleteOrderInfo(int orderID)
        {      
            try
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = "SP_DeleteOrder";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = conn;
                    cmd.Parameters.AddWithValue("@orderID", orderID);

                    int iRowAffect = cmd.ExecuteNonQuery();

                    if (iRowAffect > 0)
                        return true;
                    else
                        return false;
                }
            }
            catch(Exception err)
            {
                return false;
            }
        }
    }
}
