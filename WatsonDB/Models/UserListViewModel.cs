using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WatsonDB.Models
{
    public class UserListViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string RoleName { get; set; }
        public Employee emp { get; set; }
    }
}