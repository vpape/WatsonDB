using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WatsonDB.Models
{
    public class UserListViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string UserRole { get; set; }
        public string EmployeeNumber { get; set; }
        public int Employee_id { get; set; }
        public Employee emp { get; set; }
        public List<AspNetUser> user { get; set; }
    }
}