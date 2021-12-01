using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using ImageProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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
            //WSMessageSubscribe messageModel = JsonConvert.DeserializeObject<WSMessageSubscribe>(e.Data);
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

                var list = await AnalyzePicture(imageData, newImage);
                newImage.СonstituentСolors = list;
                user.UserImages.Add(newImage);
                await _applicationContext.SaveChangesAsync();
            }
            await _applicationContext.SaveChangesAsync();
            string message = $"{files.Count} upload on server!";
            return Json(message);
        }
        public async Task<List<СonstituentСolor>> AnalyzePicture(byte[] page, UserImage newImage)
        {
            List<СonstituentСolor> result = new();
            await Task.Factory.StartNew(() =>
            {
                Mat img = new();
                CvInvoke.Imdecode(page, ImreadModes.Unchanged, img);
                Image<Bgr, Byte> src = img.ToImage<Bgr, Byte>();
                CvInvoke.Resize(src, src, new System.Drawing.Size { Height = 600, Width = 600 });
                Matrix<float> samples = new Matrix<float>(src.Rows * src.Cols, 1, 3);
                Matrix<int> finalClusters = new Matrix<int>(src.Rows * src.Cols, 1);
                Mat centers2 = new();
                List<int> clusters = new();
                Dictionary<int, int> clustersPairs = new();
                List<string> hexCollection = new();
                List<float> hex = new();
                MCvTermCriteria term = new MCvTermCriteria(100, 0.5);
                term.Type = TermCritType.Iter | TermCritType.Eps;

                for (int y = 0; y < src.Rows; y++)
                {
                    for (int x = 0; x < src.Cols; x++)
                    {
                        samples.Data[y + x * src.Rows, 0] = (float)src[y, x].Blue;
                        samples.Data[y + x * src.Rows, 1] = (float)src[y, x].Green;
                        samples.Data[y + x * src.Rows, 2] = (float)src[y, x].Red;
                    }
                }

                CvInvoke.Kmeans(samples, 8, finalClusters, term, 10, KMeansInitType.PPCenters, centers2);

                for (int y = 0; y < src.Rows; y++)
                {
                    for (int x = 0; x < src.Cols; x++)
                    {
                        clusters.Add(finalClusters[y + x * src.Rows, 0]);
                    }
                }

                foreach (var t in clusters.Distinct())
                {
                    clustersPairs.Add(t, clusters.Count(r => r == t));
                }

                float[,] rgbCentersResults = centers2.GetData() as float[,];
                for (int i = 0; i < centers2.Rows; i++)
                {
                    for (int j = 0; j < centers2.Cols; j++)
                    {
                        hex.Add(rgbCentersResults[i, j]);
                    }
                    hexCollection.Add(string.Format("#{0:X2}{1:X2}{2:X2}", (int)hex[2], (int)hex[1], (int)hex[0]));
                    hex.Clear();
                }

                foreach (var t in clustersPairs.OrderBy(x => x.Key).ToDictionary(pair => pair.Key, pair => pair.Value))
                {
                    result.Add(new СonstituentСolor { HexColor = hexCollection[t.Key], ValueCount = t.Value, Image = newImage });
                }
            });
            return result;
        }
    }
}
