using ImageProject.Models;
using ImageProject.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImageProject.Controllers
{
    [Authorize(Roles = "admin")]
    public class AdminController : Controller
    {
        UserManager<User> _userManager;
        RoleManager<IdentityRole> _roleManager;

        public AdminController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _userManager.GetUsersInRoleAsync("user"));
        }

        public async  Task<IActionResult> Roles()
        {
            return View(await _roleManager.Roles.ToListAsync());
        }

        public async Task<IActionResult> BannedUsers()
        {
            return View(await _userManager.GetUsersInRoleAsync("banned"));
        }

        //НАПОМИНАЛКА: Сделать уведомление в хедере лайота для всех действий, в drag and drop добавить добавлеение через кнопку;
        [HttpPost]
        public async Task<IActionResult> BanUser(string id)
        {
            User user = await _userManager.FindByIdAsync(id);
            await _userManager.RemoveFromRoleAsync(user, "user");
            await _userManager.AddToRoleAsync(user, "banned");
            return RedirectToAction("BannedUsers", "Admin");
        }

        [HttpPost]
        public async Task<IActionResult> UnBanUser(string id)
        {
            User user = await _userManager.FindByIdAsync(id);
            await _userManager.RemoveFromRoleAsync(user, "banned");
            await _userManager.AddToRoleAsync(user, "user");
            return RedirectToAction("BannedUsers", "Admin");
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                User user = await _userManager.FindByIdAsync(model.Id);
                if (user != null)
                {
                    user.Email = model.Email;
                    user.UserName = model.UserName;

                    var result = await _userManager.UpdateAsync(user);
                    if (result.Succeeded)
                        return RedirectToAction("Index");
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                }
            }

            return View();
        }
    }
}
