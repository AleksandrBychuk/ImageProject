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
        private static List<UserImage> _memoryCache = new();
        public UserController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, ApplicationContext applicationContext)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _applicationContext = applicationContext;
            _userImages = _applicationContext.UserImages.Include(_ => _.UserOwner).Include(_ => _.Coords).Include(_ => _.СonstituentСolors);
        }

        [HttpGet("User/{name}")] // Check urls
        public async Task<IActionResult> Index(string name)
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
            if (_imagePairs.ContainsKey(User.Identity.Name))
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
            }
            return RedirectToAction("Euc", "User", new { userName = User.Identity.Name });
        }

        [HttpGet("Edit")]
        public async Task<IActionResult> Edit()
        {
            UserImage image = new();
            if (_imagePairs.ContainsKey(User.Identity.Name))
            {
                if (_imagePairs[User.Identity.Name].Values.Count == 1)
                {
                    foreach (var item in _imagePairs[User.Identity.Name].Keys)
                    {
                        image = await _userImages.FirstOrDefaultAsync(_ => _.Id == item);
                    }
                    _imagePairs[User.Identity.Name].Clear();
                    return View(image);
                }
                else return RedirectToAction("Euc", "User", new { userName = User.Identity.Name });
            }
            else return RedirectToAction("Euc", "User", new { userName = User.Identity.Name });
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

        [HttpGet("Compare")]
        public async Task<IActionResult> CompareImages(List<UserImage> result)
        {
            //List<UserImage> result = new();
            List<UserImage> resultNear = new();
            if (_imagePairs.ContainsKey(User.Identity.Name))
            {
                if (_imagePairs[User.Identity.Name].Values.Count != 0)
                {
                    _memoryCache = new();
                    foreach (var selectedItem in _imagePairs[User.Identity.Name].Keys)
                    {
                        var newItem = await _userImages.FirstOrDefaultAsync(_ => _.Id == selectedItem);
                        result.Add(newItem);
                    }

                    foreach (var item in await _userImages.ToListAsync())
                    {
                        if (!result.Contains(item))
                        {
                            foreach (var resultItem in result)
                            {
                                double dist = DistanceBetweenPlaces((double)item.Coords.LatitudeDegree, (double)item.Coords.LatitudeMinute, (double)item.Coords.LatitudeSecond,
                                    (double)item.Coords.LongitudeDegree, (double)item.Coords.LongitudeMinute, (double)item.Coords.LongitudeSecond,
                                    (double)resultItem.Coords.LatitudeDegree, (double)resultItem.Coords.LatitudeMinute, (double)resultItem.Coords.LatitudeSecond,
                                    (double)resultItem.Coords.LongitudeDegree, (double)resultItem.Coords.LongitudeMinute, (double)resultItem.Coords.LongitudeSecond);
                                if (dist < 100)
                                {
                                    resultNear.Add(item);
                                }
                            }
                        }
                    }
                    _imagePairs[User.Identity.Name].Clear();
                    _memoryCache = result;
                    ViewBag.NearImg = resultNear.Distinct();
                    //PartialView("_CompareDetails", resultNear.Distinct());
                    return View(result);
                }
                else if (_memoryCache.Count != 0)
                {
                    result = _memoryCache;
                    foreach (var item in await _userImages.ToListAsync())
                    {
                        if (!result.Any(_ => _.Id == item.Id))
                        {
                            foreach (var resultItem in result)
                            {
                                double dist = DistanceBetweenPlaces((double)item.Coords.LatitudeDegree, (double)item.Coords.LatitudeMinute, (double)item.Coords.LatitudeSecond,
                                    (double)item.Coords.LongitudeDegree, (double)item.Coords.LongitudeMinute, (double)item.Coords.LongitudeSecond,
                                    (double)resultItem.Coords.LatitudeDegree, (double)resultItem.Coords.LatitudeMinute, (double)resultItem.Coords.LatitudeSecond,
                                    (double)resultItem.Coords.LongitudeDegree, (double)resultItem.Coords.LongitudeMinute, (double)resultItem.Coords.LongitudeSecond);
                                if (dist < 100)
                                {
                                    resultNear.Add(item);
                                }
                            }
                        }
                    }
                    _imagePairs[User.Identity.Name].Clear();
                    _memoryCache = result;
                    ViewBag.NearImg = resultNear.Distinct();
                    //PartialView("_CompareDetails", resultNear.Distinct());
                    return View(result);
                }
                else
                {
                    return RedirectToAction("Euc", "User", new { userName = User.Identity.Name });
                }
            }
            else if (_memoryCache.Count != 0)
            {
                result = _memoryCache;
                foreach (var item in await _userImages.ToListAsync())
                {
                    if (!result.Any(_ => _.Id == item.Id))
                    {
                        foreach (var resultItem in result)
                        {
                            double dist = DistanceBetweenPlaces((double)item.Coords.LatitudeDegree, (double)item.Coords.LatitudeMinute, (double)item.Coords.LatitudeSecond,
                                (double)item.Coords.LongitudeDegree, (double)item.Coords.LongitudeMinute, (double)item.Coords.LongitudeSecond,
                                (double)resultItem.Coords.LatitudeDegree, (double)resultItem.Coords.LatitudeMinute, (double)resultItem.Coords.LatitudeSecond,
                                (double)resultItem.Coords.LongitudeDegree, (double)resultItem.Coords.LongitudeMinute, (double)resultItem.Coords.LongitudeSecond);
                            if (dist < 100)
                            {
                                resultNear.Add(item);
                            }
                        }
                    }
                }
                _imagePairs[User.Identity.Name].Clear();
                _memoryCache = result;
                ViewBag.NearImg = resultNear.Distinct();
                //PartialView("_CompareDetails", resultNear.Distinct());
                return View(result);
            }
            else
            {
                return RedirectToAction("Euc", "User", new { userName = User.Identity.Name });
            }
        }

        [HttpPost("Compare")]
        public async Task<IActionResult> CompareImages(int id)
        {
            var image = await _userImages.FirstOrDefaultAsync(_ => _.Id == id);
            _memoryCache.Add(image);
            return RedirectToAction("CompareImages", "User");
        }

        public static double DistanceBetweenPlaces(double LatitudeDegree, double LatitudeMinute, double LatitudeSecond, double LongitudeDegree, double LongitudeMinute, double LongitudeSecond,
            double LatitudeDegree2, double LatitudeMinute2, double LatitudeSecond2, double LongitudeDegree2, double LongitudeMinute2, double LongitudeSecond2)
        {
            double Lat1 = LatitudeDegree + LatitudeMinute / 60d + LatitudeSecond / 3600d;
            double Lon1 = LongitudeDegree + LongitudeMinute / 60d + LongitudeSecond / 3600d;
            double Lat2 = LatitudeDegree2 + LatitudeMinute2 / 60d + LatitudeSecond2 / 3600d;
            double Lon2 = LongitudeDegree2 + LongitudeMinute2 / 60d + LongitudeSecond2 / 3600d;
            double R = 6371; // km

            double sLat1 = Math.Sin(Radians(Lat1));
            double sLat2 = Math.Sin(Radians(Lat2));
            double cLat1 = Math.Cos(Radians(Lat1));
            double cLat2 = Math.Cos(Radians(Lat2));
            double cLon = Math.Cos(Radians(Lon1) - Radians(Lon2));

            double cosD = sLat1 * sLat2 + cLat1 * cLat2 * cLon;

            double d = Math.Acos(cosD);

            double dist = R * d;

            return dist;
        }

        public static double Radians(double x)
        {
            return x * Math.PI / 180;
        }
    }
}
