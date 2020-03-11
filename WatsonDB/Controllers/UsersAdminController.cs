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
    public class UsersAdminController : Controller
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<ApplicationUser> userManager;
        private ApplicationDbContext context = new ApplicationDbContext();

        public UsersAdminController()
        {

        }

        public UsersAdminController(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;
            context = new ApplicationDbContext();
        }


        //====================================
        // GET: Index 
        //==================================== 

        public ActionResult Index()
        {
            return View(userManager.Users);

        }

        //====================================
        // GET: AdminUsers/ListUsers 
        //==================================== 
        [HttpGet]
        public ActionResult ListUsers()
        {
            return View(userManager.Users);
        }

        //====================================
        // GET: AdminUsers/CreateUser 
        //====================================  
        [HttpGet]
        public async Task<ActionResult> CreateUser()
        {
            ViewBag.RoleId = new SelectList(await roleManager.Roles.ToListAsync(), "Name", "Name");

            return View();
        }

        //====================================
        // Post: AdminUsers/CreateUser
        //==================================== 
        [HttpPost]
        public async Task<ActionResult> CreateUser(RegisterViewModel model, params string[] selectedRoles)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser applicationUser = new ApplicationUser
                {
                    UserName = model.UserName,
                    Email = model.Email
                };

                IdentityResult identityResult = await userManager.CreateAsync(applicationUser, model.Password);

                if (identityResult.Succeeded)
                {
                    if (selectedRoles != null)
                    {
                        var result = await userManager.AddToRolesAsync(applicationUser.Id, selectedRoles);
                        if (!result.Succeeded)
                        {
                            ModelState.AddModelError(string.Empty, result.Errors.First());
                            ViewBag.RoleId = new SelectList(await roleManager.Roles.ToListAsync(), "Name", "Name");
                            return View();
                        }
                    }
                }
                else
                {
                    foreach (var error in identityResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, identityResult.Errors.First());
                        return View();
                    }
                }

                return RedirectToAction("Index");
            }
            ViewBag.RoleId = new SelectList(roleManager.Roles, "Name", "Name");

            return View(model);
        }

        //====================================
        // GET: AdminUsers/EditUser 
        //==================================== 
        [HttpGet]
        public async Task<ActionResult> EditUser(string Id)
        {
            if (Id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var user = await userManager.FindByIdAsync(Id);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {Id} cannot be found";
                return View("NotFound");
                //return HttpNotFound();
            }

            //var userClaims = await userManager.GetClaimsAsync(user.Id);
            var userRoles = await userManager.GetRolesAsync(user.Id);

            return View(new EditUserViewModel()
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                RolesList = roleManager.Roles.ToList().Select(x => new SelectListItem()
                {
                    Selected = userRoles.Contains(x.Name),
                    Text = x.Name,
                    Value = x.Name
                    //Claims = userClaims.Select(c => c.Value).ToList(),
                    //Roles = userRoles
                })
            });
        }


        //====================================
        // Post: AdminUsers/EditUser 
        //==================================== 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditUser([Bind(Include = "Id,UserName,Email")] EditUserViewModel model, params string[] selectedRole)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByIdAsync(model.Id);
                if (user == null)
                {
                    ViewBag.ErrorMessage = $"Role with Id = {model.Id} cannot be found";
                    return View("NotFound");
                }
                else
                {
                    user.Id = model.Id;
                    user.UserName = model.UserName;
                    user.Email = model.Email;

                    //return View(new EditUserViewModel()
                    //{
                    //    Id = model.Id,
                    //    Email = model.Email,
                    //    RolesList = RoleManager.Roles.ToList().Select(x => new SelectListItem()
                    //    {
                    //        Selected = userRoles.Contains(x.Name),
                    //        Text = x.Name,
                    //        Value = x.Name
                    //    })
                    //});

                    //var applicationUser = await userManager.FindByIdAsync(User.Identity.GetUserId());
                    var userRole = await userManager.GetRolesAsync(user.Id);

                    selectedRole = selectedRole ?? new string[] { };

                    var result = await userManager.AddToRolesAsync(user.Id, selectedRole.Except(userRole).ToArray<string>());

                    if (!result.Succeeded)
                    {
                        ModelState.AddModelError(string.Empty, result.Errors.First());
                        return View();
                    }

                    result = await userManager.RemoveFromRolesAsync(user.Id, userRole.Except(selectedRole).ToArray<string>());

                    if (!result.Succeeded)
                    {
                        ModelState.AddModelError(string.Empty, result.Errors.First());
                        return View();
                    }

                    if (result.Succeeded)
                    {
                        return RedirectToAction("ListUsers", "UsersAdmin");
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

        [HttpGet]
        public async Task<ActionResult> ManageUserRoles(string UserId)
        {
            ViewBag.UserId = UserId;

            if (UserId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var user = await userManager.FindByIdAsync(UserId);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {UserId} cannot be found";
                return View("NotFound");
                //return HttpNotFound();
            }

            var model = new List<UserRolesViewModel>();

            foreach (var role in roleManager.Roles)
            {
                var UserRolesViewModel = new UserRolesViewModel
                {
                    RoleId = role.Id,
                    UserRole = role.Name
                };

                if (await userManager.IsInRoleAsync(user.Id, role.Name))
                {
                    UserRolesViewModel.isSelected = true;
                }
                else
                {
                    UserRolesViewModel.isSelected = false;
                }

                model.Add(UserRolesViewModel);

            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ManageUserRoles(List<UserRolesViewModel> model, string UserId, params string[] selectedRoles)
        {

            var user = await userManager.FindByIdAsync(UserId);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"Role with Id = {UserId} cannot be found";
                return View("NotFound");
            }

            var roles = await userManager.GetRolesAsync(user.Id);

            selectedRoles = selectedRoles ?? new string[] { };

            var result = await userManager.AddToRolesAsync(user.Id, selectedRoles.Except(roles).ToArray<string>());

            for (int i = 0; i < model.Count;)
            {
                if (model[i].isSelected && await userManager.IsInRoleAsync(user.Id, user.UserRole))
                {
                    result = await userManager.RemoveFromRolesAsync(user.Id, user.UserRole);

                }
                else if (!model[i].isSelected && await userManager.IsInRoleAsync(user.Id, user.UserRole))
                {
                    result = await userManager.RemoveFromRoleAsync(user.Id, user.UserRole);
                }

                if (!result.Succeeded)
                {
                    ModelState.AddModelError(string.Empty, "Cannot remove user existing roles");
                    return View(model);
                }
                //result = await userManager.RemoveFromRolesAsync(user.Id, roles.Except(selectedRoles).ToArray<string>());

                //result = await userManager.AddToRoleAsync(user.Id model.Where(x => x.isSelected).Select(y => y.UserRole));
                var Result = await userManager.AddToRolesAsync(user.Id, selectedRoles.Except(roles).ToArray<string>());
                if (!result.Succeeded)
                {
                    ModelState.AddModelError(string.Empty, "Cannot add selected roles to user");
                    return View(model);
                }
                return RedirectToAction("EditUser", "UsersAdmin", new { Id = UserId });
            }

            return View(model);
        }

        //====================================
        // Get: AdminUsers/DeleteUser/UserId
        //==================================== 
        public async Task<ActionResult> DeleteUser(string UserId)
        {
            if (UserId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var user = await userManager.FindByIdAsync(UserId);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"Role with Id = {UserId} cannot be found";
                return View("NotFound");
                //return HttpNotFound();
            }
            //else
            //{
            //    var result = await userManager.DeleteAsync(user);

            //    if (result.Succeeded)
            //    {
            //        return RedirectToAction("ListUsers");
            //    }
            //    foreach(var error in result.Errors)
            //    {
            //        ModelState.AddModelError(string.Empty, result.Errors.First());
            //    }

            //    return View("ListUsers");
            //}

            return View(user);
        }

        //====================================
        // Post: AdminUsers/DeleteUser/UserId
        //==================================== 
        [HttpPost, ActionName("DeleteUser")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteUserConfirmed(string UserId)
        {
            if (UserId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var user = await userManager.FindByIdAsync(UserId);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"Role with Id = {UserId} cannot be found";
                return View("NotFound");
                //return HttpNotFound();
            }

            var result = await userManager.DeleteAsync(user);
            if (result != null)
            {
                result = await userManager.DeleteAsync(user);
            }
            else
            {
                result = await userManager.DeleteAsync(user);
            }

            if (result.Succeeded)
            {
                return RedirectToAction("ListUsers");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, result.Errors.First());

            }
            return View();
        }
    }
}