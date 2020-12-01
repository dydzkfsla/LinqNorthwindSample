using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NorthwindVO
{
    public class EmployeeVO
    {
        public int EmployeeID { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Title { get; set; }
        public string BirthDate { get; set; }
        public string HireDate { get; set; }
        public string HomePhone { get; set; }
        public byte[] Photo { get; set; }
        public string Notes { get; set; }
    }
}
