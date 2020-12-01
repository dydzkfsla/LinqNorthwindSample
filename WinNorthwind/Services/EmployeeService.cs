using NorthwindDAC;
using NorthwindVO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinNorthwind.Services
{
    class EmployeeService
    {
        public bool RegisterEmployee(EmployeeVO vo)
        {
            EmployeeDAC dac = new EmployeeDAC();
            return dac.RegisterEmployee(vo);
        }

        public List<EmployeeVO> GetAllEmployee()
        {
            EmployeeDAC dac = new EmployeeDAC();
            return dac.GetAllEmployee();
        }

        public EmployeeVO GetEmployeeInfo(int empID)
        {
            EmployeeDAC dac = new EmployeeDAC();
            return dac.GetEmployeeInfo(empID);
        }
    }
}
