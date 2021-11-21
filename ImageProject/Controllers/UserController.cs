using ImageProject.Models;
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
    [Authorize(Roles = "user, admin")]
    public class UserController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationContext _applicationContext;
        public UserController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, ApplicationContext applicationContext)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _applicationContext = applicationContext;
        }

        [HttpGet("User/{name}")] //НАПОМИНАЛКА: Разобраться полностью с дизайном в последнюю очередь! Не забыть!
        public async Task<IActionResult> Index(string name) //НАПОМИНАЛКА: Переделать получение картинок под связи в БД(читать metanit про менеджеров)!!! Заменить поиск юзеров где нужно под Finbynameasync...
        {
            User user = await _applicationContext.Users.Include(i => i.UserImages).ThenInclude(i => i.СonstituentСolors).SingleAsync(x => x.UserName == name);

            return View(user);
        }
    }
}
