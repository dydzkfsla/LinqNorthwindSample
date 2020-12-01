using NorthwindDAC;
using NorthwindVO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinNorthwind.Services
{
    class OrderService
    {
        public List<ProductInfoVO> GetProductAllList()
        {
            OrderDAC dac = new OrderDAC();
            return dac.GetProductAllList();
        }

        public bool RegisterOrder(OrderVO order, List<OrderDetailVO> ordDetail)
        {
            OrderDAC dac = new OrderDAC();
            return dac.RegisterOrder(order, ordDetail);
        }

        public List<OrderVO> GetOrderSearchList(string custID, int empID, string dtFrom, string dtTo)
        {
            OrderDAC dac = new OrderDAC();
            return dac.GetOrderSearchList(custID, empID, dtFrom, dtTo);
        }

        public List<OrderDetailVO> GetOrderDetailList(int orderID)
        {
            OrderDAC dac = new OrderDAC();
            return dac.GetOrderDetailList(orderID);
        }

        public bool UpdateShipInfo(OrderVO order)
        {
            OrderDAC dac = new OrderDAC();
            return dac.UpdateShipInfo(order);
        }

        public bool DeleteOrderInfo(int orderID)
        {
            OrderDAC dac = new OrderDAC();
            return dac.DeleteOrderInfo(orderID);
        }

        public decimal GetOrderAmount(int orderID)
        {
            OrderDAC dac = new OrderDAC();
            return dac.GetOrderAmount(orderID);
        }
    }
}
