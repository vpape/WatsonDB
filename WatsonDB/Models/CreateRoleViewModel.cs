using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WatsonDB.Models
{
    public class CreateRoleViewModel
    {
        [Required]
        public string UserRole { get; set; }
        public string RoleId { get; set; }

        [Required(AllowEmptyStrings = false)]
        [Display(Name = "RoleName")]
        public string Name { get; set; }
    }
}