using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WatsonDB.Models
{
    public class EditRoleViewModel
    {
        public EditRoleViewModel()
        {
            Users = new List<string>();
        }

        public string RoleId { get; set; }

        //[Required(ErrorMessage = "User Role is required")]
        [Required(AllowEmptyStrings = false)]
        [Display(Name = "UserRole")]
        public string UserRole { get; set; }

        public List<string> Users { get; set; }
    }
}