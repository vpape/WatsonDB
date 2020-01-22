using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WatsonDB.Models
{
    public class UserRoleViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public bool isSelected { get; set; }
    }
}