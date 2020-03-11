using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WatsonDB.Models
{
    public class EmployeeAndUserListVM
    {
        public List<Employee> empList { get; set; }
        public Employee emp { get; set; }
        public IEnumerable<AspNetUser> userList { get; set; }
    }
}