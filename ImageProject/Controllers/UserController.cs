using ImageProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Concurrent;
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
        private static ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> _imagePairs = new();
        private IQueryable<UserImage> _userImages;
        public UserController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, ApplicationContext applicationContext)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _applicationContext = applicationContext;
            _userImages = _applicationContext.UserImages.Include(_ => _.UserOwner).Include(_ => _.Coords);
        }

        [HttpGet("User/{name}")] //НАПОМИНАЛКА: Разобраться полностью с дизайном в последнюю очередь! Не забыть!
        public async Task<IActionResult> Index(string name) //НАПОМИНАЛКА: Переделать получение картинок под связи в БД(читать metanit про менеджеров)!!! Заменить поиск юзеров где нужно под Finbynameasync...
        {
            User user = await _applicationContext.Users.Include(i => i.UserImages).ThenInclude(i => i.СonstituentСolors).SingleAsync(x => x.UserName == name);

            return View(user);
        }

        [HttpGet("Euc/{userName}")]
        public async Task<IActionResult> Euc(string userName)
        {
            if (User.Identity.Name != userName)
                return NotFound();
            var user = await _userManager.FindByNameAsync(userName);
            var model = await _applicationContext.UserImages.Include(u => u.UserOwner).Where(u => u.UserOwner == user).OrderByDescending(m => m.DateAdded).ToListAsync();
            return View(model);
        }

        //[HttpGet("{selectedImage}/{userOwner}")]
        //public void GetSelectedImage(string selectedImage, string userOwner)
        //{
        //    _imagePairs.AddOrUpdate(userOwner, new ConcurrentDictionary<string, byte>(), (k, v) => v);
        //    if (!_imagePairs[userOwner].TryAdd(selectedImage, 0))
        //        _imagePairs[userOwner].Remove(selectedImage, out _);
        //}

        public async Task<IActionResult> Delete()
        {
            foreach (var deleteItem in _imagePairs[User.Identity.Name].Values)
            {
                var result = await _userImages.FirstOrDefaultAsync(_ => _.Id == deleteItem);
                _applicationContext.UserImages.Remove(result);
            }
            _imagePairs[User.Identity.Name].Clear();
            await _applicationContext.SaveChangesAsync();
            return RedirectToAction("Euc", User.Identity.Name);
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
           
            return RedirectToAction("Euc", User.Identity.Name);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UserImage image)
        {

            return RedirectToAction("Euc", User.Identity.Name);
        }
    }
}
