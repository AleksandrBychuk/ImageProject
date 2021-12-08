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
        private static ConcurrentDictionary<string, ConcurrentDictionary<int, byte>> _imagePairs = new();
        private IQueryable<UserImage> _userImages;
        public UserController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, ApplicationContext applicationContext)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _applicationContext = applicationContext;
            _userImages = _applicationContext.UserImages.Include(_ => _.UserOwner).Include(_ => _.Coords).Include(_ => _.СonstituentСolors);
        }

        [HttpGet("User/{name}")] //НАПОМИНАЛКА: Разобраться полностью с дизайном в последнюю очередь! Не забыть!
        public async Task<IActionResult> Index(string name) //НАПОМИНАЛКА: Переделать получение картинок под связи в БД(читать metanit про менеджеров)!!! Заменить поиск юзеров где нужно под Finbynameasync...
        {
            User user = await _applicationContext.Users
                .Include(_ => _.UserImages).ThenInclude(_ => _.СonstituentСolors)
                .Include(_ => _.UserImages).ThenInclude(_ => _.Coords)
                .SingleAsync(_ => _.UserName == name);

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

        [HttpPost]
        public void GetSelectedImage(int selectedImage)
        {
            _imagePairs.AddOrUpdate(User.Identity.Name, new ConcurrentDictionary<int, byte>(), (k, v) => v);
            if (!_imagePairs[User.Identity.Name].TryAdd(selectedImage, 0))
                _imagePairs[User.Identity.Name].Remove(selectedImage, out _);
        }

        [HttpGet("Delete")]
        public async Task<IActionResult> Delete()
        {
            if (_imagePairs[User.Identity.Name].Values.Count != 0)
            {
                foreach (var deleteItem in _imagePairs[User.Identity.Name].Keys)
                {
                    var result = await _userImages.FirstOrDefaultAsync(_ => _.Id == deleteItem);
                    _applicationContext.UserImages.Remove(result);
                }
                _imagePairs[User.Identity.Name].Clear();
                await _applicationContext.SaveChangesAsync();
            }
            return RedirectToAction("Euc", "User", new { userName = User.Identity.Name });
        }

        [HttpGet("Edit")]
        public async Task<IActionResult> Edit()
        {
            UserImage image = new();
            if (_imagePairs[User.Identity.Name].Values.Count == 1)
            {
                foreach (var item in _imagePairs[User.Identity.Name].Keys)
                {
                    image = await _userImages.FirstOrDefaultAsync(_ => _.Id == item);
                }
                _imagePairs[User.Identity.Name].Clear();
                return View(image);
            } else
                return RedirectToAction("Euc", "User", new { userName = User.Identity.Name });
        }

        [HttpPost("Edit")]
        public async Task<IActionResult> Edit(UserImage selectedImage, ImageCoord imageCoord)
        {
            UserImage editImage = await _userImages.FirstOrDefaultAsync(_ => _.Id == selectedImage.Id);
            editImage.Coords.LatitudeDegree = imageCoord.LatitudeDegree;
            editImage.Coords.LatitudeMinute = imageCoord.LatitudeMinute;
            editImage.Coords.LatitudeSecond = imageCoord.LatitudeSecond;
            editImage.Coords.LongitudeDegree = imageCoord.LongitudeDegree;
            editImage.Coords.LongitudeMinute = imageCoord.LongitudeMinute;
            editImage.Coords.LongitudeSecond = imageCoord.LongitudeSecond;
            editImage.Coords.Altitude = imageCoord.Altitude;
            _applicationContext.UserImages.Update(editImage);
            await _applicationContext.SaveChangesAsync();
            return RedirectToAction("Euc", "User", new { userName = User.Identity.Name });
        }
    }
}
