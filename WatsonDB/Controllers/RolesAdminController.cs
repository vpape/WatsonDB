using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Data.Entity;
using System;
using System.Diagnostics;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Data.Entity.Validation;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Text;
using System.Data;
using System.Net.Http;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;
using System.Web.Mvc;
using WatsonDB.Models;

namespace WatsonDB.Controllers
{
    //[Authorize(Roles = "Admin")]
    public class RolesAdminController : Controller
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<ApplicationUser> userManager;
        private ApplicationDbContext context = new ApplicationDbContext();

        public RolesAdminController()
        {

        }


        public RolesAdminController(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;
        }


        //====================================
        // GET: Index 
        //==================================== 

        //public ActionResult Index()
        //{
        //    return View(roleManager.Roles);

        //}

        public ActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                if (!isAdminUser())
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }

            var roles = context.Roles.ToList();
            return View(roles);

        }

        public bool isAdminUser()
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = User.Identity;
                ApplicationDbContext context = new ApplicationDbContext();
                var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
                var s = UserManager.GetRoles(user.GetUserId());
                if (s[0].ToString() == "Admin")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        // GET: /Roles/Details/5
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var role = await roleManager.FindByIdAsync(id);

            // Get the list of Users in this Role
            var users = new List<ApplicationUser>();

            foreach (var user in userManager.Users.ToList())
            {
                if (await userManager.IsInRoleAsync(user.Id, role.Name))
                {
                    users.Add(user);
                }
            }

            ViewBag.Users = users;
            ViewBag.UserCount = users.Count();
            return View(role);
        }

        //====================================
        // GET: CreateRole 
        //====================================  
        [HttpGet]
        public ActionResult CreateRole()
        {
            return View();
        }

        //====================================
        // Post: CreateRole 
        //==================================== 
        [HttpPost]
        public async Task<ActionResult> CreateRole(CreateRoleViewModel model)
        {
            if (ModelState.IsValid)
            {
                IdentityRole Role = new IdentityRole
                {
                    Name = model.UserRole
                };

                IdentityResult result = await roleManager.CreateAsync(Role);

                if (result.Succeeded)
                {
                    return RedirectToAction("ListRoles", "RolesAdmin");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, result.Errors.First());
                        //var errors = string.Join(",", result.Errors);
                        //ModelState.AddModelError("", errors);
                    }
                    return View(model);
                }

            }

            //if (!roleresult.Succeeded)
            //{
            //    ModelState.AddModelError("", roleresult.Errors.First());
            //    return View();
            //}
            //return RedirectToAction("Index");

            return View(model);
        }

        //====================================
        // GET: ListRoles 
        //==================================== 
        [HttpGet]
        public ActionResult ListRoles()
        {
            var roles = roleManager.Roles;

            return View(roles);
        }

        //====================================
        // GET: EditRole 
        //==================================== 
        [HttpGet]
        public async Task<ActionResult> EditRole(string RoleId)
        {
            if (RoleId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var role = await roleManager.FindByIdAsync(RoleId);
            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with Id = {RoleId} cannot be found";
                return View("NotFound");
                //return HttpNotFound();
            }

            EditRoleViewModel model = new EditRoleViewModel
            {
                RoleId = role.Id,
                UserRole = role.Name
            };

            foreach (var user in userManager.Users)
            {
                if (await userManager.IsInRoleAsync(/*role.Id*/ user.Id, role.Name))
                {
                    model.Users.Add(user.UserName);
                }
            }

            return View(model);
        }

        //====================================
        // Post: EditRole 
        //==================================== 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditRole([Bind(Include = "Name,RoleId")] EditRoleViewModel model)
        {
            if (ModelState.IsValid)
            {
                var role = await roleManager.FindByIdAsync(model.RoleId);

                if (role == null)
                {
                    ViewBag.ErrorMessage = $"Role with Id = {model.RoleId} cannot be found";
                    return View("NotFound");
                }
                else
                {
                    role.Name = model.UserRole;
                    var result = await roleManager.UpdateAsync(role);

                    if (result.Succeeded)
                    {
                        return RedirectToAction("ListRoles", "RolesAdmin");
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, result.Errors.First());

                    }
                    return View(model);
                }
            }
            return View(model);
        }

        //====================================
        // GET: EditUsersInRole 
        //==================================== 
        [HttpGet]
        public async Task<ActionResult> EditUsersInRole(string RoleId)
        {
            ViewBag.RoleId = RoleId;

            var role = await roleManager.FindByIdAsync(RoleId);
            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with Id = {RoleId} cannot be found";
                return View("NotFound");
                //return HttpNotFound();
            }

            var model = new List<UserRoleViewModel>();

            foreach (var user in userManager.Users)
            {
                var userRoleViewModel = new UserRoleViewModel
                {
                    UserId = user.Id,
                    UserName = user.UserName

                };

                if (await userManager.IsInRoleAsync(role.Id, role.Name))
                {
                    userRoleViewModel.isSelected = true;
                }
                else
                {
                    userRoleViewModel.isSelected = false;
                }

                model.Add(userRoleViewModel);
            }

            return View(model);
        }

        //====================================
        // Post: EditUsersInRole 
        //==================================== 
        [HttpPost]
        public async Task<ActionResult> EditUsersInRole(List<UserRoleViewModel> model, string RoleId)
        {
            var role = await roleManager.FindByIdAsync(RoleId);
            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with Id = {RoleId} cannot be found";
                return View("NotFound");
                //return HttpNotFound();
            }

            for (int i = 0; i < model.Count; i++)
            {
                var user = await userManager.FindByIdAsync(model[i].UserId);

                IdentityResult result = null;

                if (model[i].isSelected && !(await userManager.IsInRoleAsync(role.Id, role.Name)))
                {
                    result = await userManager.AddToRoleAsync(role.Id, role.Name);

                }
                else if (!model[i].isSelected && await userManager.IsInRoleAsync(role.Id, role.Name))
                {
                    result = await userManager.RemoveFromRoleAsync(role.Id, role.Name);
                }
                else
                {
                    continue;
                }

                if (result.Succeeded)
                {
                    if (i < (model.Count - 1))
                        continue;
                    else
                        return RedirectToAction("EditRole", "RolesAdmin", new { Id = RoleId });
                }
            }

            return RedirectToAction("EditRole", "RolesAdmin", new { Id = RoleId });
        }

        //====================================
        // Get: AdminRoles/DeleteRole/RoleId
        //==================================== 
        public async Task<ActionResult> DeleteRole(string RoleId)
        {
            if (RoleId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var role = await roleManager.FindByIdAsync(RoleId);
            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with Id = {RoleId} cannot be found";
                return View("NotFound");
                //return HttpNotFound();
            }

            return View(role);

        }

        //====================================
        // Post: AdminRoles/DeleteRole/RoleId
        //==================================== 
        [HttpPost, ActionName("DeleteRole")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteRoleConfirmed(string RoleId, string User)
        {
            if (RoleId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var role = await roleManager.FindByIdAsync(RoleId);
            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with Id = {RoleId} cannot be found";
                return View("NotFound");
                //return HttpNotFound();
            }

            IdentityResult result;
            if (User != null)
            {
                result = await roleManager.DeleteAsync(role);
            }
            else
            {
                result = await roleManager.DeleteAsync(role);
            }

            if (result.Succeeded)
            {
                return RedirectToAction("ListRoles", "RolesAdmin");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, result.Errors.First());

            }
            return View("Index", "RolesAdmin");
        }
    }
}