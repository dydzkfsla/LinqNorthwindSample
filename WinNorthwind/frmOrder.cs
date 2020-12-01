using NorthwindVO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinNorthwind.Services;
using WinNorthwind.Utils;

namespace WinNorthwind
{
    public partial class frmOrder : Form
    {
        List<ProductInfoVO> prodAllList = null; //전체 제품목록
        List<OrderDetailVO> cartList = null;    //장바구니 목록

        public frmOrder()
        {
            InitializeComponent();
        }

        private void frmOrder_Load(object sender, EventArgs e)
        {
            //코드 데이터들을 조회해서 콤보박스 바인딩
            string[] gubun = { "Customer", "Employee", "Category", "Shipper" };

            CommonService service = new CommonService();
            List<ComboItemVO> allList = service.GetCodeInfoByCodeTypes(gubun);

            #region 주문 신규등록 탭
            CommonUtil.ComboBinding(cboCustomer, allList, "Customer", true, "선택");
            CommonUtil.ComboBinding(cboEmployee, allList, "Employee");
            CommonUtil.ComboBinding(cboCategory, allList, "Category");

            //배송희망일(납기희망일) 7일 후로 셋팅
            dtpRequiredDate.Value = DateTime.Now.AddDays(7);

            //장바구니 데이터그리드뷰의 항목을 셋팅
            CommonUtil.SetInitGridView(dgvCart);
            CommonUtil.AddGridTextColumn(dgvCart, "카테고리", "CategoryName", 150);
            CommonUtil.AddGridTextColumn(dgvCart, "제품ID", "ProductID", visibility: false);
            CommonUtil.AddGridTextColumn(dgvCart, "제품명", "ProductName", 310);
            CommonUtil.AddGridTextColumn(dgvCart, "제품단가", "UnitPrice");
            CommonUtil.AddGridTextColumn(dgvCart, "주문수량", "Quantity");
            #endregion

            #region 주문 조회/관리 탭
            CommonUtil.ComboBinding(cboCustomer2, allList, "Customer");
            CommonUtil.ComboBinding(cboEmployee2, allList, "Employee");
            CommonUtil.ComboBinding(cboShipper, allList, "Shipper");

            //주문일자 검색조건
            period1.ChangedPeriod += Period1_ChangedPeriod;
            //period1.dtFrom = DateTime.Now.AddDays(-3).ToShortDateString();
            //period1.dtTo = DateTime.Now.ToShortDateString();

            //주문목록, 주문상세목록 데이터그리드뷰의 항목을 셋팅
            CommonUtil.SetInitGridView(dgvOrder);
            CommonUtil.AddGridTextColumn(dgvOrder, "주문ID", "OrderID");
            CommonUtil.AddGridTextColumn(dgvOrder, "거래처명", "CompanyName", 200);
            CommonUtil.AddGridTextColumn(dgvOrder, "직원명", "EmployeeName", 200);
            CommonUtil.AddGridTextColumn(dgvOrder, "주문일", "OrderDate");
            CommonUtil.AddGridTextColumn(dgvOrder, "요청일", "RequiredDate");
            CommonUtil.AddGridTextColumn(dgvOrder, "배송일", "ShippedDate");
            CommonUtil.AddGridTextColumn(dgvOrder, "배송업체", "ShipCompanyName", 150);
            CommonUtil.AddGridTextColumn(dgvOrder, "배송료", "Freight");            

            CommonUtil.SetInitGridView(dgvOrderDetail);
            CommonUtil.AddGridTextColumn(dgvOrderDetail, "카테고리", "CategoryName");
            CommonUtil.AddGridTextColumn(dgvOrderDetail, "제품명", "ProductName", 250);
            CommonUtil.AddGridTextColumn(dgvOrderDetail, "제품단가", "UnitPrice");
            CommonUtil.AddGridTextColumn(dgvOrderDetail, "주문수량", "Quantity");
            #endregion
        }

        private void Period1_ChangedPeriod(object sender, EventArgs e)
        {
            btnSearch.PerformClick();
        }

        #region 주문 신규등록 탭
        private void cboCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            //카테고리를 실제적으로 선택했을때만 아랫코드 실행 (데이터바인딩으로 인한 이벤트는 무시) 
            if (cboCategory.SelectedIndex < 1) return;

            //제품 전체 목록이 없는 경우 제품정보를 조회
            if (prodAllList == null)
            {
                OrderService service = new OrderService();
                prodAllList = service.GetProductAllList();
            }

            //제품 전체 목록에서 선택된 카테고리에 해당하는 제품만 콤보바인딩
            List<ComboItemVO> list = (from product in prodAllList
                                      where product.CategoryID == Convert.ToInt32(cboCategory.SelectedValue)
                                      select new ComboItemVO 
                                      { Code = product.ProductID.ToString(), 
                                        CodeName = product.ProductName, 
                                        Gubun = "Product" 
                                      }).ToList();

            CommonUtil.ComboBinding(cboProducts, list, "Product");
        }

        private void cboProducts_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboProducts.SelectedIndex > 0)
            {
                //선택된 제품ID를 전체 제품목록에서 찾아서 제품정보를 컨트롤에 바인딩
                int proId = Convert.ToInt32(cboProducts.SelectedValue);
                List<ProductInfoVO> selProdList = (from prod in prodAllList
                                                   where prod.ProductID == proId
                                                   select prod).ToList();
                if (selProdList.Count > 0)
                {
                    txtQuantityPerUnit.Text = selProdList[0].QuantityPerUnit;
                    txtUnitPrice.Text = selProdList[0].UnitPrice.ToString();
                    nuQuantity.Value = selProdList[0].UnitsOnOrder;
                    nuQuantity.Increment = (selProdList[0].UnitsOnOrder > 0) ? selProdList[0].UnitsOnOrder : 1;
                }
            }
            else
            {
                txtQuantityPerUnit.Text = txtUnitPrice.Text = "";
                nuQuantity.Value = 0;
            }
        }

        private void btnCartAdd_Click(object sender, EventArgs e)
        {
            //유효성검사(제품선택을 안했거나 주문수량이 0인경우)
            if (cboProducts.SelectedIndex < 1 || nuQuantity.Value < 1)
            {
                MessageBox.Show("장바구니에 추가할 제품을 선택하여 주십시오.");
                return;
            }

            //선택된 제품으로 OrderDetailVO 객체를 만들어서 cartList에 추가
            //이미 장바구니에 추가가된 경우 => 리스트에서 그 제품을 찾아서 수량을 증가
            //장바구니 없는 경우 => 리스트에 새로운 VO를 추가
            if (cartList == null)
                cartList = new List<OrderDetailVO>();

            int proID = Convert.ToInt32(cboProducts.SelectedValue);

            int idx = cartList.FindIndex(p => p.ProductID == proID); //못찾으면 -1
            if (idx > -1)
            {
                cartList[idx].Quantity += (int)nuQuantity.Value;
            }
            else
            {
                OrderDetailVO newItem = new OrderDetailVO();
                newItem.CategoryName = cboCategory.Text;
                newItem.ProductID = proID;
                newItem.ProductName = cboProducts.Text;
                newItem.Quantity = (int)nuQuantity.Value;
                newItem.UnitPrice = Convert.ToDecimal(txtUnitPrice.Text);

                cartList.Add(newItem);
            }
            dgvCart.DataSource = null;
            dgvCart.DataSource = cartList;
            dgvCart.ClearSelection();

            cboProducts.SelectedIndex = 0;
        }

        private void btnCartDel_Click(object sender, EventArgs e)
        {
            //유효성체크(삭제할 제품을 선택했는지 확인)
            if (dgvCart.SelectedRows.Count < 1)
            {
                MessageBox.Show("삭제할 제품을 선택하여 주십시오.");
                return;
            }

            //그리드뷰에서 선택된 제품을 장바구니에서 찾아서 리스트에서 삭제하고 
            //다시 리스트를 그리드뷰에 바인딩
            int proID = Convert.ToInt32(dgvCart.SelectedRows[0].Cells[1].Value);
            int idx = cartList.FindIndex(p => p.ProductID == proID);
            if (idx > -1)
            {
                cartList.RemoveAt(idx);
                dgvCart.DataSource = null;
                dgvCart.DataSource = cartList;
            }
        }

        private void btnOrder_Click(object sender, EventArgs e)
        {
            //유효성체크
            if (dgvCart.Rows.Count < 1)
            {
                MessageBox.Show("주문할 제품을 선택하여 주십시오.");
                return;
            }

            if (cboCustomer.SelectedIndex < 1 || cboEmployee.SelectedIndex < 1)
            {
                MessageBox.Show("주문정보를 선택하여 주십시오.");
                return;
            }

            //Order에 추가, IDentity조회해서 Order Details에 여러건 추가 
            try
            {
                OrderVO order = new OrderVO
                {
                    CustomerID = cboCustomer.SelectedValue.ToString(),
                    EmployeeID = Convert.ToInt32(cboEmployee.SelectedValue),
                    RequiredDate = dtpRequiredDate.Value.ToShortDateString()
                };

                OrderService service = new OrderService();
                bool bResult = service.RegisterOrder(order, cartList);
                if (bResult)
                {
                    cartList.Clear();
                    dgvCart.DataSource = null;

                    MessageBox.Show("주문이 완료되었습니다.");
                }
                else
                    MessageBox.Show("주문 처리 중 오류가 발생했습니다. 다시 시도하여 주십시오.");
            }
            catch(Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }
        #endregion

        private void btnSearch_Click(object sender, EventArgs e)
        {
            string custID = cboCustomer2.SelectedIndex == 0 ? string.Empty :
                cboCustomer2.SelectedValue.ToString();

            int empID = cboEmployee2.SelectedIndex == 0 ? 0 : Convert.ToInt32(cboEmployee2.SelectedValue);

            string dtFrom = period1.dtFrom;
            string dtTo = period1.dtTo;

            OrderService service = new OrderService();
            List<OrderVO> list = service.GetOrderSearchList(custID, empID, dtFrom, dtTo);
            dgvOrder.DataSource = list;
        }

        private void dgvOrder_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dgvOrder.SelectedRows.Count < 1) return;

            //더블클릭해서 선택한 주문ID
            int orderID = Convert.ToInt32(dgvOrder[0, e.RowIndex].Value);

            //주문ID에 해당하는 주문상세내역
            OrderService service = new OrderService();
            dgvOrderDetail.DataSource = service.GetOrderDetailList(orderID);
            dgvOrderDetail.ClearSelection();

            //Ouput 파라미터 사용 예
            lblAmount.Text = service.GetOrderAmount(orderID).ToString();

            //주문목록에서 주문ID를 찾아서 상세정보를 컨트롤에 바인딩
            List<OrderVO> list = (List<OrderVO>)dgvOrder.DataSource;
            OrderVO order = list.Find(o => o.OrderID == orderID);
            if (order != null)
            {
                lblOrderID.Text = order.OrderID.ToString();
                lblCustomerName.Text = order.CompanyName;
                lblEmployeeName.Text = order.EmployeeName;
                lblOrderDate.Text = order.OrderDate;
                lblRequiredDate.Text = order.RequiredDate;

                //배송정보가 이미 등록된 경우, 확인하기 위해서 배송정보를 컨트롤에 바인딩
                if (order.ShipVia != null)
                    cboShipper.SelectedValue = order.ShipVia.ToString();
                else
                    cboShipper.SelectedIndex = 0;

                dtpShippedDate.Value = string.IsNullOrEmpty(order.ShippedDate) ? DateTime.Now :
                    Convert.ToDateTime(order.ShippedDate);

                txtFreight.Text = order.Freight.ToString();

                if (order.ShipVia != null && txtFreight.Text.Length > 0)
                {
                    btnShip.Enabled = false;
                    btnDelete.Enabled = false;
                }
                else
                {
                    btnShip.Enabled = true;
                    btnDelete.Enabled = true;
                }
            }
        }

        private void btnShip_Click(object sender, EventArgs e)
        {
            //유효성체크
            if (cboShipper.SelectedIndex < 1 || txtFreight.Text.Trim().Length < 1)
            {
                MessageBox.Show("배송정보를 입력하세요.");
                return;
            }

            //선택된 주문ID의 배송정보를 UPDATE
            OrderVO order = new OrderVO
            {
                ShipVia = Convert.ToInt32(cboShipper.SelectedValue),
                ShippedDate = dtpShippedDate.Value.ToShortDateString(),
                Freight = Convert.ToDecimal(txtFreight.Text.Trim()),
                OrderID = int.Parse(lblOrderID.Text)
            };

            OrderService service = new OrderService();
            if (service.UpdateShipInfo(order))
            {
                MessageBox.Show("배송처리가 완료되었습니다.");
                btnSearch.PerformClick();
            }
            else
            {
                MessageBox.Show("배송처리중 오류가 발생했습니다. 다시 시도하여 주십시오.");
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("주문정보를 삭제하시겠습니까?", "삭제확인", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                OrderService serice = new OrderService();
                bool bFlag = serice.DeleteOrderInfo(Convert.ToInt32(lblOrderID.Text));
                if(bFlag)
                {
                    MessageBox.Show("주문이 성공적으로 삭제되었습니다.");
                    btnSearch.PerformClick();
                }
                else
                {
                    MessageBox.Show("주문삭제중 오류가 발생했습니다. 다시 시도하여 주십시오.");
                }
            }
        }
    }
}
