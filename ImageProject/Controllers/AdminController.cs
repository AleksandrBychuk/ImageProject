using ImageProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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

        public IActionResult Index()
        {
            return View(_userManager.Users.ToList());
        }

        public IActionResult Users()
        {
            return PartialView("_Users", _userManager.Users.ToList());
        }

        public IActionResult Roles()
        {
            return View(_roleManager.Roles.ToList());
        }

        public async Task<IActionResult> BannedUsers()
        {
            var users = await _userManager.GetUsersInRoleAsync("banned");
            return View(users);
        }

        //НАПОМИНАЛКА: Сделать уведомление в хедере лайота для всех действий, в drag and drop добавить добавлеение через кнопку; Забаненный юзеры в общем списке, что с ними делать? Решить!
        [HttpPost]
        public async Task<IActionResult> BanUser(string id)
        {
            User user = await _userManager.FindByIdAsync(id);
            IdentityRole role = await _roleManager.FindByNameAsync("banned");
            await _userManager.AddToRoleAsync(user, role.Name);
            return RedirectToAction("BannedUsers", "Admin");
        }

        [HttpPost]
        public async Task<IActionResult> UnBanUser(string id)
        {
            User user = await _userManager.FindByIdAsync(id);
            IdentityRole role = await _roleManager.FindByNameAsync("banned");
            await _userManager.RemoveFromRoleAsync(user, role.Name);
            return RedirectToAction("BannedUsers", "Admin");
        }
    }
}
