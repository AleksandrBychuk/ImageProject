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
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
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
            (int, int, decimal, int, int, decimal, decimal) coords = (0, 0, 0, 0, 0, 0, 0);
            foreach (var file in files)
            {
                byte[] imageData = null;
                using (var binaryReader = new BinaryReader(file.OpenReadStream()))
                {
                    imageData = binaryReader.ReadBytes((int)file.Length);
                }
                using (var ms = new MemoryStream(imageData))
                {
                    coords = ExtractLocation(new Bitmap(ms));
                    using (Image img = Image.FromStream(ms))
                    {
                        int h = 600;
                        int w = 600;

                        using (Bitmap b = new Bitmap(NormalizeOrientation(img), new Size(w, h)))
                        {
                            using (MemoryStream ms2 = new MemoryStream())
                            {
                                b.Save(ms2, System.Drawing.Imaging.ImageFormat.Jpeg);
                                imageData = ms2.ToArray();
                            }
                        }
                    }
                }
                var newImage = new UserImage { DateAdded = DateTime.Now, Image = imageData, UserOwner = user };
                var list = await AnalyzePicture(imageData, newImage);
                newImage.СonstituentСolors = list.Item1;
                newImage.GreenPercent = (int)list.Item2;
                newImage.Coords = new ImageCoord
                {
                    Image = newImage,
                    ImageId = newImage.Id,
                    LatitudeDegree = coords.Item1,
                    LatitudeMinute = coords.Item2,
                    LatitudeSecond = coords.Item3,
                    LongitudeDegree = coords.Item4,
                    LongitudeMinute = coords.Item5,
                    LongitudeSecond = coords.Item6,
                    Altitude = coords.Item7
                };
                user.UserImages.Add(newImage);
            }
            await _applicationContext.SaveChangesAsync();
            string message = $"{files.Count} upload on server!";
            return Json(message);
        }
        public async Task<(List<СonstituentСolor>, float)> AnalyzePicture(byte[] page, UserImage newImage)
        {
            List<СonstituentСolor> result = new();
            float greenPercent = 0;
            await Task.Factory.StartNew(() =>
            {
                Mat img = new();
                CvInvoke.Imdecode(page, ImreadModes.Unchanged, img);
                Image<Bgr, Byte> src = img.ToImage<Bgr, Byte>().Resize(600, 600, Inter.Linear);
                Matrix<float> samples = new Matrix<float>(src.Rows * src.Cols, 1, 3);
                Matrix<int> finalClusters = new Matrix<int>(src.Rows * src.Cols, 1);
                Mat centers2 = new();
                List<int> clusters = new();
                Dictionary<int, int> clustersPairs = new();
                List<string> hexCollection = new();
                List<float> hex = new();
                MCvTermCriteria term = new MCvTermCriteria(100, 0.5);
                term.Type = TermCritType.Iter | TermCritType.Eps;
                int bc = 0, rc = 0, gc = 0;
                for (int y = 0; y < src.Rows; y++)
                {
                    for (int x = 0; x < src.Cols; x++)
                    {
                        samples.Data[y + x * src.Rows, 0] = (float)src[y, x].Blue;
                        samples.Data[y + x * src.Rows, 1] = (float)src[y, x].Green;
                        samples.Data[y + x * src.Rows, 2] = (float)src[y, x].Red;
                        if ((float)src[y, x].Green + 1f >= (float)src[y, x].Blue && (float)src[y, x].Green + 1f >= (float)src[y, x].Red)
                            greenPercent += 1f / ((float)src.Width * (float)src.Height) * 100f;
                        if ((float)src[y, x].Green < (float)src[y, x].Blue && (float)src[y, x].Green < (float)src[y, x].Red)
                            gc++;
                        if ((float)src[y, x].Green < (float)src[y, x].Blue)
                            bc++;
                        if ((float)src[y, x].Green < (float)src[y, x].Red)
                            rc++;
                        //float result = (2f * ((float)src[y, x].Green - (float)src[y, x].Red - (float)src[y, x].Blue)) / (2f * ((float)src[y, x].Green + (float)src[y, x].Red + (float)src[y, x].Blue));
                        //float result = (2f * (float)src[y, x].Green - (float)src[y, x].Red - (float)src[y, x].Blue) / (2f * (float)src[y, x].Green + (float)src[y, x].Red + (float)src[y, x].Blue);
                        //greenPercent += ((float)Math.Pow((float)src[y, x].Green, 2) - (float)Math.Pow((float)src[y, x].Red, 2)) / ((float)Math.Pow((float)src[y, x].Green, 2) + (float)Math.Pow((float)src[y, x].Red, 2));
                        //float R = (float)src[y, x].Red;
                        //float G = (float)src[y, x].Green;
                        //float B = (float)src[y, x].Blue;
                        ////float vari = (G - R) / (G + R - B);
                        //float grvi = (G - R) / (G + R);
                        ////float mgrvi = ((float)Math.Pow(G,2) - (float)Math.Pow(R, 2)) / ((float)Math.Pow(G, 2) + (float)Math.Pow(R, 2));
                        //float rgbvi = ((float)Math.Pow(G, 2) - B * R) / ((float)Math.Pow(G, 2) + B * R);
                        ////float cive = 0.441f * R - 0.881f * G + 0.385f * B + 18.7875f; // такое себе решение
                        //float gla = (2f * (float)src[y, x].Green - (float)src[y, x].Red - (float)src[y, x].Blue) / (2f * (float)src[y, x].Green + (float)src[y, x].Red + (float)src[y, x].Blue);
                        //if (gla > 0f)
                        //    greenPercent += grvi;
                        //float R = (float)src[y, x].Red;
                        //float G = (float)src[y, x].Green;
                        //float B = (float)src[y, x].Blue;
                        //float vari = (G - R) / (G + R - B);
                        //float rgbvi = ((float)Math.Pow(G, 2) - B * R) / ((float)Math.Pow(G, 2) + B * R);
                        //float gla = (2f * (float)src[y, x].Green - (float)src[y, x].Red - (float)src[y, x].Blue) / (2f * (float)src[y, x].Green + (float)src[y, x].Red + (float)src[y, x].Blue);
                        //float lai = 3.9941f * vari + 4.8813f * rgbvi + 0.0122f * B + 6.0529f * gla + 1.2818f;
                        //if (lai > 0)
                        //    greenPercent += lai;
                    }
                }
                CvInvoke.Kmeans(samples, 9, finalClusters, term, 10, KMeansInitType.PPCenters, centers2);

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
            return (result, greenPercent);
        }

        private (int, int, decimal, int, int, decimal, decimal) ExtractLocation(Bitmap image)
        {
            (int, int, decimal) latitude = (0, 0, 0);
            (int, int, decimal) longitude = (0, 0, 0);
            decimal altitude = 0;
            if (Array.IndexOf<int>(image.PropertyIdList, 1) != -1 &&
                Array.IndexOf<int>(image.PropertyIdList, 2) != -1 &&
                Array.IndexOf<int>(image.PropertyIdList, 3) != -1 &&
                Array.IndexOf<int>(image.PropertyIdList, 4) != -1)
            {
                latitude = DecodeRational64u(image.GetPropertyItem(2));
                longitude = DecodeRational64u(image.GetPropertyItem(4));
                altitude = (decimal)BitConverter.ToUInt32(image.GetPropertyItem(6).Value, 0) / (decimal)BitConverter.ToUInt32(image.GetPropertyItem(6).Value, 4);
            }
            return (latitude.Item1, latitude.Item2, latitude.Item3, longitude.Item1, longitude.Item2, longitude.Item3, altitude);
        }

        private (int, int, decimal) DecodeRational64u(System.Drawing.Imaging.PropertyItem propertyItem)
        {
            uint dN = BitConverter.ToUInt32(propertyItem.Value, 0);
            uint dD = BitConverter.ToUInt32(propertyItem.Value, 4);
            uint mN = BitConverter.ToUInt32(propertyItem.Value, 8);
            uint mD = BitConverter.ToUInt32(propertyItem.Value, 12);
            uint sN = BitConverter.ToUInt32(propertyItem.Value, 16);
            uint sD = BitConverter.ToUInt32(propertyItem.Value, 20);

            decimal deg;
            decimal min;
            decimal sec;
            
            if (dD > 0) { deg = (decimal)dN / dD; } else { deg = dN; }
            if (mD > 0) { min = (decimal)mN / mD; } else { min = mN; }
            if (sD > 0) { sec = (decimal)sN / sD; } else { sec = sN; }

            if (sec == 0) return ((int)deg, (int)min, 0);
            else return ((int)deg, (int)min, sec);
        }

        public Image NormalizeOrientation(Image img)
        {
            if (Array.IndexOf(img.PropertyIdList, 274) > -1)
            {
                var orientation = (int)img.GetPropertyItem(274).Value[0];
                switch (orientation)
                {
                    case 1:
                        // No rotation required.
                        break;
                    case 2:
                        img.RotateFlip(RotateFlipType.RotateNoneFlipX);
                        break;
                    case 3:
                        img.RotateFlip(RotateFlipType.Rotate180FlipNone);
                        break;
                    case 4:
                        img.RotateFlip(RotateFlipType.Rotate180FlipX);
                        break;
                    case 5:
                        img.RotateFlip(RotateFlipType.Rotate90FlipX);
                        break;
                    case 6:
                        img.RotateFlip(RotateFlipType.Rotate90FlipNone);
                        break;
                    case 7:
                        img.RotateFlip(RotateFlipType.Rotate270FlipX);
                        break;
                    case 8:
                        img.RotateFlip(RotateFlipType.Rotate270FlipNone);
                        break;
                }
                // This EXIF data is now invalid and should be removed.
                img.RemovePropertyItem(274);
            }
            return img;
        }
    }
}
