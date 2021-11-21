using ImageProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ImageProject.Controllers
{
    [Authorize(Roles = "user, admin")] //ВАЖНО: Многопользовательность async/await пихать везде!
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationContext _applicationContext;
        private readonly UserManager<User> _userManager;

        public HomeController(ILogger<HomeController> logger, ApplicationContext applicationContext, UserManager<User> userManager)
        {
            _logger = logger;
            _applicationContext = applicationContext;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public async Task<IActionResult> UploadFiles() // желательно переделать с List<IFormFile> files на принятие, но это не точно, гуглить
        {
            var files = Request.Form.Files;
            User user = await _userManager.GetUserAsync(User);
            foreach (var file in files)
            {
                byte[] imageData = null;
                using (var binaryReader = new BinaryReader(file.OpenReadStream()))
                {
                    imageData = binaryReader.ReadBytes((int)file.Length);
                }
                //await _applicationContext.UserImages.AddAsync(new UserImage { DateAdded = DateTime.Now, Image = imageData, UserOwner = user });
                var newImage = new UserImage { DateAdded = DateTime.Now, Image = imageData, UserOwner = user };
                var constColors = new List<СonstituentСolor>
                {
                    new СonstituentСolor { HexColor = "#FF0C13", ValueCount = 8, Image = newImage },
                    new СonstituentСolor { HexColor = "#68C350", ValueCount = 6, Image = newImage },
                    new СonstituentСolor { HexColor = "#6365EF", ValueCount = 2, Image = newImage }
                };
                
                newImage.СonstituentСolors = constColors;
                user.UserImages.Add(newImage);
                await _applicationContext.SaveChangesAsync();
            }
            await _applicationContext.SaveChangesAsync();
            string message = $"{files.Count} upload on server!";
            return Json(message);
        }
    }
}
